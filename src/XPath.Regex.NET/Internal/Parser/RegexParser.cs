using System.Collections.Immutable;
using System.Globalization;
using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Ast;
using XPath.Regex.NET.Internal.Unicode;

namespace XPath.Regex.NET.Internal.Parser;

internal static class RegexParser
{
    public static ParseResult Parse(
        IReadOnlyList<RegexToken> tokens,
        RegexDialect dialect,
        RegexFlags flags,
        IUnicodeNameValidator? unicodeValidator,
        RegexCompileOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        IUnicodeNameValidator validator = unicodeValidator ?? PermissiveUnicodeNameValidator.Instance;

        var state = new ParserState(tokens, dialect, flags, validator, options ?? RegexCompileOptions.Default);
        ParseResult result = state.Parse();

        SemanticValidator.Validate(result.Root, result.CaptureCount, dialect, validator);
        return result;
    }

    private sealed class ParserState
    {
        private readonly ParserCursor _cursor;
        private readonly RegexDialect _dialect;
        private readonly RegexFlags _flags;
        private readonly IUnicodeNameValidator _unicodeValidator;
        private readonly UnicodeClassEvaluator _unicodeEvaluator;
        private readonly RegexCompileOptions _options;

        private int _nextCaptureNumber = 1;

        public ParserState(
            IReadOnlyList<RegexToken> tokens,
            RegexDialect dialect,
            RegexFlags flags,
            IUnicodeNameValidator unicodeValidator,
            RegexCompileOptions options)
        {
            _cursor = new ParserCursor(tokens);
            _dialect = dialect;
            _flags = flags;
            _unicodeValidator = unicodeValidator;
            _unicodeEvaluator = new UnicodeClassEvaluator(dialect);
            _options = options;
        }

        public ParseResult Parse()
        {
            AstNode root = ParseRegExp();
            _ = _cursor.Expect(RegexTokenKind.End, "end of pattern");
            return new ParseResult(root, _nextCaptureNumber - 1);
        }

        private AstNode ParseRegExp()
        {
            var alternatives = new List<AstNode> { ParseBranch() };
            while (_cursor.Match(RegexTokenKind.Pipe))
                alternatives.Add(ParseBranch());

            return AstBuilderHelpers.BuildAlternation(alternatives);
        }

        private AstNode ParseBranch()
        {
            var pieces = new List<AstNode>();

            while (!IsBranchStop(_cursor.Current.Kind))
                pieces.Add(ParsePiece());

            return AstBuilderHelpers.BuildConcat(pieces);
        }

        private static bool IsBranchStop(RegexTokenKind kind) =>
            kind is RegexTokenKind.Pipe or RegexTokenKind.RParen or RegexTokenKind.End;

        private AstNode ParsePiece()
        {
            if (IsQuantifierStart(_cursor.Current.Kind))
                throw ParserExceptionFactory.QuantifierHasNoTarget(_cursor.Current.OriginalOffset);

            AstNode atom = ParseAtom();
            if (!IsQuantifierStart(_cursor.Current.Kind))
                return atom;

            RegexToken quantifierToken = _cursor.Current;
            ParsedQuantifier quantifier = ParseQuantifier();

            return new RepeatNode(atom, quantifier.Min, quantifier.Max, quantifier.Greedy);
        }

        private ParsedQuantifier ParseQuantifier()
        {
            RegexToken token = _cursor.Consume();

            int min;
            int? max;

            switch (token.Kind)
            {
                case RegexTokenKind.Question:
                    min = 0;
                    max = 1;
                    break;

                case RegexTokenKind.Star:
                    min = 0;
                    max = null;
                    break;

                case RegexTokenKind.Plus:
                    min = 1;
                    max = null;
                    break;

                case RegexTokenKind.LBrace:
                    (min, max) = ParseQuantifierBraces(token.OriginalOffset);
                    break;

                default:
                    throw ParserExceptionFactory.UnexpectedToken(token);
            }

            bool greedy = true;
            if (_cursor.Current.Kind == RegexTokenKind.Question)
            {
                RegexToken suffixToken = _cursor.Consume();
                if (_dialect == RegexDialect.Xsd)
                    throw ParserExceptionFactory.ReluctantQuantifierNotSupported(_dialect, suffixToken.OriginalOffset);

                greedy = false;
            }

            return new ParsedQuantifier(min, max, greedy);
        }

