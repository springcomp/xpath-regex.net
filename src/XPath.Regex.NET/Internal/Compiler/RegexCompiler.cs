using System.Collections.Generic;
using System.Collections.Immutable;
using XPath.Regex.NET.Internal.Ast;
using XPath.Regex.NET.Internal.Nfa;
using XPath.Regex.NET.Internal.Parser;
using XPath.Regex.NET.Internal.Unicode;

namespace XPath.Regex.NET.Internal.Compiler;

internal static class RegexCompiler
{
    public static NfaProgram Compile(
        AstNode root,
        int captureCount,
        RegexDialect dialect,
        RegexFlags flags,
        RegexCompileOptions? options = null)
    {
        var cursor = new CompilerCursor((options ?? RegexCompileOptions.Default).MaxProgramInstructions);
        var compiler = new AstCompiler(cursor, dialect, flags);

        if (dialect == RegexDialect.Xsd)
            cursor.Emit(new AnchorStartInstruction());

        if (dialect != RegexDialect.Xsd && !MetadataAnalyzer.ContainsBackReferences(root))
            EmitUnanchoredPrefix(cursor);

        cursor.Emit(new SaveInstruction(0));

        CompileFragment body = compiler.CompileNode(root);
        int overallEnd = cursor.Emit(new SaveInstruction(1));
        cursor.Patch(body.Outs, overallEnd);

        if (dialect == RegexDialect.Xsd)
            cursor.Emit(new AnchorEndInstruction());

        cursor.Emit(new MatchInstruction());

        string? requiredPrefixHint = PrefixBuilder.TryExtractPrefix(root);
        Dictionary<int, int> groupMinLengths = MetadataAnalyzer.ComputeGroupMinLengths(root);
        bool canMatchEmpty = MetadataAnalyzer.CanMatchEmpty(cursor.Instructions, groupMinLengths);
        int minMatchLength = MetadataAnalyzer.ComputeMinMatchLength(cursor.Instructions, groupMinLengths);

        ImmutableArray<ImmutableArray<int>> groupChildren = BuildGroupChildren(root, captureCount);

        return cursor.BuildProgram(captureCount, canMatchEmpty, minMatchLength, requiredPrefixHint, groupChildren);
    }

    private static void EmitUnanchoredPrefix(CompilerCursor cursor)
    {
        int split = cursor.Emit(new SplitInstruction(-1, -1));
        int scan = cursor.Emit(new CharInstruction(ImmutableArray.Create((0, UnicodeConstants.MaxCodePoint))));
        cursor.Emit(new JmpInstruction(split));
        cursor.Patch(PatchList.ForSplitFirst(split), cursor.Current);
        cursor.Patch(PatchList.ForSplitSecond(split), scan);
    }

    /// <summary>
    /// Builds the group-children map from the AST.
    /// Index 0 = top-level groups; index N = children directly enclosed by group N.
    /// </summary>
    private static ImmutableArray<ImmutableArray<int>> BuildGroupChildren(AstNode root, int captureCount)
    {
        // parentToChildren[N] = groups directly nested inside group N (0 = top-level).
        var parentToChildren = new List<int>[captureCount + 1];
        for (int i = 0; i <= captureCount; i++)
            parentToChildren[i] = new List<int>();

        WalkCaptures(root, parentGroup: 0, parentToChildren);

        var builder = ImmutableArray.CreateBuilder<ImmutableArray<int>>(captureCount + 1);
        for (int i = 0; i <= captureCount; i++)
            builder.Add(parentToChildren[i].ToImmutableArray());

        return builder.ToImmutable();
    }

    private static void WalkCaptures(AstNode node, int parentGroup, List<int>[] map)
    {
        switch (node)
        {
            case CaptureNode capture:
                map[parentGroup].Add(capture.GroupNumber);
                WalkCaptures(capture.Child, capture.GroupNumber, map);
                break;
            case ConcatNode concat:
                foreach (AstNode child in concat.Children)
                    WalkCaptures(child, parentGroup, map);
                break;
            case AlternationNode alt:
                foreach (AstNode child in alt.Alternatives)
                    WalkCaptures(child, parentGroup, map);
                break;
            case RepeatNode repeat:
                WalkCaptures(repeat.Child, parentGroup, map);
                break;
            case NonCaptureNode nonCap:
                WalkCaptures(nonCap.Child, parentGroup, map);
                break;
            // Terminals: LiteralNode, WildcardNode, CharClassNode, AnchorStartNode, AnchorEndNode, BackReferenceNode
            default:
                break;
        }
    }

