namespace XPath.Regex.NET.Validates;

/// <summary>
/// Test cases from: iregexp/test/iregexp-atoms.spec.ts
/// </summary>
public class AtomsTests
{
    [Fact]
    public void Sequence()
    {
        Pass("a(a)a");
    }
}
