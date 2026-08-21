namespace XPath.Regex.NET.Internal.Matcher;

internal static class AnchorEvaluator
{
    public static bool IsStart(ReadOnlySpan<char> input, int position, bool multiLine)
    {
        if (position == 0)
            return true;

        if (!multiLine)
            return false;

        return position > 0 && input[position - 1] == '\n';
    }

    public static bool IsEnd(ReadOnlySpan<char> input, int position, bool multiLine)
    {
        if (position == input.Length)
            return true;

        if (!multiLine)
            return false;

        return position < input.Length && input[position] == '\n';
    }
}
