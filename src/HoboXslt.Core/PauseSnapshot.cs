namespace HoboXslt.Core;

// Context is null when the context item is not an element (or a node inside one) of the XML input.
public sealed record PauseSnapshot(string File, int Line, IReadOnlyList<Variable> Variables, ContextLocation? Context);

// Line and column as Saxon reports them: the position just after the start tag.
public sealed record ContextLocation(int Line, int Column);

// Value is null when it cannot be inspected (shown as unavailable).
public sealed record Variable(string Name, string? Value, VariableScope Scope);

public enum VariableScope
{
    Local,
    Global
}
