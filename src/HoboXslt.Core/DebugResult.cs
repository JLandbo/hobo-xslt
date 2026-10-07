namespace HoboXslt.Core;

public enum DebugOutcome
{
    Completed,
    Stopped,
    Failed
}

public sealed record DebugResult(
    DebugOutcome Outcome,
    string? Output,
    IReadOnlyList<Diagnostic> Diagnostics,
    IReadOnlyList<Breakpoint> UnboundBreakpoints);
