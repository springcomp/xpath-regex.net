namespace XPath.Regex.NET.MatchesTests;

/// <summary>
/// Test cases from: iregexp/test/iregexp-atoms.spec.ts
/// </summary>
public class AtomsTests
{
    [Fact]
    public void Sequence_Matches()
    {
        Matches("a(a)a", "aaa");
    }

    [Fact]
    public void Sequence_FailsToMatch()
    {
        FailsToMatch("a(a)a", "");
    }
}