    private readonly struct CompileFragment
    {
        public CompileFragment(int entry, PatchList outs)
        {
            Entry = entry;
            Outs = outs;
        }

        public int Entry { get; }

        public PatchList Outs { get; }
    }

    private sealed class AstCompiler
    {
        private readonly CompilerCursor _cursor;
        private readonly RegexDialect _dialect;
        private readonly RegexFlags _flags;

        public AstCompiler(CompilerCursor cursor, RegexDialect dialect, RegexFlags flags)
        {
            _cursor = cursor;
            _dialect = dialect;
            _flags = flags;
        }

        public CompileFragment CompileNode(AstNode node)
        {
            return node switch
            {
                LiteralNode literal => CompileLiteral(literal),
                WildcardNode => CompileWildcard(),
                CharClassNode charClass => CompileCharClass(charClass),
                AnchorStartNode => CompileAnchorStart(),
                AnchorEndNode => CompileAnchorEnd(),
                BackReferenceNode backRef => CompileBackReference(backRef),
                AlternationNode alt => CompileAlternation(alt),
                ConcatNode concat => CompileConcat(concat),
                RepeatNode repeat => CompileRepeat(repeat),
                CaptureNode capture => CompileCapture(capture),
                NonCaptureNode nonCap => CompileNonCapture(nonCap),
                _ => throw new InvalidOperationException($"Unknown AST node type {node.GetType().Name}"),
            };
        }

        private CompileFragment CompileLiteral(LiteralNode node)
        {
            ImmutableArray<(int Lo, int Hi)> ranges = ImmutableArray.Create((node.CodePoint, node.CodePoint));

            if (_flags.IgnoreCase)
                ranges = UnicodeAdapter.ApplyCaseClosure(ranges);

            int pc = _cursor.Emit(new CharInstruction(ranges));
            return new CompileFragment(pc, PatchList.Empty);
        }

        private CompileFragment CompileWildcard()
        {
            ImmutableArray<(int Lo, int Hi)> ranges = _flags.DotAll
                ? ImmutableArray.Create((0, UnicodeConstants.MaxCodePoint))
                : ImmutableArray.Create((0, 0x09), (0x0B, 0x0C), (0x0E, UnicodeConstants.MaxCodePoint));

            int pc = _cursor.Emit(new CharInstruction(ranges));
            return new CompileFragment(pc, PatchList.Empty);
        }

        private CompileFragment CompileCharClass(CharClassNode node)
        {
            ImmutableArray<(int Lo, int Hi)> ranges = node.Ranges;

            // Apply case closure before negation
            if (_flags.IgnoreCase)
                ranges = UnicodeAdapter.ApplyCaseClosure(ranges);

            if (node.Negated)
                ranges = AstBuilderHelpers.SubtractRanges(ImmutableArray.Create((0, UnicodeConstants.MaxCodePoint)), ranges);

            int pc = _cursor.Emit(new CharInstruction(ranges));
            return new CompileFragment(pc, PatchList.Empty);
        }

        private CompileFragment CompileAnchorStart()
        {
            int pc = _cursor.Emit(new AnchorStartInstruction());
            return new CompileFragment(pc, PatchList.Empty);
        }

        private CompileFragment CompileAnchorEnd()
        {
            int pc = _cursor.Emit(new AnchorEndInstruction());
            return new CompileFragment(pc, PatchList.Empty);
        }

        private CompileFragment CompileBackReference(BackReferenceNode node)
        {
            int pc = _cursor.Emit(new BackRefInstruction(node.GroupNumber, _flags.IgnoreCase));
            return new CompileFragment(pc, PatchList.Empty);
        }

        private CompileFragment CompileAlternation(AlternationNode node)
        {
            if (node.Alternatives.Length == 0)
                return new CompileFragment(_cursor.Current, PatchList.Empty);

            if (node.Alternatives.Length == 1)
                return CompileNode(node.Alternatives[0]);

            int entrySplit = _cursor.Emit(new SplitInstruction(-1, -1));
            int currentSplit = entrySplit;
            PatchList outs = PatchList.Empty;

            for (int i = 0; i < node.Alternatives.Length; i++)
            {
                bool last = i == node.Alternatives.Length - 1;

                if (last)
                {
                    // Last alternative: patch previous split's Second directly
                    // to this alternative's body (no extra Split needed).
                    int alternativeStart = _cursor.Current;
                    _cursor.Patch(PatchList.ForSplitSecond(currentSplit), alternativeStart);

                    CompileFragment alternative = CompileNode(node.Alternatives[i]);
                    int exitJmp = _cursor.Emit(new JmpInstruction(-1));
                    outs = outs.Concat(alternative.Outs).Concat(PatchList.ForJmp(exitJmp));
                }
                else
                {
                    int alternativeStart = _cursor.Current;
                    _cursor.Patch(PatchList.ForSplitFirst(currentSplit), alternativeStart);

                    CompileFragment alternative = CompileNode(node.Alternatives[i]);
                    int exitJmp = _cursor.Emit(new JmpInstruction(-1));
                    outs = outs.Concat(alternative.Outs).Concat(PatchList.ForJmp(exitJmp));

                    int nextSplit = _cursor.Emit(new SplitInstruction(-1, -1));
                    _cursor.Patch(PatchList.ForSplitSecond(currentSplit), nextSplit);
                    currentSplit = nextSplit;
                }
            }

            return new CompileFragment(entrySplit, outs);
        }

