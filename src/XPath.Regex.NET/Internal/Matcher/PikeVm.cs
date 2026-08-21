using System.Diagnostics;
using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Nfa;
using XPath.Regex.NET.Internal.Unicode;

namespace XPath.Regex.NET.Internal.Matcher;

internal static class PikeVm
{
    // Fails fast after 5 seconds so pathological patterns or oversized inputs
    // cannot monopolize a matching thread.
    private const int TimeoutMs = 5000;
    // Bounds live VM state to keep pathological patterns from exhausting memory.
    private const int MaxThreadCount = 100000;
    public static bool IsMatch(NfaProgram program, RegexFlags flags, RegexDialect dialect, string input) =>
        Match(program, flags, dialect, input) is not null;

    public static MatchContext? Match(NfaProgram program, RegexFlags flags, RegexDialect dialect, string input)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(input);

        foreach (MatchContext match in Matches(program, flags, dialect, input))
            return match;

        return null;
    }

    public static IEnumerable<MatchContext> Matches(NfaProgram program, RegexFlags flags, RegexDialect dialect, string input)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(input);

        int slotCount = (program.CaptureCount + 1) * 2;
        int programLength = program.Instructions.Length;

        var rentals = new List<int[]>(16);
        var current = new ThreadList(programLength, program.HasBackReferences, slotCount);
        var next = new ThreadList(programLength, program.HasBackReferences, slotCount);
        // Reused across every AddThread call in this enumeration (calls are
        // synchronous/non-overlapping) to avoid a fresh allocation per epsilon expansion.
        var epsilonStack = new Stack<(int Pc, int Position, int[] Captures)>(32);
        bool singlePassSearch = dialect != RegexDialect.Xsd && !program.HasBackReferences;
        var timeout = Stopwatch.StartNew();

        int startPos = 0;
        while (startPos <= input.Length)
        {
            if (timeout.ElapsedMilliseconds > TimeoutMs)
                throw new RegexEngineLimitExceededException(
                    RegexEngineLimit.TimeoutMs,
                    $"Regex matching timeout ({TimeoutMs}ms exceeded): pattern too complex or input too large.");

            // Prefix hints are used only on restart-based searches. Regular XPath
            // programs already use one VM scan, while OrdinalIgnoreCase cannot
            // guarantee the VM's Unicode SimpleFold semantics.
            if (!singlePassSearch && dialect != RegexDialect.Xsd && !flags.IgnoreCase &&
                program.RequiredPrefixHint is { Length: > 0 } prefix)
            {
                int prefixPosition = input.IndexOf(prefix, startPos, StringComparison.Ordinal);
                if (prefixPosition < 0)
                    break;
                startPos = prefixPosition;
            }

            if (input.Length - startPos < program.MinMatchLength)
                break;

#if REGEX_INSTRUMENTATION
      if (VmInstrumentation.Current is { } candidateMetrics)
        candidateMetrics.CandidateStarts++;
#endif

            rentals.Clear();
            current.Clear();
            next.Clear();

            int[] initialCaptures = CaptureSet.Rent(slotCount, rentals);
            initialCaptures[0] = startPos;
            AddThread(program, flags, current, 0, initialCaptures, input, startPos, rentals, epsilonStack);

            MatchContext? found = Run(program, flags, input, startPos, current, next, rentals, epsilonStack);

            if (found is not null)
            {
                int[] capturesCopy = (int[])found.Captures.Clone();
                yield return new MatchContext(found.Start, found.EndExclusive, capturesCopy);
                startPos = found.EndExclusive == found.Start ? found.Start + 1 : found.EndExclusive;
            }
            else
            {
                if (dialect == RegexDialect.Xsd)
                {
                    CaptureSet.ReturnAll(rentals);
                    yield break;
                }

                if (singlePassSearch)
                {
                    CaptureSet.ReturnAll(rentals);
                    yield break;
                }

                startPos++;
            }

            CaptureSet.ReturnAll(rentals);
        }
    }

    private static MatchContext? Run(
        NfaProgram program,
        RegexFlags flags,
        string input,
        int startPos,
        ThreadList current,
        ThreadList next,
        List<int[]> rentals,
        Stack<(int Pc, int Position, int[] Captures)> epsilonStack)
    {
        ReadOnlySpan<char> span = input.AsSpan();
        MatchContext? found = null;

#if REGEX_INSTRUMENTATION
    if (VmInstrumentation.Current is { } runMetrics)
      runMetrics.RunExecutions++;
#endif

        while (true)
        {
            foreach (Thread thread in current.Threads)
            {
#if REGEX_INSTRUMENTATION
        if (VmInstrumentation.Current is { } dispatchMetrics)
          dispatchMetrics.ThreadDispatches++;
#endif
                NfaInstruction inst = program.Instructions[thread.Pc];
                int pos = thread.Position;

                switch (inst)
                {
                    case MatchInstruction:
                        {
                            // Capture arrays are shared between threads until a save changes
                            // one slot. Clone before recording overall match end, otherwise
                            // this write corrupts capture state of other live threads.
                            int[] captures = CaptureSet.Clone(thread.Captures, rentals);
                            captures[1] = pos;

                            // Overall match end is not a capture-group end. Keep it out of
                            // backreference state while VM explores other paths.
                            // Overwrite (not ??=): later steps from higher-priority
                            // consuming threads produce longer, better matches.
                            found = new MatchContext(captures[0], pos, captures);

                            // All remaining threads in current are lower priority;
                            // skip them (Pike VM leftmost-first guarantee).
                            goto doneWithCurrent;
                        }

                    case CharInstruction ch:
                        {
#if REGEX_INSTRUMENTATION
              if (VmInstrumentation.Current is { } characterMetrics)
                characterMetrics.CharacterTests++;
#endif
                            if (pos >= span.Length)
                                break;

                            int codePoint = ReadCodePoint(span, pos, out int width);
                            if (!IsInRanges(codePoint, ch.Ranges))
                                break;

                            AddThread(program, flags, next, thread.Pc + 1, thread.Captures, input, pos + width, rentals, epsilonStack);
                            break;
                        }

                    case BackRefInstruction back:
                        {
#if REGEX_INSTRUMENTATION
              if (VmInstrumentation.Current is { } backReferenceMetrics)
                backReferenceMetrics.BackReferenceTests++;
#endif
                            int[] captures = thread.Captures;
                            int start = captures[2 * back.GroupNumber];
                            int end = captures[2 * back.GroupNumber + 1];
                            if (start < 0 || end < start)
                                break;

                            int len = end - start;
                            if (pos + len > span.Length)
                                break;

                            if (!BackRefEquals(span, start, pos, len, back.IgnoreCase))
                                break;

                            AddThread(program, flags, next, thread.Pc + 1, captures, input, pos + len, rentals, epsilonStack);
                            break;
                        }
                }
            }

        doneWithCurrent:

            // Only return when no higher-priority threads can extend.
            // Threads already in next from earlier (higher-priority) consuming
            // instructions may still produce a longer match at a later step.
            if (next.Threads.Count == 0)
                return found;

            (current, next) = (next, current);
            next.Clear();
        }
    }

    private static void AddThread(
        NfaProgram program,
        RegexFlags flags,
        ThreadList list,
        int pc,
        int[] captures,
        string input,
        int position,
        List<int[]> rentals,
        Stack<(int Pc, int Position, int[] Captures)> stack)
    {
        // Allows realistic nested epsilon paths while failing before stack growth
        // can overflow on pathological patterns.
        const int MaxStackDepth = 10000;
        stack.Clear();
        stack.Push((pc, position, captures));

        int threadCountEstimate = 0;
        while (stack.Count > 0)
        {
#if REGEX_INSTRUMENTATION
      if (VmInstrumentation.Current is { } epsilonMetrics)
        epsilonMetrics.EpsilonExpansionSteps++;
#endif
            if (stack.Count > MaxStackDepth)
                throw new RegexEngineLimitExceededException(
                    RegexEngineLimit.MaxStackDepth,
                    "Pattern is too complex (epsilon expansion depth exceeds " + MaxStackDepth + ").");

            if (++threadCountEstimate > MaxThreadCount)
                throw new RegexEngineLimitExceededException(
                    RegexEngineLimit.MaxThreadCount,
                    "Pattern creates too many threads (probable catastrophic backtracking or huge quantifier).");

            (int curPc, int curPos, int[] curCaps) = stack.Pop();
            if (curPc < 0 || curPc >= program.Instructions.Length)
                continue;

            if (!list.TryAdd(new Thread(curPc, curPos, curCaps)))
                continue;

            switch (program.Instructions[curPc])
            {
                case SplitInstruction split:
                    stack.Push((split.Second, curPos, curCaps));
                    stack.Push((split.First, curPos, curCaps));
                    break;

                case JmpInstruction jmp:
                    stack.Push((jmp.Target, curPos, curCaps));
                    break;

                case SaveInstruction save:
                    {
                        int[] nextCaps = curCaps;
                        if (nextCaps[save.Slot] != curPos)
                        {
                            nextCaps = CaptureSet.Clone(curCaps, rentals);
                            nextCaps[save.Slot] = curPos;
                        }

                        stack.Push((curPc + 1, curPos, nextCaps));
                        break;
                    }

                case AnchorStartInstruction:
                    if (AnchorEvaluator.IsStart(input, curPos, flags.MultiLine))
                        stack.Push((curPc + 1, curPos, curCaps));
                    break;

                case AnchorEndInstruction:
                    if (AnchorEvaluator.IsEnd(input, curPos, flags.MultiLine))
                        stack.Push((curPc + 1, curPos, curCaps));
                    break;

                case MatchInstruction:
                    break;

                case CharInstruction:
                case BackRefInstruction:
                    break;

                default:
                    throw new InvalidOperationException($"Unknown instruction {program.Instructions[curPc].GetType().Name} at {curPc}.");
            }
        }
    }

    private static int ReadCodePoint(ReadOnlySpan<char> input, int index, out int width)
    {
#if REGEX_INSTRUMENTATION
    if (VmInstrumentation.Current is { } codePointMetrics)
      codePointMetrics.CodePointReads++;
#endif
        char first = input[index];
        if (char.IsHighSurrogate(first) && index + 1 < input.Length && char.IsLowSurrogate(input[index + 1]))
        {
            width = 2;
            return char.ConvertToUtf32(first, input[index + 1]);
        }

        width = 1;
        return first;
    }

    private static bool IsInRanges(int codePoint, System.Collections.Immutable.ImmutableArray<(int Lo, int Hi)> ranges)
    {
        int left = 0;
        int right = ranges.Length - 1;
        while (left <= right)
        {
            int mid = (left + right) >> 1;
            (int lo, int hi) = ranges[mid];
            if (codePoint < lo)
            {
                right = mid - 1;
            }
            else if (codePoint > hi)
            {
                left = mid + 1;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    private static bool BackRefEquals(ReadOnlySpan<char> input, int aStart, int bStart, int len, bool ignoreCase)
    {
        ReadOnlySpan<char> a = input.Slice(aStart, len);
        ReadOnlySpan<char> b = input.Slice(bStart, len);

        if (!ignoreCase)
            return a.SequenceEqual(b);

        int i = 0;
        int j = 0;

        while (i < a.Length && j < b.Length)
        {
            int cpA = UnicodeAdapter.SimpleFold(ReadCodePoint(a, i, out int wA));
            int cpB = UnicodeAdapter.SimpleFold(ReadCodePoint(b, j, out int wB));

            if (cpA != cpB)
                return false;

            i += wA;
            j += wB;
        }

        return i == a.Length && j == b.Length;
    }
}
