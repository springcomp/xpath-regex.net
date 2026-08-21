using System.Globalization;

namespace XPath.Regex.NET.Internal.Parser;

internal static class RegexLexer
{
    public static IReadOnlyList<RegexToken> Tokenize(PreprocessedPattern input, RegexDialect dialect)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.IsLiteralMode)
            return TokenizeLiteral(input);

        string pattern = input.EffectivePattern;
        var tokens = new List<RegexToken>(pattern.Length + 1);

        int i = 0;
        int classDepth = 0;

        while (i < pattern.Length)
        {
            int start = i;
            char c = pattern[i];
            bool insideClass = classDepth > 0;



            if (c == '\\')
            {
                tokens.Add(TokenizeEscape(pattern, input, dialect, insideClass, ref i));
                continue;
            }

            if (!insideClass && c is >= '0' and <= '9' && ShouldTokenizeAsNumber(tokens))
            {
                tokens.Add(TokenizeNumber(pattern, input, ref i));
                continue;
            }

            switch (c)
            {
                case '|':
                    tokens.Add(SimpleToken(RegexTokenKind.Pipe, pattern, input, i++, 1));
                    break;

                case '(':
                    tokens.Add(SimpleToken(RegexTokenKind.LParen, pattern, input, i++, 1));
                    if (i + 1 < pattern.Length && pattern[i] == '?' && pattern[i + 1] == ':')
                    {
                        tokens.Add(SimpleToken(RegexTokenKind.NonCapturingPrefix, pattern, input, i, 2));
                        i += 2;
                    }
                    break;

                case ')':
                    tokens.Add(SimpleToken(RegexTokenKind.RParen, pattern, input, i++, 1));
                    break;

                case '[':
                    tokens.Add(SimpleToken(RegexTokenKind.LBracket, pattern, input, i++, 1));
                    classDepth++;
                    break;

                case ']':
                    tokens.Add(SimpleToken(RegexTokenKind.RBracket, pattern, input, i++, 1));
                    if (classDepth > 0)
                        classDepth--;
                    break;

                case '^':
                    tokens.Add(SimpleToken(RegexTokenKind.Caret, pattern, input, i++, 1));
                    break;

                case '$':
                    tokens.Add(SimpleToken(RegexTokenKind.Dollar, pattern, input, i++, 1));
                    break;

                case '-':
                    tokens.Add(SimpleToken(RegexTokenKind.Hyphen, pattern, input, i++, 1));
                    break;

                case '.':
                    tokens.Add(insideClass
                        ? LiteralToken(pattern, input, i++, c, 1)
                        : SimpleToken(RegexTokenKind.Dot, pattern, input, i++, 1));
                    break;

                case '?':
                    tokens.Add(insideClass
                        ? LiteralToken(pattern, input, i++, c, 1)
                        : SimpleToken(RegexTokenKind.Question, pattern, input, i++, 1));
                    break;

                case '*':
                    tokens.Add(insideClass
                        ? LiteralToken(pattern, input, i++, c, 1)
                        : SimpleToken(RegexTokenKind.Star, pattern, input, i++, 1));
                    break;

                case '+':
                    tokens.Add(insideClass
                        ? LiteralToken(pattern, input, i++, c, 1)
                        : SimpleToken(RegexTokenKind.Plus, pattern, input, i++, 1));
                    break;

                case '{':
                    tokens.Add(insideClass
                        ? LiteralToken(pattern, input, i++, c, 1)
                        : SimpleToken(RegexTokenKind.LBrace, pattern, input, i++, 1));
                    break;

                case '}':
                    tokens.Add(insideClass
                        ? LiteralToken(pattern, input, i++, c, 1)
                        : SimpleToken(RegexTokenKind.RBrace, pattern, input, i++, 1));
                    break;

                case ',':
                    tokens.Add(insideClass
                        ? LiteralToken(pattern, input, i++, c, 1)
                        : SimpleToken(RegexTokenKind.Comma, pattern, input, i++, 1));
                    break;

                default:
                    tokens.Add(LiteralToken(pattern, input, i++, c, 1));
                    break;
            }

            if (i <= start)
                throw new InvalidOperationException("Lexer cursor did not advance.");
        }

        tokens.Add(new RegexToken(
            RegexTokenKind.End,
            input.ToOriginalBoundaryOffset(pattern.Length),
            0,
            ReadOnlyMemory<char>.Empty));

        return tokens;
    }

    private static List<RegexToken> TokenizeLiteral(PreprocessedPattern input)
    {
        string pattern = input.EffectivePattern;
        var tokens = new List<RegexToken>(pattern.Length + 1);

        for (int i = 0; i < pattern.Length; i++)
            tokens.Add(LiteralToken(pattern, input, i, pattern[i], 1));

        tokens.Add(new RegexToken(
            RegexTokenKind.End,
            input.ToOriginalBoundaryOffset(pattern.Length),
            0,
            ReadOnlyMemory<char>.Empty));

        return tokens;
    }

    private static RegexToken TokenizeEscape(
        string pattern,
        PreprocessedPattern input,
        RegexDialect dialect,
        bool insideClass,
        ref int i)
    {
        int start = i;
        int startOffset = input.ToOriginalOffset(start);

        i++;
        if (i >= pattern.Length)
            throw LexerExceptionFactory.DanglingBackslash(startOffset);

        char next = pattern[i];

        if (!insideClass && next is >= '1' and <= '9')
        {
            int digitStart = i;
            while (i < pattern.Length && char.IsAsciiDigit(pattern[i]))
                i++;

            ReadOnlyMemory<char> lexeme = pattern.AsMemory(start, i - start);
            int? value = int.TryParse(pattern.AsSpan(digitStart, i - digitStart), NumberStyles.None, CultureInfo.InvariantCulture, out int parsed)
                ? parsed
                : null;

            return new RegexToken(RegexTokenKind.BackReference, startOffset, i - start, lexeme, IntValue: value);
        }

        if (next is 'p' or 'P')
            return TokenizePropertyEscape(pattern, input, ref i, start, startOffset, isComplement: next == 'P');

        if (IsMultiCharEscape(next))
        {
            i++;
            return new RegexToken(
                RegexTokenKind.MultiCharEscape,
                startOffset,
                2,
                pattern.AsMemory(start, 2),
                CharValue: next);
        }

        if (IsSingleCharEscape(next, dialect))
        {
            i++;
            return new RegexToken(
                RegexTokenKind.SingleCharEscape,
                startOffset,
                2,
                pattern.AsMemory(start, 2),
                CharValue: DecodeSingleCharEscape(next));
        }

        throw LexerExceptionFactory.InvalidEscape(startOffset);
    }

    private static RegexToken TokenizePropertyEscape(
        string pattern,
        PreprocessedPattern input,
        ref int i,
        int start,
        int startOffset,
        bool isComplement)
    {
        int payloadStart = i + 1;
        if (payloadStart >= pattern.Length || pattern[payloadStart] != '{')
            throw LexerExceptionFactory.InvalidEscape(startOffset);

        int payloadContentStart = payloadStart + 1;
        int close = payloadContentStart;

        while (close < pattern.Length && pattern[close] != '}')
            close++;

        if (close >= pattern.Length)
            throw LexerExceptionFactory.UnterminatedUnicodePropertyEscape(startOffset);

        if (close == payloadContentStart)
            throw LexerExceptionFactory.EmptyUnicodePropertyEscape(startOffset);

        string payload = pattern.Substring(payloadContentStart, close - payloadContentStart);

        i = close + 1;
        int length = i - start;

        return new RegexToken(
            isComplement ? RegexTokenKind.ComplementEscape : RegexTokenKind.CategoryEscape,
            input.ToOriginalOffset(start),
            length,
            pattern.AsMemory(start, length),
            TextValue: payload);
    }

    private static RegexToken TokenizeNumber(string pattern, PreprocessedPattern input, ref int i)
    {
        int start = i;
        while (i < pattern.Length && char.IsAsciiDigit(pattern[i]))
            i++;

        int length = i - start;
        int? value = int.TryParse(pattern.AsSpan(start, length), NumberStyles.None, CultureInfo.InvariantCulture, out int parsed)
            ? parsed
            : null;

        return new RegexToken(
            RegexTokenKind.Number,
            input.ToOriginalOffset(start),
            length,
            pattern.AsMemory(start, length),
            IntValue: value);
    }



    private static RegexToken SimpleToken(RegexTokenKind kind, string pattern, PreprocessedPattern input, int start, int length) =>
        new(kind, input.ToOriginalOffset(start), length, pattern.AsMemory(start, length));

    private static RegexToken LiteralToken(string pattern, PreprocessedPattern input, int start, char value, int length) =>
        new(RegexTokenKind.LiteralChar, input.ToOriginalOffset(start), length, pattern.AsMemory(start, length), CharValue: value);

    private static bool ShouldTokenizeAsNumber(List<RegexToken> tokens)
    {
        if (tokens.Count == 0)
            return false;

        RegexTokenKind previous = tokens[^1].Kind;
        return previous is RegexTokenKind.LBrace or RegexTokenKind.Comma;
    }

    private static bool IsSingleCharEscape(char c, RegexDialect dialect)
    {
        if (c == '$')
            return dialect == RegexDialect.XPath30;

        return c is
            'n' or 'r' or 't' or
            '\\' or '|' or '.' or '?' or '*' or '+' or '(' or ')' or '{' or '}' or
            '-' or '[' or ']' or '^';
    }

    private static bool IsMultiCharEscape(char c) =>
        c is 's' or 'S' or 'i' or 'I' or 'c' or 'C' or 'd' or 'D' or 'w' or 'W';

    private static char DecodeSingleCharEscape(char c) =>
        c switch
        {
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            _ => c,
        };


}
