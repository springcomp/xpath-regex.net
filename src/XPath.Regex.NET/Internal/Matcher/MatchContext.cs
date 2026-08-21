namespace XPath.Regex.NET.Internal.Matcher;

internal sealed class MatchContext
{
    public int Start { get; }

    public int EndExclusive { get; }

    public int[] Captures { get; }

    public MatchContext(int start, int endExclusive, int[] captures)
    {
        Start = start;
        EndExclusive = endExclusive;
        Captures = captures;
    }
}
