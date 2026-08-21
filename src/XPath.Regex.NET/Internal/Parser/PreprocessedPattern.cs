namespace XPath.Regex.NET.Internal.Parser;

internal sealed class PreprocessedPattern
{
    public PreprocessedPattern(string effectivePattern, bool isLiteralMode, IReadOnlyList<SourceMapEntry> map, int originalLength)
    {
        EffectivePattern = effectivePattern;
        IsLiteralMode = isLiteralMode;
        Map = map;
        OriginalLength = originalLength;
    }

    public string EffectivePattern { get; }

    public bool IsLiteralMode { get; }

    public IReadOnlyList<SourceMapEntry> Map { get; }

    public int OriginalLength { get; }

    public int ToOriginalOffset(int effectiveOffset)
    {
        if ((uint)effectiveOffset >= (uint)Map.Count)
            throw new ArgumentOutOfRangeException(nameof(effectiveOffset));

        return Map[effectiveOffset].OriginalOffset;
    }

    public int ToOriginalBoundaryOffset(int effectiveOffset)
    {
        if ((uint)effectiveOffset > (uint)EffectivePattern.Length)
            throw new ArgumentOutOfRangeException(nameof(effectiveOffset));

        if (effectiveOffset == EffectivePattern.Length)
            return OriginalLength;

        return ToOriginalOffset(effectiveOffset);
    }
}
