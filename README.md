# XPath.Regex.NET

XSD/XPath-compatible regex engine for .NET, suitable for embedding into XSLT processors and XML tools.

## Features

- **XSD/XPath regex dialect** - Full compliance with W3C XSD spec
- **Unicode-aware** - Powered by [Unicode.NET](https://github.com/springcomp/unicode.net) library
- **Unicode character classes** - General categories, blocks, scripts, binary properties
- **Case handling** - Simple and full case folding, case closure
- **Pattern matching** - `matches()`, `replace()`, `tokenize()` functions
- **Named groups and flags** - Extended regex capabilities
- **Performance tuned** - Optimized for embedding in high-performance scenarios

## Installation

```xml
<PackageReference Include="XPath.Regex.NET" Version="1.0.0" />
```

## Quick Start

### Basic matching

```csharp
using XPath.Regex.NET;

var pattern = @"[a-z]+";
var regex = XPathRegex.Compile(pattern);

if (regex.Matches("hello"))
    Console.WriteLine("Match!");
```

### Unicode character classes

```csharp
// General category: uppercase letters
var pattern = @"\p{Lu}+";
var regex = XPathRegex.Compile(pattern);
Console.WriteLine(regex.Matches("HELLO")); // true

// Script: Greek letters
var greek = @"\p{scr=Grek}+";
var regex2 = XPathRegex.Compile(greek);
Console.WriteLine(regex2.Matches("Ελληνικά")); // true
```

### Pattern functions

```csharp
// Replace all occurrences
var result = regex.Replace("hello world", "h", "H");
// result: "Hello world"

// Tokenize by pattern
var tokens = regex.Tokenize("a1b2c3", @"\d+");
// tokens: ["a", "b", "c"]

// Analyze string with capturing groups
var match = regex.AnalyzeString("test123end", @"(\w+)(\d+)(\w+)");
// Access groups via match.Groups
```

### Regex flags

```csharp
var pattern = @"hello";
var flags = RegexFlags.IgnoreCase;
var regex = XPathRegex.Compile(pattern, flags);

Console.WriteLine(regex.Matches("HELLO")); // true with IgnoreCase flag
```

## Supported Dialects

- **XSD** - Full XSD regex support.
- **XPath 2.0** - Anchors, lazy quantifiers, back references.
- **XPath 3.x** - Non capturing groups, 'q' flag.

Specify via:
```csharp
var regex = XPathRegex.Compile(pattern, dialect: RegexDialect.XPath30);
```

## Unicode Version

Default: Unicode 15.1.0 (configurable via `Unicode.NET` dependency)

## Documentation

- [XSD Regex Specification](https://www.w3.org/TR/xmlschema-2/#regexs)
- [XPath Functions and Operators](https://www.w3.org/TR/xpath-functions/)
- [Unicode.NET Reference](https://github.com/springcomp/uni)

## Use Cases

- **XSLT processors** - Pattern matching in XSLT transformations
- **XML validation** - XSD pattern facet evaluation
- **Text processing** - Unicode-aware regex on XML content
- **Data extraction** - Capturing groups from XML text nodes

## Compile-time safety limits

`XPathRegex.Compile` accepts an optional `RegexCompileOptions` to cap the
resources a pattern may consume while compiling: `MaxQuantifierBound` (default
1,000) limits either bound of a `{min,max}` quantifier, and
`MaxProgramInstructions` (default 100,000) limits the size of the compiled
NFA program, defending against pathological nested repeats such as
`(a{1000}){1000}` where each quantifier is individually within bounds but
their expansion is not. Callers may only lower these secure defaults; each
option has a non-bypassable hard ceiling equal to its default
(`RegexCompileOptions.HardCeilingMaxQuantifierBound`,
`RegexCompileOptions.HardCeilingMaxProgramInstructions`), and constructing
options above either ceiling throws `ArgumentOutOfRangeException`. Exceeding a
configured limit while compiling throws
`RegexCompilationLimitExceededException`, which is distinct from
`Forx0002Exception` (invalid pattern syntax).

## Runtime safety limits

Matching can throw `RegexEngineLimitExceededException` when a runtime safety
valve is reached: `TimeoutMs` (5,000 ms), `MaxThreadCount` (100,000 threads),
or `MaxStackDepth` (10,000 epsilon-expansion levels). This exception is not a
`FORX00xx` error; `Forx0002Exception` is reserved for invalid patterns detected
during compilation.

## License

Apache License 2.0
