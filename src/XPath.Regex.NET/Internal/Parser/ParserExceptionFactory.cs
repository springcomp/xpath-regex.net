using System.Globalization;
using XPath.Regex.NET.Errors;

namespace XPath.Regex.NET.Internal.Parser;

internal static class ParserExceptionFactory
{
    public static Forx0002Exception UnexpectedToken(RegexToken token) =>
        new($"Unexpected token '{token.Lexeme.ToString()}' at offset {token.OriginalOffset.ToString(CultureInfo.InvariantCulture)}.", token.OriginalOffset);

    public static Forx0002Exception Expected(string expected, int offset) =>
        new($"Expected '{expected}' at offset {offset.ToString(CultureInfo.InvariantCulture)}.", offset);

    public static Forx0002Exception QuantifierHasNoTarget(int offset) =>
        new($"Quantifier has no target at offset {offset.ToString(CultureInfo.InvariantCulture)}.", offset);

    public static Forx0002Exception InvalidQuantifierBounds(int offset) =>
        new($"Invalid quantifier bounds at offset {offset.ToString(CultureInfo.InvariantCulture)}.", offset);

    public static Forx0002Exception ReluctantQuantifierNotSupported(RegexDialect dialect, int offset) =>
        new($"Reluctant quantifier not supported in {dialect} at offset {offset.ToString(CultureInfo.InvariantCulture)}.", offset);

    public static Forx0002Exception BackReferenceInvalid(int groupNumber, int offset) =>
        new($"Back-reference '\\{groupNumber.ToString(CultureInfo.InvariantCulture)}' is not valid at offset {offset.ToString(CultureInfo.InvariantCulture)}.", offset);

    public static Forx0002Exception UnknownUnicodePropertyOrBlock(string name, int offset) =>
        new($"Unknown Unicode property or block '{name}' at offset {offset.ToString(CultureInfo.InvariantCulture)}.", offset);

    public static Forx0002Exception CharacterRangeStartGreaterThanEnd(int offset) =>
        new($"Character range start is greater than end at offset {offset.ToString(CultureInfo.InvariantCulture)}.", offset);

    public static Forx0002Exception FeatureNotSupported(string feature, RegexDialect dialect, int offset) =>
        new($"Feature '{feature}' is not supported in {dialect} at offset {offset.ToString(CultureInfo.InvariantCulture)}.", offset);
}
