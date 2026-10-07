namespace HoboXslt.Core;

public sealed class DebugSession(string xsltPath, string xmlPath, IEnumerable<Breakpoint> breakpoints) : IDisposable
{
    private readonly HashSet<Breakpoint> _breakpoints = [.. breakpoints];
    private readonly object _gate = new();
    private DebugCommand? _command;
    private volatile bool _stopRequested;
    private Task<DebugResult>? _run;

    // Raised on the debug thread; the run stays paused until a step, continue or stop command arrives.
    public event Action<PauseSnapshot>? Paused;

    public Task<DebugResult> Start() =>
        _run ??= Task.Factory.StartNew(Run, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

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

    private void Send(DebugCommand command)
    {
        lock (_gate)
        {
            _command = command;
            Monitor.PulseAll(_gate);
        }
    }

    private DebugResult Run()
    {
        var listener = new DebugTraceListener(this, _breakpoints);
        try
        {
            var result = new XsltRunner().Run(xsltPath, xmlPath, listener);
            var outcome = result.Output is null ? DebugOutcome.Failed : DebugOutcome.Completed;
            return new(outcome, result.Output, result.Diagnostics, listener.UnboundBreakpoints);
        }
        catch (DebugStoppedException)
        {
            return new(DebugOutcome.Stopped, null, [], listener.UnboundBreakpoints);
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
