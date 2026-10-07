namespace HoboXslt.Core;

public enum DiagnosticKind
{
    CompileError,
    RuntimeError,
    Message
}

public sealed record Diagnostic(DiagnosticKind Kind, string? File, int? Line, string Text);
