namespace XPath.Regex.NET.Validates;

/// <summary>
/// Test cases from: iregexp/test/iregexp.spec.ts
/// Real-world RFC pattern validation from YANG models, XML schemas, etc.
/// </summary>
public class AppendixATests
{
    [Fact]
    public void InvalidExpressionReportsError()
    {
        // Expression '[?' should fail
        Fail("[?");
    }

    // Valid RFC patterns
    [Theory]
    [InlineData("([0-9a-fA-F]{2}(:[0-9a-fA-F]{2})*)?")]
    [InlineData("[0-9a-fA-F]{2}(:[0-9a-fA-F]{2}){5}")]
    [InlineData("((:|[0-9a-fA-F]{0,4}):)([0-9a-fA-F]{0,4}:){0,5}")]
    [InlineData("(([^:]+:){6}(([^:]+:[^:]+)|(.*\\..*)))|")]
    [InlineData("[0-9a-fA-F]*")]
    [InlineData("[aeiouy]*")]
    [InlineData("[A-Z][a-z]*")]
    [InlineData(@"\*")]
    [InlineData(@"[^\*].*")]
    [InlineData(@"\p{IsBasicLatin}{0,255}")]
    [InlineData("[a-zA-Z_][a-zA-Z0-9\\-_.]*")]
    [InlineData(".|..|[^xX].*|.[^mM].*|..[^lL].*")]
    [InlineData("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-")]
    [InlineData("[0-9\\.]*")]
    [InlineData("[0-9a-fA-F:\\.]*")]
    [InlineData("([0-9a-fA-F]){2}(:([0-9a-fA-F]){2}){0,254}")]
    [InlineData("([0-9a-fA-F]){2}(:([0-9a-fA-F]){2}){4,31}")]
    [InlineData("[xX][mM][lL].*")]
    [InlineData("[A-Z]{2}")]
    [InlineData("[0-9]{8}\\.[0-9]{6}")]
    [InlineData("(2((2[4-9])|(3[0-9]))\\.).*")]
    [InlineData("(([fF]{2}[0-9a-fA-F]{2}):).*")]
    [InlineData("/?([a-zA-Z0-9\\-_.]+)(/[a-zA-Z0-9\\-_.]+)*")]
    [InlineData("([a-zA-Z0-9\\-_.]+:)*")]
    [InlineData("[0-9a-fA-F]{2}(:[0-9a-fA-F]{2}){7}")]
    public void ValidRfcPatterns(string expression)
        => Pass(expression);

    [Theory]
    [InlineData(@"\S(.*\S)?")]
    [InlineData("(([0-1](\\.[1-3]?[0-9]))|(2\\.(0|([1-9]\\d*))))")]
    [InlineData(@"\d*(\.\d*){1,127}")]
    [InlineData(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?")]
    [InlineData(@"\S+")]
    [InlineData(@"\d{2}:\d{2}:\d{2}(\.\d+)?")]
    [InlineData(@"\d{4}-\d{2}-\d{2}")]
    [InlineData(@"Z|[\+\-]\d{2}:\d{2}")]
    [InlineData(@"[\S ]+")]
    public void InvalidRfcPatterns(string expression)
        => Pass(expression);
}
