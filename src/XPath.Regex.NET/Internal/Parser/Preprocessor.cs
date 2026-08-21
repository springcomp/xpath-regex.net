using System.Text;

namespace XPath.Regex.NET.Internal.Parser;

internal static class Preprocessor
{
    public static PreprocessedPattern Process(string pattern, RegexFlags flags)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (flags.Literal)
            return BuildLiteral(pattern);

        if (!flags.FreeSpacing)
            return BuildIdentity(pattern, isLiteralMode: false);

        var effective = new StringBuilder(pattern.Length);
        var map = new List<SourceMapEntry>(pattern.Length);

        int classDepth = 0;
        bool escaped = false;
        bool inComment = false;

        for (int originalOffset = 0; originalOffset < pattern.Length; originalOffset++)
        {
            char c = pattern[originalOffset];

            if (inComment)
            {
                if (c != '\n' && c != '\r')
                    continue;

                inComment = false;
            }

            if (!escaped && classDepth == 0)
            {
                if (IsFreeSpacingWhitespace(c))
                    continue;

                if (c == '#')
                {
                    inComment = true;
                    continue;
                }
            }

            int effectiveOffset = effective.Length;
            effective.Append(c);
            map.Add(new SourceMapEntry(effectiveOffset, originalOffset));

            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == '[')
            {
                classDepth++;
            }
            else if (c == ']' && classDepth > 0)
            {
                classDepth--;
            }
        }

        return new PreprocessedPattern(effective.ToString(), isLiteralMode: false, map, pattern.Length);
    }

    private static PreprocessedPattern BuildLiteral(string pattern) =>
        BuildIdentity(pattern, isLiteralMode: true);

    private static PreprocessedPattern BuildIdentity(string pattern, bool isLiteralMode)
    {
        var map = new List<SourceMapEntry>(pattern.Length);
        for (int i = 0; i < pattern.Length; i++)
            map.Add(new SourceMapEntry(i, i));

        return new PreprocessedPattern(pattern, isLiteralMode, map, pattern.Length);
    }

    private static bool IsFreeSpacingWhitespace(char c) =>
        c is '\u0020' or '\u0009' or '\u000A' or '\u000D';
}
