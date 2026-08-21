namespace XPath.Regex.NET;

/// <summary>
/// The result of <see cref="XPathRegex.AnalyzeString"/>.
/// Contains an ordered sequence of <see cref="AnalyzeStringRegion"/> items
/// (matched and non-matched) that together partition the entire input.
/// </summary>
public sealed class AnalyzeStringResult
{
    /// <summary>The original input string that was analyzed.</summary>
    public string Input { get; }

    /// <summary>
    /// Ordered regions covering the full input from left to right.
    /// Adjacent regions do not overlap.
    /// </summary>
    public IReadOnlyList<AnalyzeStringRegion> Regions { get; }

    /// <summary>Initializes a new result.</summary>
    internal AnalyzeStringResult(string input, IReadOnlyList<AnalyzeStringRegion> regions)
    {
        Input = input;
        Regions = regions;
    }
}

/// <summary>Base class for a region within an <see cref="AnalyzeStringResult"/>.</summary>
public abstract class AnalyzeStringRegion
{
    /// <summary>The text of this region.</summary>
    public string Value { get; }

    /// <summary>Initializes a new region.</summary>
    internal AnalyzeStringRegion(string value) => Value = value;
}

/// <summary>A region that was NOT matched by the pattern.</summary>
public sealed class NonMatchRegion : AnalyzeStringRegion
{
    /// <summary>Initializes a new non-match region.</summary>
    internal NonMatchRegion(string value) : base(value) { }
}

/// <summary>A region that WAS matched by the pattern, with optional captured-group detail.</summary>
public sealed class MatchRegion : AnalyzeStringRegion
{
    /// <summary>
    /// Captured sub-groups, 1-indexed. May be empty when the pattern has no capturing groups.
    /// Each entry corresponds to one capturing group in left-to-right opening-parenthesis order.
    /// A non-participating group has <see cref="CapturedGroup.Success"/> = <see langword="false"/>.
    /// </summary>
    public IReadOnlyList<CapturedGroup> Groups { get; }

    /// <summary>Initializes a new match region.</summary>
    internal MatchRegion(string value, IReadOnlyList<CapturedGroup> groups) : base(value)
        => Groups = groups;
}

/// <summary>A captured group within a <see cref="MatchRegion"/>.</summary>
public sealed class CapturedGroup
{
    /// <summary>1-based group number.</summary>
    public int Number { get; }

    /// <summary>Whether this group participated in the match.</summary>
    public bool Success { get; }

    /// <summary>
    /// Captured text,
    /// or <see cref="string.Empty"/> if <see cref="Success"/> is <see langword="false"/>.
    /// </summary>
    public string Value { get; }

    /// <summary>Nested captured groups (for groups containing groups), in left-to-right order.</summary>
    public IReadOnlyList<CapturedGroup> Groups { get; }

    /// <summary>Initializes a new captured group.</summary>
    internal CapturedGroup(int number, bool success, string value, IReadOnlyList<CapturedGroup> groups)
    {
        Number = number;
        Success = success;
        Value = value;
        Groups = groups;
    }
}
