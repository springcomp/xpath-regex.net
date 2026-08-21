namespace XPath.Regex.NET.Internal.Parser;

internal sealed class PermissiveUnicodeNameValidator : IUnicodeNameValidator
{
    public static readonly PermissiveUnicodeNameValidator Instance = new();

    private PermissiveUnicodeNameValidator()
    {
    }

    public bool IsValidCategory(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return true;
    }

    public bool IsValidBlock(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return true;
    }
}
