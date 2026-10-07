using net.sf.saxon.expr;
using net.sf.saxon.expr.instruct;
using net.sf.saxon.lib;
using net.sf.saxon.om;
using net.sf.saxon.trace;

namespace HoboXslt.Core;

internal sealed class DebugTraceListener(DebugSession session, IReadOnlySet<Breakpoint> breakpoints) : TraceListener
{
    private readonly HashSet<Breakpoint> _bound = [];
    private DebugCommand _mode = DebugCommand.Continue;
    private int _modeDepth;
    private Breakpoint? _pausedAt;
    private int _depth;
    private bool _readingVariables;

    public IReadOnlyList<Breakpoint> UnboundBreakpoints => [.. breakpoints.Where(b => !_bound.Contains(b))];

    public void enter(Traceable instruction, java.util.Map properties, XPathContext context)
    {
        if (_readingVariables)
            return;

        session.ThrowIfStopRequested();
        if (instruction is Block)
            return;

        var depth = _depth;
        if (AddsDepth(instruction))
            _depth++;

        var location = instruction.getLocation();
        var file = FileKey.FromSystemId(location?.getSystemId());
        var line = location?.getLineNumber() ?? -1;
        if (file is null || line <= 0)
            return;

        var position = new Breakpoint(file, line);
        var hit = breakpoints.Contains(position);
        if (hit)
            _bound.Add(position);

        // Several instructions often share a line; a breakpoint pauses once per visit to its line, not per instruction.
        if (!position.Equals(_pausedAt))
            _pausedAt = null;

        if (!(hit && _pausedAt is null) && !IsStepTarget(depth))
            return;

        _mode = session.WaitForCommand(new(file, line, ReadVariables(context)));
        _modeDepth = depth;
        _pausedAt = position;
    }

    public void leave(Traceable instruction)
    {
        if (!_readingVariables && AddsDepth(instruction))
            _depth--;
    }

    // A Block only groups a sequence constructor (its location is its first child's), and a LetExpression
    // wraps the instructions after its xsl:variable; counting either would make siblings look like children.
    private static bool AddsDepth(Traceable instruction) => instruction is not (Block or LetExpression);

    private bool IsStepTarget(int depth) => _mode switch
    {
        DebugCommand.StepInto => true,
        DebugCommand.StepOver => depth <= _modeDepth,
        DebugCommand.StepOut => depth < _modeDepth,
        _ => false
    };

    private List<Variable> ReadVariables(XPathContext context)
    {
        var frame = context.getStackFrame();
        var names = frame.getStackFrameMap()?.getVariableMap();
        var values = frame.getStackFrameValues();
        List<Variable> variables = [];
        for (var slot = 0; names is not null && slot < Math.Min(names.size(), values.Length); slot++)
        {
            if (names.get(slot) is StructuredQName name && values[slot] is { } value)
                variables.Add(new(name.getDisplayName(), ReadValue(value)));
        }
        return variables;
    }

    private string? ReadValue(Sequence value)
    {
        _readingVariables = true;
        try
        {
            return value.materialize().getStringValue();
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            _readingVariables = false;
        }
    }

    public void setOutputDestination(Logger stream) { }
    public void open(net.sf.saxon.Controller controller) { }
    public void close() { }
    public void startCurrentItem(Item currentItem) { }
    public void endCurrentItem(Item item) { }
    public void startRuleSearch() { }
    public void endRuleSearch(object rule, net.sf.saxon.trans.Mode mode, Item item) { }
    public object? checkpoint() => null;
    public void recover(object rule, net.sf.saxon.trans.XPathException exception) { }
}
