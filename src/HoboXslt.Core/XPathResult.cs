namespace HoboXslt.Core;

public sealed record XPathResult(IReadOnlyList<string> Items, string? Error);
