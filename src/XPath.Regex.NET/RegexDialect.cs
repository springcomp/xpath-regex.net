namespace XPath.Regex.NET;

/// <summary>
/// Specifies the dialect / compatibility level of an XSD/XPath regular expression.
/// Each level is a strict superset of the previous.
/// </summary>
public enum RegexDialect
{
    /// <summary>
    /// XML Schema Part 2 Appendix F (W3C 2004).
    /// Patterns are implicitly full-string anchored.
    /// Capturing groups have no capture semantics.
    /// <c>^</c> and <c>$</c> are ordinary characters.
    /// </summary>
    Xsd,

    /// <summary>
    /// XPath and XQuery Functions and Operators 2.0.
    /// Substring matching by default. <c>^</c> / <c>$</c> are anchor metacharacters.
    /// Adds reluctant quantifiers, capturing groups, back-references.
    /// Flags <c>s m i x</c> supported.
    /// </summary>
    XPath20,

    /// <summary>
    /// XPath and XQuery Functions and Operators 3.0 and 3.1 (recommended default).
    /// Substring matching by default. <c>^</c> / <c>$</c> are anchor metacharacters.
    /// Adds non-capturing groups <c>(?:…)</c> and <c>q</c> flag (literal mode).
    /// Clarifies <c>.</c> to exclude both newline and carriage return.
    /// Flags <c>s m i x q</c> supported.
    /// XPath 3.0 and 3.1 share identical features; 3.1 modularizes documentation only.
    /// </summary>
    XPath30,
}
