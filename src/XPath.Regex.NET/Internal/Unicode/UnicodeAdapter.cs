using Unicode.NET;
using Unicode.NET.Xml;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace XPath.Regex.NET.Internal.Unicode;

/// <summary>
/// Adapts Unicode.NET services to REGEX internal tuple-range representation.
/// </summary>
internal static class UnicodeAdapter
{
    /// <summary>Converts UNI CodePointSet to REGEX tuple ranges.</summary>
    public static ImmutableArray<(int Lo, int Hi)> ToTupleRanges(CodePointSet set)
    {
        if (set.IsEmpty)
            return ImmutableArray<(int Lo, int Hi)>.Empty;

        var builder = ImmutableArray.CreateBuilder<(int, int)>(set.RangeCount);
        foreach (var range in set.Ranges)
            builder.Add((range.Start.Value, range.End.Value));
        return builder.MoveToImmutable();
    }

    /// <summary>Converts REGEX tuple ranges to UNI CodePointSet.</summary>
    public static CodePointSet ToCodePointSet(ImmutableArray<(int Lo, int Hi)> ranges)
    {
        if (ranges.IsDefaultOrEmpty)
            return CodePointSet.Empty;

        var builder = new CodePointSetBuilder();
        foreach (var (lo, hi) in ranges)
            builder.Add(CodePointRange.Create(lo, hi));
        return builder.Build();
    }

    /// <summary>
    /// Resolves Unicode property/category/block/script name via UNI.
    /// Applies dialect gating: XSD blocks scripts.
    /// </summary>
    // Surrogates never occur at the XML character abstraction level; excluded by the grammar.
    private static readonly HashSet<string> s_excludedBlocks =
        new(StringComparer.Ordinal) { "HighSurrogates", "LowSurrogates", "HighPrivateUseSurrogates" };

    public static ImmutableArray<(int Lo, int Hi)> ResolveProperty(
        string name,
        bool negated,
        RegexDialect dialect)
    {
        string normalized = NormalizeName(name);
        bool isBlockReference = name.StartsWith("Is", StringComparison.OrdinalIgnoreCase) ||
                                name.StartsWith("In", StringComparison.OrdinalIgnoreCase);

        // The 'C' general category grammar production is 'C' [cfon]? - 's' (Surrogate) is not a valid suffix.
        if (!isBlockReference && normalized == "Cs")
            throw new InvalidOperationException(
                $"Unicode category '{name}' (Surrogate) is not valid: surrogates do not occur at the XML character abstraction level.");

        CodePointSet set;
        if (isBlockReference)
        {
            // Is/In prefix denotes a block reference only; never fall back to script/category/binary resolution.
            if (!UnicodeBlocks.TryResolveBlock(name, UnicodeVersion.V15_1_0, out set) ||
                s_excludedBlocks.Contains(normalized))
                throw new InvalidOperationException($"Unknown Unicode block '{name}'.");
        }
        else
        {
            // Dialect gating: XSD does not support script properties.
            if (dialect == RegexDialect.Xsd && IsScript(normalized))
                throw new InvalidOperationException(
                    $"Script property '{name}' not supported in XSD dialect.");

            if (!UnicodeProperties.TryResolve(normalized, UnicodeVersion.V15_1_0, out set))
            {
                var suggestions = UnicodeProperties.Suggest(normalized, UnicodeVersion.V15_1_0, maxSuggestions: 3);
                string hint = suggestions.Any() ? $" Did you mean: {string.Join(", ", suggestions.Select(s => $"'{s}'"))}?" : "";
                throw new InvalidOperationException($"Unknown Unicode property '{name}'.{hint}");
            }
        }

        if (negated)
            set = set.Complement();

        return ToTupleRanges(set);
    }

    /// <summary>Resolves shortcut escapes (\d, \s, \w, \i, \c).</summary>
    public static ImmutableArray<(int Lo, int Hi)> ResolveShortcut(char escape)
    {
        CodePointSet set = escape switch
        {
            'd' => XPathShortcuts.Digit,
            'D' => XPathShortcuts.Digit.Complement(),
            's' => XPathShortcuts.Space,
            'S' => XPathShortcuts.Space.Complement(),
            'w' => XPathShortcuts.Word,
            'W' => XPathShortcuts.Word.Complement(),
            'i' => XmlCharacterSets.NameStartChar,
            'I' => XmlCharacterSets.NameStartChar.Complement(),
            'c' => XmlCharacterSets.NameChar,
            'C' => XmlCharacterSets.NameChar.Complement(),
            _ => throw new ArgumentOutOfRangeException(nameof(escape))
        };
        return ToTupleRanges(set);
    }

    /// <summary>Applies case-insensitive closure via UNI.</summary>
    public static ImmutableArray<(int Lo, int Hi)> ApplyCaseClosure(ImmutableArray<(int Lo, int Hi)> ranges)
    {
        CodePointSet inputSet = ToCodePointSet(ranges);
        CodePointSet closedSet = CaseClosure.Closure(inputSet, CaseFoldingMode.Simple, version: UnicodeVersion.V15_1_0);
        return ToTupleRanges(closedSet);
    }

    /// <summary>Simple case fold for a single code point.</summary>
    public static int SimpleFold(int codePoint)
    {
        var builder = new CodePointSetBuilder();
        builder.Add(CodePointRange.Create(codePoint, codePoint));
        var set = builder.Build();
        var folded = CaseClosure.Closure(set, CaseFoldingMode.Simple, version: UnicodeVersion.V15_1_0);
        // Return first code point in folded set
        if (!folded.IsEmpty)
            return folded.Ranges.First().Start.Value;
        return codePoint;
    }

    private static string NormalizeName(string name)
    {
        if (name.StartsWith("Is", StringComparison.OrdinalIgnoreCase))
            return name.Substring(2);
        if (name.StartsWith("In", StringComparison.OrdinalIgnoreCase))
            return name.Substring(2);
        return name;
    }

    private static bool IsScript(string name)
    {
        if (UnicodeScripts.TryResolveScript(name, UnicodeVersion.V15_1_0, out _))
            return true;
        // Handle compound syntax: "Script=Greek", "sc=Grek", etc.
        int eq = name.IndexOf('=', StringComparison.Ordinal);
        if (eq >= 0)
        {
            string prefix = name.Substring(0, eq).Trim();
            string value = name.Substring(eq + 1).Trim();
            if (prefix.Equals("Script", StringComparison.OrdinalIgnoreCase) ||
                prefix.Equals("sc", StringComparison.OrdinalIgnoreCase))
                return UnicodeScripts.TryResolveScript(value, UnicodeVersion.V15_1_0, out _);
        }
        return false;
    }
}
