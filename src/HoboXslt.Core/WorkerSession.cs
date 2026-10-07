using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using HoboXslt.Core.Languages;

namespace HoboXslt.Core;

// One run or debug session in a worker process; Stop ends the process, which stops any transformation at once.
public sealed class WorkerSession : IDisposable
{
    private static readonly TimeSpan StopGrace = TimeSpan.FromMilliseconds(500);

    private readonly Process _process;
    private readonly Translator _translator;
    private volatile bool _stopRequested;

    // Starts the worker at once, so it can warm up before the run or debug request arrives.
    public WorkerSession(string workerPath, Translator translator)
    {
        _translator = translator;
        var start = new ProcessStartInfo(workerPath, Worker.Argument)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            StandardInputEncoding = Worker.Encoding,
            StandardOutputEncoding = Worker.Encoding
        };
        _process = Process.Start(start)!;
    }

    private readonly List<Diagnostic> _diagnostics = [];
    private readonly StringBuilder _output = new();

    // Raised on a background thread; the debug run stays paused until a step, continue or stop command arrives.
    public event Action<PauseSnapshot>? Paused;

    // Raised on a background thread as the run reports a diagnostic.
    public event Action<Diagnostic>? Reported;

    // Raised on a background thread with each piece of output the run writes.
    public event Action<string>? Written;

    public Task<DebugResult> Run(string xsltPath, string xmlPath) => Start(new(xsltPath, xmlPath, null, _translator.Current.Name));

    public Task<DebugResult> Debug(string xsltPath, string xmlPath, IEnumerable<Breakpoint> breakpoints) =>
        Start(new(xsltPath, xmlPath, [.. breakpoints], _translator.Current.Name));

    public void Continue() => Send(DebugCommand.Continue);

    public void StepInto() => Send(DebugCommand.StepInto);

    public void StepOver() => Send(DebugCommand.StepOver);

    public void StepOut() => Send(DebugCommand.StepOut);

    // The worker sends its remaining output and ends itself; one that does not end in time is killed.
    public void Stop()
    {
        _stopRequested = true;
        Send(Worker.StopCommand);
        _ = Task.Delay(StopGrace).ContinueWith(_ => Kill());
    }

    public void Dispose()
    {
        _stopRequested = true;
        Kill();
        _process.Dispose();
    }

    private void Kill()
    {
        try
        {
            _process.Kill();
        }
        catch (Exception e) when (e is InvalidOperationException or Win32Exception)
        {
            // The worker has already ended.
        }
    }

    private Task<DebugResult> Start(WorkerRequest request)
    {
        Send(JsonSerializer.Serialize(request));
        return ReadResult();
    }

    private void Send(DebugCommand command) => Send(command.ToString());

    private void Send(string line)
    {
        try
        {
            _process.StandardInput.WriteLine(line);
        }
        catch (IOException)
        {
            // The worker has ended; ReadResult reports how.
        }
    }

    private async Task<DebugResult> ReadResult()
    {
        while (await _process.StandardOutput.ReadLineAsync().ConfigureAwait(false) is { } line)
        {
            var message = JsonSerializer.Deserialize<WorkerMessage>(line)!;
            if (message.Result is { } result)
                return result.Outcome == DebugOutcome.Completed ? result with { Output = _output.ToString() } : result;
            if (message.Paused is { } snapshot)
                Paused?.Invoke(snapshot);
            if (message.Diagnostic is { } diagnostic)
            {
                _diagnostics.Add(diagnostic);
                Reported?.Invoke(diagnostic);
            }
            if (message.Output is { } text)
            {
                _output.Append(text);
                Written?.Invoke(text);
            }
        }

        return _stopRequested
            ? new(DebugOutcome.Stopped, null, _diagnostics, [])
            : new(DebugOutcome.Failed, null, [.. _diagnostics, new(DiagnosticKind.RuntimeError, null, null, _translator.Of("Run.WorkerEnded"))], []);
    }
}
