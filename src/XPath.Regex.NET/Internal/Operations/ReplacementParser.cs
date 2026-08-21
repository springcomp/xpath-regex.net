using System.Text;
using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Matcher;

namespace XPath.Regex.NET.Internal.Operations;

// ---------------------------------------------------------------------------
// Parts
// ---------------------------------------------------------------------------

internal abstract class ReplacementPart
{
    public abstract void Append(MatchContext match, string input, StringBuilder sb);
}

internal sealed class LiteralPart(string text) : ReplacementPart
{
    public override void Append(MatchContext match, string input, StringBuilder sb)
        => sb.Append(text);
}

internal sealed class GroupRefPart(int groupNumber) : ReplacementPart
{
    public override void Append(MatchContext match, string input, StringBuilder sb)
    {
        int s = match.Captures[2 * groupNumber];
        int e = match.Captures[2 * groupNumber + 1];
        if (s >= 0 && e >= s)
            sb.Append(input, s, e - s);
    }
}

// ---------------------------------------------------------------------------
// ParsedReplacement
// ---------------------------------------------------------------------------

internal sealed class ParsedReplacement(IReadOnlyList<ReplacementPart> parts)
{
    public void Apply(MatchContext match, string input, StringBuilder sb)
    {
        foreach (ReplacementPart part in parts)
            part.Append(match, input, sb);
    }
}

// ---------------------------------------------------------------------------
// ReplacementParser
// ---------------------------------------------------------------------------

internal static class ReplacementParser
{
    /// <summary>
    /// Parses a replacement string into a compiled <see cref="ParsedReplacement"/>.
    /// </summary>
    /// <exception cref="Forx0004Exception">
    /// Thrown when replacement contains an invalid '$' or '\' sequence
    /// (only when Literal flag is off).
    /// </exception>
    public static ParsedReplacement Parse(string replacement, RegexFlags flags, int captureCount)
    {
        ArgumentNullException.ThrowIfNull(replacement);

        // When q flag active: entire string is literal, no '$' or '\' interpretation.
        if (flags.Literal)
            return new ParsedReplacement([new LiteralPart(replacement)]);

        var parts = new List<ReplacementPart>();
        var literal = new StringBuilder();

        int i = 0;
        while (i < replacement.Length)
        {
            char c = replacement[i];

            if (c == '\\')
            {
                i++;
                if (i >= replacement.Length)
                    throw new Forx0004Exception("Dangling '\\' in replacement string.", i - 1);

                char next = replacement[i];
                if (next == '$' || next == '\\')
                {
                    literal.Append(next);
                    i++;
                }
                else
                {
                    throw new Forx0004Exception($"Invalid '\\{next}' in replacement string.", i - 1);
                }
                continue;
            }

            if (c == '$')
            {
                i++;
                if (i >= replacement.Length || !char.IsAsciiDigit(replacement[i]))
                    throw new Forx0004Exception("'$' not followed by digit in replacement string.", i - 1);

                // Consume all consecutive digits.
                int digitStart = i;
                while (i < replacement.Length && char.IsAsciiDigit(replacement[i]))
                    i++;

                string digitStr = replacement.Substring(digitStart, i - digitStart);
                // Parse as long to avoid overflow, then resolve.
                long n = 0;
                foreach (char d in digitStr)
                    n = n * 10 + (d - '0');

                // T-001 §11 resolution: while N > captureCount and N > 9, strip last digit.
                var trailingLiterals = new List<char>();
                while (n > captureCount && n > 9)
                {
                    trailingLiterals.Insert(0, (char)('0' + (int)(n % 10)));
                    n /= 10;
                }

                // Flush any accumulated literal so far.
                if (literal.Length > 0)
                {
                    parts.Add(new LiteralPart(literal.ToString()));
                    literal.Clear();
                }

                if (n <= captureCount)
                {
                    parts.Add(new GroupRefPart((int)n));
                }
                else
                {
                    // N is 1-9 but > captureCount: resolves to empty string (no part needed).
                    // We still need to emit nothing, so just skip.
                }

                if (trailingLiterals.Count > 0)
                    parts.Add(new LiteralPart(new string(trailingLiterals.ToArray())));

                continue;
            }

            literal.Append(c);
            i++;
        }

        if (literal.Length > 0)
            parts.Add(new LiteralPart(literal.ToString()));

        return new ParsedReplacement(parts);
    }
}
