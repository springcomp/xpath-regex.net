using System.Collections.Generic;
using XPath.Regex.NET.Internal.Ast;
using XPath.Regex.NET.Internal.Nfa;

namespace XPath.Regex.NET.Internal.Compiler;

internal static class MetadataAnalyzer
{
    public static bool ContainsBackReferences(AstNode root)
    {
        return root switch
        {
            BackReferenceNode => true,
            AlternationNode alt => alt.Alternatives.Any(ContainsBackReferences),
            ConcatNode concat => concat.Children.Any(ContainsBackReferences),
            RepeatNode repeat => ContainsBackReferences(repeat.Child),
            CaptureNode capture => ContainsBackReferences(capture.Child),
            NonCaptureNode nonCap => ContainsBackReferences(nonCap.Child),
            _ => false,
        };
    }

    public static bool CanMatchEmpty(IReadOnlyList<NfaInstruction> instructions, IReadOnlyDictionary<int, int> groupMinLengths)
    {
        var queue = new Queue<int>();
        var visited = new HashSet<int>();
        queue.Enqueue(0);

        while (queue.Count > 0)
        {
            int pc = queue.Dequeue();
            if (!visited.Add(pc))
                continue;

            if (pc < 0 || pc >= instructions.Count)
                continue;

            switch (instructions[pc])
            {
                case MatchInstruction:
                    return true;
                case SplitInstruction split:
                    queue.Enqueue(split.First);
                    queue.Enqueue(split.Second);
                    break;
                case JmpInstruction jmp:
                    queue.Enqueue(jmp.Target);
                    break;
                case SaveInstruction:
                case AnchorStartInstruction:
                case AnchorEndInstruction:
                    queue.Enqueue(pc + 1);
                    break;
                case BackRefInstruction backRef:
                    if (groupMinLengths.TryGetValue(backRef.GroupNumber, out int min) && min == 0)
                        queue.Enqueue(pc + 1);
                    break;
                case CharInstruction:
                    break;
                default:
                    throw new InvalidOperationException($"Unknown instruction {instructions[pc].GetType().Name} at {pc}.");
            }
        }

        return false;
    }

    public static int ComputeMinMatchLength(IReadOnlyList<NfaInstruction> instructions, IReadOnlyDictionary<int, int> groupMinLengths)
    {
        var distances = new Dictionary<int, int>();
        var queue = new PriorityQueue<int, int>();
        queue.Enqueue(0, 0);

        while (queue.Count > 0)
        {
            queue.TryDequeue(out int pc, out int distance);
            if (pc < 0 || pc >= instructions.Count)
                continue;

            if (distances.TryGetValue(pc, out int existing) && existing <= distance)
                continue;

            distances[pc] = distance;

            NfaInstruction inst = instructions[pc];
            switch (inst)
            {
                case MatchInstruction:
                    return distance;
                case SplitInstruction split:
                    queue.Enqueue(split.First, distance);
                    queue.Enqueue(split.Second, distance);
                    break;
                case JmpInstruction jmp:
                    queue.Enqueue(jmp.Target, distance);
                    break;
                case SaveInstruction:
                case AnchorStartInstruction:
                case AnchorEndInstruction:
                    queue.Enqueue(pc + 1, distance);
                    break;
                case BackRefInstruction backRef:
                    int backrefCost = groupMinLengths.TryGetValue(backRef.GroupNumber, out int min) ? min : 0;
                    queue.Enqueue(pc + 1, distance + backrefCost);
                    break;
                case CharInstruction:
                    queue.Enqueue(pc + 1, distance + 1);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown instruction {inst.GetType().Name} at {pc}.");
            }
        }

        return int.MaxValue;
    }

    public static Dictionary<int, int> ComputeGroupMinLengths(AstNode root)
    {
        var map = new Dictionary<int, int>();
        ComputeMinLengths(root, map);
        return map;
    }

    private static int ComputeMinLengths(AstNode node, Dictionary<int, int> map)
    {
        return node switch
        {
            LiteralNode => 1,
            WildcardNode => 1,
            CharClassNode => 1,
            AnchorStartNode => 0,
            AnchorEndNode => 0,
            BackReferenceNode backRef => map.TryGetValue(backRef.GroupNumber, out int len) ? len : 0,
            AlternationNode alt => ComputeMinAlt(alt, map),
            ConcatNode concat => ComputeMinConcat(concat, map),
            RepeatNode repeat => ComputeMinRepeat(repeat, map),
            CaptureNode capture => ComputeMinCapture(capture, map),
            NonCaptureNode nonCap => ComputeMinLengths(nonCap.Child, map),
            _ => throw new InvalidOperationException($"Unknown AST node type {node.GetType().Name}"),
        };
    }

    private static int ComputeMinAlt(AlternationNode node, Dictionary<int, int> map)
    {
        int min = int.MaxValue;
        foreach (AstNode alt in node.Alternatives)
            min = Math.Min(min, ComputeMinLengths(alt, map));
        return min == int.MaxValue ? 0 : min;
    }

    private static int ComputeMinConcat(ConcatNode node, Dictionary<int, int> map)
    {
        int sum = 0;
        foreach (AstNode child in node.Children)
        {
            int childMin = ComputeMinLengths(child, map);
            if (childMin == int.MaxValue)
                return int.MaxValue;
            sum += childMin;
        }

        return sum;
    }

    private static int ComputeMinRepeat(RepeatNode node, Dictionary<int, int> map)
    {
        int childMin = ComputeMinLengths(node.Child, map);
        if (childMin == int.MaxValue)
            return int.MaxValue;

        // Min match length depends only on the lower repeat bound; the upper
        // bound (Max, null = unbounded) never affects the minimum.
        long product = (long)node.Min * childMin;
        return product >= int.MaxValue ? int.MaxValue : (int)product;
    }

    private static int ComputeMinCapture(CaptureNode node, Dictionary<int, int> map)
    {
        int inner = ComputeMinLengths(node.Child, map);
        map[node.GroupNumber] = inner;
        return inner;
    }
}
