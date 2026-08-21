namespace XPath.Regex.NET.Internal.Parser;

internal interface IUnicodeNameValidator
{
    bool IsValidCategory(string name);

    bool IsValidBlock(string name);
}
