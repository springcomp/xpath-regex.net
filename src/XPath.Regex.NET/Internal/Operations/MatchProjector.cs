using System.Collections.Immutable;
using XPath.Regex.NET.Internal.Matcher;
using XPath.Regex.NET.Internal.Nfa;

namespace XPath.Regex.NET.Internal.Operations;

/// <summary>
/// Projects internal VM match results (<see cref="MatchContext"/>) onto the
/// public <see cref="RegexMatch"/> and <see cref="CapturedGroup"/> DTOs.
/// </summary>
internal static class MatchProjector
{
    /// <summary>Projects a VM match context onto a public match DTO.</summary>
    public static RegexMatch ToRegexMatch(string input, MatchContext ctx, NfaProgram program)
    {
        int length = ctx.EndExclusive - ctx.Start;
        var groups = new List<RegexGroup>((ctx.Captures.Length / 2) + 1);

        for (int g = 0; g < ctx.Captures.Length / 2; g++)
        {
            int start = ctx.Captures[2 * g];
            int end = ctx.Captures[2 * g + 1];
            if (start >= 0 && end >= start)
            {
                int groupLength = end - start;
                groups.Add(new RegexGroup(true, start, groupLength, input.Substring(start, groupLength)));
            }
            else
            {
                groups.Add(new RegexGroup(false, -1, 0, string.Empty));
            }
        }

        return new RegexMatch(input, ctx.Start, length, input.Substring(ctx.Start, length), groups);
    }

    /// <summary>Projects a VM match context onto an analyze-string match region.</summary>
    public static MatchRegion ToMatchRegion(string input, MatchContext ctx, NfaProgram program)
    {
        IReadOnlyList<CapturedGroup> topGroups = BuildCapturedGroups(ctx.Captures, input, program, parentGroup: 0);
        int length = ctx.EndExclusive - ctx.Start;
        return new MatchRegion(input.Substring(ctx.Start, length), topGroups);
    }

    private static List<CapturedGroup> BuildCapturedGroups(
        int[] captures, string input, NfaProgram program, int parentGroup)
    {
        ImmutableArray<int> children = program.GroupChildren[parentGroup];
        var result = new List<CapturedGroup>(children.Length);

        foreach (int g in children)
        {
            int s = captures[2 * g];
            int e = captures[2 * g + 1];
            bool success = s >= 0 && e >= s;
            string value = success ? input.Substring(s, e - s) : string.Empty;
            List<CapturedGroup> nested = BuildCapturedGroups(captures, input, program, g);
            result.Add(new CapturedGroup(g, success, value, nested));
        }

        return result;
    }
}