        private CompileFragment CompileConcat(ConcatNode node)
        {
            CompileFragment? current = null;

            foreach (AstNode child in node.Children)
            {
                CompileFragment next = CompileNode(child);
                current = ConcatFragments(current, next);
            }

            return current ?? new CompileFragment(_cursor.Current, PatchList.Empty);
        }

        private CompileFragment CompileRepeat(RepeatNode node)
        {
            CompileFragment? fragment = null;
            for (int i = 0; i < node.Min; i++)
                fragment = ConcatFragments(fragment, CompileNode(node.Child));

            if (node.Max == node.Min)
                return fragment ?? new CompileFragment(_cursor.Current, PatchList.Empty);

            if (node.Max is null)
            {
                CompileFragment zeroOrMore = CompileZeroOrMore(node.Child, node.Greedy);
                return ConcatFragments(fragment, zeroOrMore);
            }

            int optionalCount = node.Max.Value - node.Min;
            CompileFragment current = fragment ?? new CompileFragment(_cursor.Current, PatchList.Empty);

            for (int i = 0; i < optionalCount; i++)
            {
                CompileFragment optional = CompileOptional(node.Child, node.Greedy);
                current = ConcatFragments(current, optional);
            }

            return current;
        }

        private CompileFragment CompileOptional(AstNode child, bool greedy)
        {
            int splitAddr = _cursor.Emit(new SplitInstruction(-1, -1));
            CompileFragment body = CompileNode(child);

            PatchList exitPatch;
            if (greedy)
            {
                _cursor.Patch(PatchList.ForSplitFirst(splitAddr), body.Entry);
                exitPatch = PatchList.ForSplitSecond(splitAddr);
            }
            else
            {
                _cursor.Patch(PatchList.ForSplitSecond(splitAddr), body.Entry);
                exitPatch = PatchList.ForSplitFirst(splitAddr);
            }

            PatchList outs = body.Outs.Concat(exitPatch);
            return new CompileFragment(splitAddr, outs);
        }

        private CompileFragment CompileZeroOrMore(AstNode child, bool greedy)
        {
            int splitAddr = _cursor.Emit(new SplitInstruction(-1, -1));
            CompileFragment body = CompileNode(child);

            PatchList exitPatch;
            if (greedy)
            {
                _cursor.Patch(PatchList.ForSplitFirst(splitAddr), body.Entry);
                exitPatch = PatchList.ForSplitSecond(splitAddr);
            }
            else
            {
                _cursor.Patch(PatchList.ForSplitSecond(splitAddr), body.Entry);
                exitPatch = PatchList.ForSplitFirst(splitAddr);
            }

            _cursor.Patch(body.Outs, splitAddr);
            _cursor.Emit(new JmpInstruction(splitAddr));

            return new CompileFragment(splitAddr, exitPatch);
        }

        private CompileFragment CompileCapture(CaptureNode node)
        {
            int startSlot = 2 * node.GroupNumber;
            int entry = _cursor.Emit(new SaveInstruction(startSlot));
            CompileFragment child = CompileNode(node.Child);
            int endSave = _cursor.Emit(new SaveInstruction(startSlot + 1));
            _cursor.Patch(child.Outs, endSave);
            return new CompileFragment(entry, PatchList.Empty);
        }

        private CompileFragment CompileNonCapture(NonCaptureNode node) => CompileNode(node.Child);

        private CompileFragment ConcatFragments(CompileFragment? left, CompileFragment right)
        {
            if (left is null)
                return right;

            _cursor.Patch(left.Value.Outs, right.Entry);
            return new CompileFragment(left.Value.Entry, right.Outs);
        }
    }
}
