namespace HoboXslt.Core;

public sealed record PauseSnapshot(string File, int Line, IReadOnlyList<Variable> Variables);

// Value is null when it cannot be inspected (shown as unavailable).
public sealed record Variable(string Name, string? Value, VariableScope Scope);

public enum VariableScope
{
    Local,
    Global
}