        private (int Min, int? Max) ParseQuantifierBraces(int quantifierOffset)
        {
            RegexToken first = _cursor.Expect(RegexTokenKind.Number, "number");
            int min = ReadQuantifierBound(first, quantifierOffset);
            ValidateQuantifierBound(min, quantifierOffset);

            if (_cursor.Match(RegexTokenKind.RBrace))
                return (min, min);

            _ = _cursor.Expect(RegexTokenKind.Comma, ",");

            if (_cursor.Match(RegexTokenKind.RBrace))
                return (min, null);

            RegexToken second = _cursor.Expect(RegexTokenKind.Number, "number");
            int max = ReadQuantifierBound(second, quantifierOffset);
            ValidateQuantifierBound(max, quantifierOffset);
            _ = _cursor.Expect(RegexTokenKind.RBrace, "}");

            if (min > max)
                throw ParserExceptionFactory.InvalidQuantifierBounds(quantifierOffset);

            return (min, max);
        }

        // Enforced before expansion so a huge bound never reaches CompileRepeat's unrolling loop.
        private void ValidateQuantifierBound(int value, int offset)
        {
            if (value > _options.MaxQuantifierBound)
                throw new RegexCompilationLimitExceededException(
                    RegexCompilationLimit.MaxQuantifierBound,
                    $"Quantifier bound {value.ToString(CultureInfo.InvariantCulture)} at offset {offset.ToString(CultureInfo.InvariantCulture)} exceeds the configured limit of {_options.MaxQuantifierBound.ToString(CultureInfo.InvariantCulture)}.");
        }

        private AstNode ParseAtom()
        {
            RegexToken token = _cursor.Current;
            switch (token.Kind)
            {
                case RegexTokenKind.LiteralChar:
                    _ = _cursor.Consume();
                    return new LiteralNode(token.CharValue ?? throw new InvalidOperationException("Literal token missing CharValue."));

                case RegexTokenKind.Dot:
                    _ = _cursor.Consume();
                    return new WildcardNode();

                case RegexTokenKind.Caret:
                    _ = _cursor.Consume();
                    if (_dialect == RegexDialect.Xsd)
                        return new LiteralNode('^'); // XSD: ^ is a plain literal, not an anchor

                    return new AnchorStartNode();

                case RegexTokenKind.Dollar:
                    _ = _cursor.Consume();
                    if (_dialect == RegexDialect.Xsd)
                        return new LiteralNode('$'); // XSD: $ is a plain literal, not an anchor

                    return new AnchorEndNode();

                case RegexTokenKind.SingleCharEscape:
                    _ = _cursor.Consume();
                    return new LiteralNode(token.CharValue ?? throw new InvalidOperationException("Single escape token missing CharValue."));

                case RegexTokenKind.MultiCharEscape:
                    _ = _cursor.Consume();
                    ImmutableArray<(int Lo, int Hi)> multiRanges = UnicodeClassEvaluator.ResolveMultiCharEscape(token.CharValue ?? throw new InvalidOperationException("Multi escape token missing CharValue."));
                    return new CharClassNode(multiRanges, false);

                case RegexTokenKind.CategoryEscape:
                case RegexTokenKind.ComplementEscape:
                    _ = _cursor.Consume();
                    return ParseUnicodeEscapeAtom(token);

                case RegexTokenKind.BackReference:
                    _ = _cursor.Consume();
                    return ParseBackReference(token);

                case RegexTokenKind.LParen:
                    return ParseGroup();

                case RegexTokenKind.LBracket:
                    return ParseCharClassExpr();



                case RegexTokenKind.Hyphen:
                    // Dash outside character class is a literal character
                    _ = _cursor.Consume();
                    return new LiteralNode('-');

                case RegexTokenKind.Comma:
                    // Comma outside a {n,m} quantifier is a literal character
                    _ = _cursor.Consume();
                    return new LiteralNode(',');

                default:
                    throw ParserExceptionFactory.UnexpectedToken(token);
            }
        }

        private AstNode ParseGroup()
        {
            RegexToken open = _cursor.Expect(RegexTokenKind.LParen, "(");

            bool nonCapturing = _cursor.Match(RegexTokenKind.NonCapturingPrefix);
            if (nonCapturing && _dialect == RegexDialect.Xsd)
                throw ParserExceptionFactory.FeatureNotSupported("(?:)", _dialect, open.OriginalOffset);

            int groupNumber = 0;
            if (!nonCapturing)
            {
                groupNumber = _nextCaptureNumber;
                _nextCaptureNumber++;
            }

            AstNode child = ParseRegExp();
            _ = _cursor.Expect(RegexTokenKind.RParen, ")");

            if (nonCapturing)
                return new NonCaptureNode(child);

            return new CaptureNode(groupNumber, child);
        }

