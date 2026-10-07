using HoboXslt.Core.Languages;

namespace HoboXslt.Core;

public sealed class DebugSession(string xsltPath, string xmlPath, IEnumerable<Breakpoint> breakpoints, Translator translator, XsltRunner? runner = null) : IDisposable
{
    private const int DebugStackSize = 16 * 1024 * 1024;

    private readonly HashSet<Breakpoint> _breakpoints = [.. breakpoints];
    private readonly object _gate = new();
    private DebugCommand? _command;
    private volatile bool _stopRequested;
    private Task<DebugResult>? _run;

    // Raised on the debug thread; the run stays paused until a step, continue or stop command arrives.
    public event Action<PauseSnapshot>? Paused;

    public Task<DebugResult> Start() => _run ??= RunOnDebugThread();

    public void Continue() => Send(DebugCommand.Continue);

    public void StepInto() => Send(DebugCommand.StepInto);

    public void StepOver() => Send(DebugCommand.StepOver);

    public void StepOut() => Send(DebugCommand.StepOut);

    public void Stop()
    {
        lock (_gate)
        {
            _stopRequested = true;
            Monitor.PulseAll(_gate);
        }
    }

    public void Dispose() => Stop();

    internal void ThrowIfStopRequested()
    {
        if (_stopRequested)
            throw new DebugStoppedException();
    }

    internal DebugCommand WaitForCommand(PauseSnapshot snapshot)
    {
        lock (_gate)
            _command = null;

        Paused?.Invoke(snapshot);

        lock (_gate)
        {
            while (_command is null && !_stopRequested)
                Monitor.Wait(_gate);

            ThrowIfStopRequested();
            return _command!.Value;
        }
    }

    internal void Send(DebugCommand command)
    {
        lock (_gate)
        {
            _command = command;
            Monitor.PulseAll(_gate);
        }
    }

    private Task<DebugResult> RunOnDebugThread()
    {
        var run = new Task<DebugResult>(Run);
        // Tracing defeats Saxon's tail calls, so recursive stylesheets need a deeper stack than the default 1 MB.
        new Thread(run.RunSynchronously, DebugStackSize) { IsBackground = true }.Start();
        return run;
    }

    private DebugResult Run()
    {
        var listener = new DebugTraceListener(this, _breakpoints, Path.GetFullPath(xmlPath), translator);
        List<Diagnostic> diagnostics = [];
        try
        {
            var result = (runner ?? new XsltRunner()).Run(xsltPath, xmlPath, listener, diagnostics);
            var outcome = result.Output is null ? DebugOutcome.Failed : DebugOutcome.Completed;
            return new(outcome, result.Output, result.Diagnostics, listener.UnboundBreakpoints);
        }
        catch (DebugStoppedException)
        {
            return new(DebugOutcome.Stopped, null, diagnostics, listener.UnboundBreakpoints);
        }
        catch (StackExhaustedException e)
        {
            return new(DebugOutcome.Failed, null, [.. diagnostics, e.Diagnostic], listener.UnboundBreakpoints);
        }
    }

    private sealed class DebugStoppedException : Exception;
}

internal enum DebugCommand
{
    Continue,
    StepInto,
    StepOver,
    StepOut
}
