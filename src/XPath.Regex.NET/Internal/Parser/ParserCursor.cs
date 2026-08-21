namespace XPath.Regex.NET.Internal.Parser;

internal sealed class ParserCursor
{
    private readonly IReadOnlyList<RegexToken> _tokens;
    private int _index;

    public ParserCursor(IReadOnlyList<RegexToken> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        if (tokens.Count == 0)
            throw new ArgumentException("Token list must not be empty.", nameof(tokens));

        _tokens = tokens;
        _index = 0;
    }

    public RegexToken Current => _tokens[_index];

    public RegexToken Consume()
    {
        RegexToken token = Current;
        if (_index < _tokens.Count - 1)
            _index++;

        return token;
    }

    public bool Match(RegexTokenKind kind)
    {
        if (Current.Kind != kind)
            return false;

        Consume();
        return true;
    }

    public RegexToken Expect(RegexTokenKind kind, string expected)
    {
        if (Current.Kind == kind)
            return Consume();

        throw ParserExceptionFactory.Expected(expected, Current.OriginalOffset);
    }

    public RegexToken Peek(int lookahead)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(lookahead);

        int index = _index + lookahead;
        if (index >= _tokens.Count)
            index = _tokens.Count - 1;

        return _tokens[index];
    }
}