        private BackReferenceNode ParseBackReference(RegexToken token)
        {
            if (_dialect == RegexDialect.Xsd)
                throw ParserExceptionFactory.FeatureNotSupported("back-reference", _dialect, token.OriginalOffset);

            int groupNumber = token.IntValue ?? ReadBackReference(token);
            if (groupNumber <= 0 || groupNumber >= _nextCaptureNumber)
                throw ParserExceptionFactory.BackReferenceInvalid(groupNumber, token.OriginalOffset);

            return new BackReferenceNode(groupNumber);
        }

        private CharClassNode ParseUnicodeEscapeAtom(RegexToken token)
        {
            string payload = token.TextValue ?? throw new InvalidOperationException("Unicode escape token missing TextValue.");
            bool isBlock = payload.StartsWith("Is", StringComparison.Ordinal);
            bool valid = isBlock
                ? _unicodeValidator.IsValidBlock(payload)
                : _unicodeValidator.IsValidCategory(payload);

            if (!valid)
                throw ParserExceptionFactory.UnknownUnicodePropertyOrBlock(payload, token.OriginalOffset);

            bool negated = token.Kind == RegexTokenKind.ComplementEscape;
            ImmutableArray<(int Lo, int Hi)> ranges = _unicodeEvaluator.ResolvePropertyEscape(payload, negated);
            return new CharClassNode(ranges, false);
        }

        private CharClassNode ParseCharClassExpr()
        {
            _ = _cursor.Expect(RegexTokenKind.LBracket, "[");

            bool negated = _cursor.Match(RegexTokenKind.Caret);
            List<(int Lo, int Hi)> baseRanges = ParseClassItems(negated);
            if (baseRanges.Count == 0)
                throw ParserExceptionFactory.UnexpectedToken(_cursor.Current);

            ImmutableArray<(int Lo, int Hi)> effectiveRanges = AstBuilderHelpers.NormalizeRanges(baseRanges);

            if (_cursor.Current.Kind == RegexTokenKind.Hyphen && _cursor.Peek(1).Kind == RegexTokenKind.LBracket)
            {
                _ = _cursor.Consume();
                CharClassNode subtractNode = (CharClassNode)ParseCharClassExpr();
                ImmutableArray<(int Lo, int Hi)> subtractRanges = ExpandRanges(subtractNode);
                effectiveRanges = AstBuilderHelpers.SubtractRanges(effectiveRanges, subtractRanges);
            }

            _ = _cursor.Expect(RegexTokenKind.RBracket, "]");
            return new CharClassNode(effectiveRanges, negated);
        }

        private static ImmutableArray<(int Lo, int Hi)> ExpandRanges(CharClassNode node)
        {
            if (!node.Negated)
                return node.Ranges;

            return AstBuilderHelpers.SubtractRanges(ImmutableArray.Create((0, UnicodeConstants.MaxCodePoint)), node.Ranges);
        }

