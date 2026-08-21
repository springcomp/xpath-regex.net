namespace XPath.Regex.NET.Validates;

/// <summary>
/// Test helpers for validating XSD/XPath regular expressions.
/// Maps to iregexp/test/utils.ts test utilities.
/// </summary>
internal static class RegexTestHelpers
{
    /// <summary>
    /// Asserts that the expression compiles successfully as a valid regex.
    /// Equivalent to iregexp test utility: pass()
    /// </summary>
    internal static void Pass(string expression)
    {
        try
        {
            var result = XPathRegex.Compile(expression);
            Assert.NotNull(result);
        }
        catch (ForxException ex)
        {
            Assert.Fail($"Expression should be valid but threw: {ex.Message}\nExpression: {expression}");
        }
    }

    /// <summary>
    /// Asserts that the expression fails to compile with an error.
    /// Equivalent to iregexp test utility: fail()
    /// </summary>
    internal static void Fail(string expression)
    {
        var succeeded = false;
        try
        {
            XPathRegex.Compile(expression);
            succeeded = true;
        }
        catch (ForxException)
        {
            // Expected to throw
        }

        if (succeeded)
        {
            Assert.Fail($"Expression should be invalid but compiled successfully: {expression}");
        }
    }
}
