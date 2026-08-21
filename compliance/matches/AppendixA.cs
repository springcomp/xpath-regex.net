namespace XPath.Regex.NET.MatchesTests;

/// <summary>
/// Test cases from: iregexp/test/iregexp.spec.ts
/// Real-world RFC pattern matching from YANG models, XML schemas, etc.
/// Note: "InvalidRfcPatterns" is preserved as-is from validates/ -- it is misnamed, the expressions
/// there are all valid/compilable, mirroring validates.AppendixATests.InvalidRfcPatterns which calls Pass().
/// </summary>
public class AppendixATests
{
    [Theory]
    [InlineData("[0-9a-fA-F]{2}(:[0-9a-fA-F]{2}){5}", "AB:CD:EF:12:34:56")]
    [InlineData("((:|[0-9a-fA-F]{0,4}):)([0-9a-fA-F]{0,4}:){0,5}", ":")]
    [InlineData("[A-Z][a-z]*", "Hello")]
    [InlineData(@"\*", "*")]
    [InlineData(@"[^\*].*", "abc")]
    [InlineData("[a-zA-Z_][a-zA-Z0-9\\-_.]*", "abc_123")]
    [InlineData(".|..|[^xX].*|.[^mM].*|..[^lL].*", "a")]
    [InlineData("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-", "12345678-ABCD-EF01-")]
    [InlineData("([0-9a-fA-F]){2}(:([0-9a-fA-F]){2}){0,254}", "AB")]
    [InlineData("([0-9a-fA-F]){2}(:([0-9a-fA-F]){2}){4,31}", "AB:CD:EF:12:34")]
    [InlineData("[xX][mM][lL].*", "xml")]
    [InlineData("[A-Z]{2}", "AB")]
    [InlineData("[0-9]{8}\\.[0-9]{6}", "12345678.123456")]
    [InlineData("(2((2[4-9])|(3[0-9]))\\.).*", "224.abc")]
    [InlineData("(([fF]{2}[0-9a-fA-F]{2}):).*", "FF12:rest")]
    [InlineData("/?([a-zA-Z0-9\\-_.]+)(/[a-zA-Z0-9\\-_.]+)*", "abc")]
    [InlineData("[0-9a-fA-F]{2}(:[0-9a-fA-F]{2}){7}", "AB:CD:EF:12:34:56:78:9A")]
    public void ValidRfcPatterns_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("[0-9a-fA-F]{2}(:[0-9a-fA-F]{2}){5}", "")]
    [InlineData("((:|[0-9a-fA-F]{0,4}):)([0-9a-fA-F]{0,4}:){0,5}", "")]
    [InlineData("[A-Z][a-z]*", "")]
    [InlineData(@"\*", "")]
    [InlineData(@"[^\*].*", "")]
    [InlineData("[a-zA-Z_][a-zA-Z0-9\\-_.]*", "")]
    [InlineData(".|..|[^xX].*|.[^mM].*|..[^lL].*", "")]
    [InlineData("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-", "")]
    [InlineData("([0-9a-fA-F]){2}(:([0-9a-fA-F]){2}){0,254}", "")]
    [InlineData("([0-9a-fA-F]){2}(:([0-9a-fA-F]){2}){4,31}", "")]
    [InlineData("[xX][mM][lL].*", "")]
    [InlineData("[A-Z]{2}", "")]
    [InlineData("[0-9]{8}\\.[0-9]{6}", "")]
    [InlineData("(2((2[4-9])|(3[0-9]))\\.).*", "")]
    [InlineData("(([fF]{2}[0-9a-fA-F]{2}):).*", "")]
    [InlineData("/?([a-zA-Z0-9\\-_.]+)(/[a-zA-Z0-9\\-_.]+)*", "")]
    [InlineData("[0-9a-fA-F]{2}(:[0-9a-fA-F]{2}){7}", "")]
    public void ValidRfcPatterns_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    // Nullable RFC patterns (top-level ?, *, or an empty alternative branch), uplifted to anchored form.
    [Theory]
    [InlineData("^(?:([0-9a-fA-F]{2}(:[0-9a-fA-F]{2})*)?)$", "AB")] // original: ([0-9a-fA-F]{2}(:[0-9a-fA-F]{2})*)?
    [InlineData("^(?:(([^:]+:){6}(([^:]+:[^:]+)|(.*\\..*)))|)$", "1:2:3:4:5:6:7:8")] // original: (([^:]+:){6}(([^:]+:[^:]+)|(.*\..*)))|
    [InlineData("^(?:[0-9a-fA-F]*)$", "AB12")] // original: [0-9a-fA-F]*
    [InlineData("^(?:[aeiouy]*)$", "aeiou")] // original: [aeiouy]*
    [InlineData(@"^(?:\p{IsBasicLatin}{0,255})$", "Hello")] // original: \p{IsBasicLatin}{0,255}
    [InlineData("^(?:[0-9\\.]*)$", "1.2.3")] // original: [0-9\.]*
    [InlineData("^(?:[0-9a-fA-F:\\.]*)$", "1a:2b.3c")] // original: [0-9a-fA-F:\.]*
    [InlineData("^(?:([a-zA-Z0-9\\-_.]+:)*)$", "abc:def:")] // original: ([a-zA-Z0-9\-_.]+:)*
    public void NullableRfcPatterns_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData("^(?:([0-9a-fA-F]{2}(:[0-9a-fA-F]{2})*)?)$", "XYZ")] // original: ([0-9a-fA-F]{2}(:[0-9a-fA-F]{2})*)?
    [InlineData("^(?:(([^:]+:){6}(([^:]+:[^:]+)|(.*\\..*)))|)$", "abc")] // original: (([^:]+:){6}(([^:]+:[^:]+)|(.*\..*)))|
    [InlineData("^(?:[0-9a-fA-F]*)$", "xyz")] // original: [0-9a-fA-F]*
    [InlineData("^(?:[aeiouy]*)$", "xyz")] // original: [aeiouy]*
    [InlineData(@"^(?:\p{IsBasicLatin}{0,255})$", "\u4E00")] // original: \p{IsBasicLatin}{0,255}
    [InlineData("^(?:[0-9\\.]*)$", "abc")] // original: [0-9\.]*
    [InlineData("^(?:[0-9a-fA-F:\\.]*)$", "xyz")] // original: [0-9a-fA-F:\.]*
    [InlineData("^(?:([a-zA-Z0-9\\-_.]+:)*)$", "abc")] // original: ([a-zA-Z0-9\-_.]+:)*
    public void NullableRfcPatterns_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);

    // Note: original method name is misleading -- these patterns are all Pass() cases in validates/, not Fail() cases.
    [Theory]
    [InlineData(@"\S(.*\S)?", "a")]
    [InlineData("(([0-1](\\.[1-3]?[0-9]))|(2\\.(0|([1-9]\\d*))))", "0.5")]
    [InlineData(@"\d*(\.\d*){1,127}", "1.5")]
    [InlineData(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?", "2024-01-01T12:00:00")]
    [InlineData(@"\S+", "abc")]
    [InlineData(@"\d{2}:\d{2}:\d{2}(\.\d+)?", "12:00:00")]
    [InlineData(@"\d{4}-\d{2}-\d{2}", "2024-01-01")]
    [InlineData(@"Z|[\+\-]\d{2}:\d{2}", "Z")]
    [InlineData(@"[\S ]+", "abc")]
    public void InvalidRfcPatterns_Matches(string expression, string input)
        => Matches(expression, input);

    [Theory]
    [InlineData(@"\S(.*\S)?", "")]
    [InlineData("(([0-1](\\.[1-3]?[0-9]))|(2\\.(0|([1-9]\\d*))))", "")]
    [InlineData(@"\d*(\.\d*){1,127}", "")]
    [InlineData(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?", "")]
    [InlineData(@"\S+", "")]
    [InlineData(@"\d{2}:\d{2}:\d{2}(\.\d+)?", "")]
    [InlineData(@"\d{4}-\d{2}-\d{2}", "")]
    [InlineData(@"Z|[\+\-]\d{2}:\d{2}", "")]
    [InlineData(@"[\S ]+", "")]
    public void InvalidRfcPatterns_FailsToMatch(string expression, string input)
        => FailsToMatch(expression, input);
}
