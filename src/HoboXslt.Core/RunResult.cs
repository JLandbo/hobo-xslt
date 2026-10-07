namespace HoboXslt.Core;

public sealed record RunResult(string? Output, IReadOnlyList<Diagnostic> Diagnostics);
