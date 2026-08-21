using System.Globalization;
using Unicode.NET;

namespace XPath.Regex.NET.MatchesTests;

/// <summary>
/// Test helpers for validating the runtime matching behavior of XSD/XPath regular expressions.
/// </summary>
internal static class RegexTestHelpers
{
    static readonly CultureInfo ENUS = new CultureInfo("en-US", false);

    /// <summary>
    /// Asserts that the expression compiles and matches the given input.
    /// </summary>
    internal static void Matches(string expression, string input, RegexDialect dialect = RegexDialect.XPath30)
    {
        var regex = Compile(expression, dialect);
        Assert.True(regex.IsMatch(input), $"Expected '{expression}' to match '{input}'");
    }

    /// <summary>
    /// Asserts that the expression compiles, matches the given input and matched the expected string.
    /// </summary>
    internal static void Matches(string expression, string input, string matched, RegexDialect dialect = RegexDialect.XPath30)
    {
        var index = 0;
        var pos = matched.IndexOf(':', StringComparison.Ordinal);
        if (pos != -1)
        {
            index = int.Parse(matched.AsSpan(0, pos), NumberStyles.None, ENUS);
            matched = matched.Substring(pos + 1);
        }

        var regex = Compile(expression, dialect);
        var match = regex.Match(input);
        Assert.True(match is not null, $"Expected '{expression}' to match '{input}'");
        Assert.True(matched == match.Value, $"Expected '{expression}' to match {index}:'{matched}', but matched {match.Index}:'{match.Value}' instead.");
    }

    /// <summary>
    /// Asserts that the expression compiles but does not match the given input.
    /// </summary>
    internal static void FailsToMatch(string expression, string input, RegexDialect dialect = RegexDialect.XPath30)
    {
        var regex = Compile(expression, dialect);
        Assert.False(regex.IsMatch(input), $"Expected '{expression}' to NOT match '{input}'");
    }

    private static XPathRegex Compile(string expression, RegexDialect dialect = RegexDialect.XPath30)
    {
        try
        {
            return XPathRegex.Compile(expression, dialect);
        }
        catch (ForxException ex)
        {
            Assert.Fail($"Expression should be valid but threw: {ex.Message}\nExpression: {expression}");
            throw;
        }
    }
}
