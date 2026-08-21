// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project. See:
// https://docs.microsoft.com/dotnet/fundamentals/code-analysis/suppress-warnings
using System.Diagnostics.CodeAnalysis;

// CA1724: The public type 'XPathRegex' partially or fully matches the namespace name
// 'XPath.Regex.NET'. This is intentional per the API design contract.
[assembly: SuppressMessage(
    "Naming", "CA1724:Type names should not match namespaces",
    Justification = "Public API contract: XPathRegex is the primary entry point in the XPath.Regex.NET namespace.",
    Scope = "type", Target = "~T:XPath.Regex.NET.XPathRegex")]
