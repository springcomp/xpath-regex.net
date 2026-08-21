using System.Collections.Immutable;
using XPath.Regex.NET.Internal.Ast;

namespace XPath.Regex.NET.Internal.Parser;

internal static class AstBuilderHelpers
{
    public static AstNode BuildAlternation(IReadOnlyList<AstNode> alternatives)
    {
        if (alternatives.Count == 1)
            return alternatives[0];

        return new AlternationNode(alternatives.ToImmutableArray());
    }

    public static AstNode BuildConcat(IReadOnlyList<AstNode> children)
    {
        if (children.Count == 1)
            return children[0];

        return new ConcatNode(children.ToImmutableArray());
    }

    public static ImmutableArray<(int Lo, int Hi)> NormalizeRanges(IReadOnlyList<(int Lo, int Hi)> ranges)
    {
        if (ranges.Count == 0)
            return ImmutableArray<(int Lo, int Hi)>.Empty;

        var sorted = new List<(int Lo, int Hi)>(ranges.Count);
        sorted.AddRange(ranges);
        sorted.Sort(static (left, right) => left.Lo.CompareTo(right.Lo));

        var merged = new List<(int Lo, int Hi)>(sorted.Count);
        (int Lo, int Hi) current = sorted[0];

        for (int i = 1; i < sorted.Count; i++)
        {
            (int Lo, int Hi) next = sorted[i];
            if (next.Lo <= current.Hi + 1)
            {
                current = (current.Lo, Math.Max(current.Hi, next.Hi));
                continue;
            }

            merged.Add(current);
            current = next;
        }

        merged.Add(current);
        return merged.ToImmutableArray();
    }

    public static ImmutableArray<(int Lo, int Hi)> SubtractRanges(
        ImmutableArray<(int Lo, int Hi)> left,
        ImmutableArray<(int Lo, int Hi)> right)
    {
        if (left.IsDefaultOrEmpty)
            return ImmutableArray<(int Lo, int Hi)>.Empty;

        if (right.IsDefaultOrEmpty)
            return left;

        var result = new List<(int Lo, int Hi)>(left.Length);

        foreach ((int leftLo, int leftHi) in left)
        {
            int cursor = leftLo;
            foreach ((int rightLo, int rightHi) in right)
            {
                if (rightHi < cursor)
                    continue;

                if (rightLo > leftHi)
                    break;

                if (rightLo > cursor)
                    result.Add((cursor, rightLo - 1));

                cursor = Math.Max(cursor, rightHi + 1);
                if (cursor > leftHi)
                    break;
            }

            if (cursor <= leftHi)
                result.Add((cursor, leftHi));
        }

        return NormalizeRanges(result);
    }

    public static ImmutableArray<(int Lo, int Hi)> RangesForSingleCodePoint(int codePoint) =>
        ImmutableArray.Create((codePoint, codePoint));

}
