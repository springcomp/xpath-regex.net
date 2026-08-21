namespace XPath.Regex.NET.Internal.Parser;

internal readonly record struct ParsedQuantifier(int Min, int? Max, bool Greedy);