        private List<(int Lo, int Hi)> ParseClassItems(bool negated)
        {
            var ranges = new List<(int Lo, int Hi)>();

            while (_cursor.Current.Kind is not RegexTokenKind.RBracket and not RegexTokenKind.End)
            {
                if (_cursor.Current.Kind == RegexTokenKind.Hyphen && _cursor.Peek(1).Kind == RegexTokenKind.LBracket && ranges.Count > 0)
                    break;

                RegexToken leftToken = _cursor.Current;
                (ImmutableArray<(int Lo, int Hi)> leftRanges, bool leftIsSingle, int leftCodePoint, bool leftFromXml, int leftOffset) = ParseClassItem();

                // unescaped ^ in a negative group is only valid as the first item of the remaining positive group
                bool leftIsUnescapedCaret = leftToken.Kind == RegexTokenKind.Caret;
                if (negated && leftIsUnescapedCaret && ranges.Count != 0)
                    throw ParserExceptionFactory.UnexpectedToken(leftToken);

                bool canContinueRange = _cursor.Current.Kind == RegexTokenKind.Hyphen &&
                    _cursor.Peek(1).Kind is not RegexTokenKind.RBracket and not RegexTokenKind.End and not RegexTokenKind.LBracket;

                if (negated && leftIsUnescapedCaret && canContinueRange)
                    throw ParserExceptionFactory.UnexpectedToken(leftToken);

                if (leftFromXml && !canContinueRange)
                    throw ParserExceptionFactory.UnexpectedToken(_cursor.Peek(0));

                if (canContinueRange)
                {
                    if (!leftIsSingle)
                        throw ParserExceptionFactory.UnexpectedToken(_cursor.Peek(0));

                    _ = _cursor.Consume();
                    RegexToken rightToken = _cursor.Current;
                    (ImmutableArray<(int Lo, int Hi)> rightRanges, bool rightIsSingle, int rightCodePoint, bool _, int __) = ParseClassItem();
                    if (!rightIsSingle || rightRanges.Length != 1 || rightRanges[0].Lo != rightRanges[0].Hi)
                        throw ParserExceptionFactory.UnexpectedToken(_cursor.Peek(0));

                    if (negated && rightToken.Kind == RegexTokenKind.Caret)
                        throw ParserExceptionFactory.UnexpectedToken(rightToken);

                    if (leftCodePoint > rightCodePoint)
                        throw ParserExceptionFactory.CharacterRangeStartGreaterThanEnd(leftOffset);

                    ranges.Add((leftCodePoint, rightCodePoint));
                    continue;
                }

                ranges.AddRange(leftRanges);
            }

            return ranges;
        }

        private (ImmutableArray<(int Lo, int Hi)> Ranges, bool IsSingle, int SingleCodePoint, bool FromXml, int Offset) ParseClassItem()
        {
            RegexToken token = _cursor.Current;
            switch (token.Kind)
            {
                case RegexTokenKind.LiteralChar:
                    _ = _cursor.Consume();
                    int literal = token.CharValue ?? throw new InvalidOperationException("Literal token missing CharValue.");
                    return (AstBuilderHelpers.RangesForSingleCodePoint(literal), true, literal, false, token.OriginalOffset);

                case RegexTokenKind.SingleCharEscape:
                    _ = _cursor.Consume();
                    int escaped = token.CharValue ?? throw new InvalidOperationException("Single escape token missing CharValue.");
                    return (AstBuilderHelpers.RangesForSingleCodePoint(escaped), true, escaped, false, token.OriginalOffset);

                case RegexTokenKind.Hyphen:
                    _ = _cursor.Consume();
                    return (AstBuilderHelpers.RangesForSingleCodePoint('-'), true, '-', false, token.OriginalOffset);

                case RegexTokenKind.Caret:
                    _ = _cursor.Consume();
                    return (AstBuilderHelpers.RangesForSingleCodePoint('^'), true, '^', false, token.OriginalOffset);

                case RegexTokenKind.MultiCharEscape:
                    _ = _cursor.Consume();
                    return (UnicodeClassEvaluator.ResolveMultiCharEscape(token.CharValue ?? throw new InvalidOperationException("Multi escape token missing CharValue.")), false, 0, false, token.OriginalOffset);

                case RegexTokenKind.CategoryEscape:
                case RegexTokenKind.ComplementEscape:
                    _ = _cursor.Consume();
                    CharClassNode classNode = ParseUnicodeEscapeAtom(token);
                    return (ExpandRanges(classNode), false, 0, false, token.OriginalOffset);



                case RegexTokenKind.BackReference:
                    throw ParserExceptionFactory.UnexpectedToken(token);

                default:
                    throw ParserExceptionFactory.UnexpectedToken(token);
            }
        }



        private int ReadQuantifierBound(RegexToken token, int offset)
        {
            if (token.IntValue.HasValue)
                return token.IntValue.Value;

            throw new RegexCompilationLimitExceededException(
                RegexCompilationLimit.MaxQuantifierBound,
                $"Quantifier bound at offset {offset.ToString(CultureInfo.InvariantCulture)} exceeds the configured limit of {_options.MaxQuantifierBound.ToString(CultureInfo.InvariantCulture)}.");
        }

        private static int ReadBackReference(RegexToken token)
        {
            if (int.TryParse(token.Lexeme.Span[1..], NumberStyles.None, CultureInfo.InvariantCulture, out int value))
                return value;

            throw ParserExceptionFactory.BackReferenceInvalid(0, token.OriginalOffset);
        }

        private static bool IsQuantifierStart(RegexTokenKind kind) =>
            kind is RegexTokenKind.Question or RegexTokenKind.Star or RegexTokenKind.Plus or RegexTokenKind.LBrace;
    }
}
