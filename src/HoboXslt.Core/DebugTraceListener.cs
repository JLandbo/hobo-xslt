using System.Runtime.CompilerServices;
using HoboXslt.Core.Languages;
using net.sf.saxon.expr;
using net.sf.saxon.expr.instruct;
using net.sf.saxon.lib;
using net.sf.saxon.om;
using net.sf.saxon.style;
using net.sf.saxon.trace;

namespace HoboXslt.Core;

internal sealed class DebugTraceListener(DebugSession session, IReadOnlySet<Breakpoint> breakpoints, Translator translator) : TraceListener
{
    private readonly HashSet<Breakpoint> _bound = [];
    private DebugCommand _mode = DebugCommand.Continue;
    private int _modeDepth;
    private Breakpoint? _pausedAt;
    private int _pausedDepth;
    private int _depth;
    private bool _readingVariables;

    public IReadOnlyList<Breakpoint> UnboundBreakpoints => [.. breakpoints.Where(b => !_bound.Contains(b))];

    public void enter(Traceable instruction, java.util.Map properties, XPathContext context)
    {
        if (_readingVariables)
            return;

        session.ThrowIfStopRequested();

        var location = instruction.getLocation();
        var file = FileKey.FromSystemId(location?.getSystemId());
        var line = location?.getLineNumber() ?? -1;

        // Aborting before the stack runs out turns infinite recursion into a failed run instead of a process crash.
        if (!RuntimeHelpers.TryEnsureSufficientExecutionStack())
            throw new StackExhaustedException(new(DiagnosticKind.RuntimeError, file, line > 0 ? line : null,
                translator.Of("Debug.TooDeep")));

        if (instruction is Block)
            return;

        var depth = _depth;
        if (AddsDepth(instruction))
            _depth++;

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
        _pausedDepth = _depth;
    }

    public void leave(Traceable instruction)
    {
        if (_readingVariables)
            return;

        if (AddsDepth(instruction))
            _depth--;

        // Leaving a direct child of the paused instruction ends the visit, so a one-line loop body pauses on each iteration.
        if (_depth <= _pausedDepth)
            _pausedAt = null;
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
                variables.Add(new(name.getDisplayName(), ReadValue(value), VariableScope.Local));
        }

        // Globals are evaluated lazily; reading the bindery directly never forces an evaluation, so one not yet evaluated has no value.
        var controller = context.getController();
        var package = (StylesheetPackage)controller.getExecutable().getTopLevelPackage();
        var globals = package.getComponentIndex().values().toArray()
            .Select(component => ((Component)component).getActor())
            .OfType<GlobalVariable>()
            .OrderBy(global => global.getVariableQName().getDisplayName(), StringComparer.Ordinal);
        foreach (var global in globals)
        {
            // Saxon inlines references to a constant global variable, so it never reaches the bindery.
            var value = controller.getBindery(global.getPackageData()).getGlobalVariableValue(global)
                ?? (global is not GlobalParam && global.getBody() is ComponentTracer tracer && tracer.getChild() is Literal literal
                    ? literal.getGroundedValue()
                    : null);
            variables.Add(new(global.getVariableQName().getDisplayName(), value is null ? null : ReadValue(value), VariableScope.Global));
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

internal sealed class StackExhaustedException(Diagnostic diagnostic) : Exception
{
    public Diagnostic Diagnostic { get; } = diagnostic;
}
