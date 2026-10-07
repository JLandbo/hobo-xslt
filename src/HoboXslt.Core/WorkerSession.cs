using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using HoboXslt.Core.Languages;

namespace HoboXslt.Core;

// One run or debug session in a worker process; Stop kills the process, which stops any transformation at once.
public sealed class WorkerSession : IDisposable
{
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

    // Raised on a background thread; the debug run stays paused until a step, continue or stop command arrives.
    public event Action<PauseSnapshot>? Paused;

    public Task<DebugResult> Run(string xsltPath, string xmlPath) => Start(new(xsltPath, xmlPath, null, _translator.Current.Name));

    public Task<DebugResult> Debug(string xsltPath, string xmlPath, IEnumerable<Breakpoint> breakpoints) =>
        Start(new(xsltPath, xmlPath, [.. breakpoints], _translator.Current.Name));

    public void Continue() => Send(DebugCommand.Continue);

    public void StepInto() => Send(DebugCommand.StepInto);

    public void StepOver() => Send(DebugCommand.StepOver);

    public void StepOut() => Send(DebugCommand.StepOut);

    public void Stop()
    {
        _stopRequested = true;
        try
        {
            _process.Kill();
        }
        catch (Exception e) when (e is InvalidOperationException or Win32Exception)
        {
            // The worker has already ended.
        }
    }

    public void Dispose()
    {
        Stop();
        _process.Dispose();
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
                return result;
            if (message.Paused is { } snapshot)
                Paused?.Invoke(snapshot);
        }

        return _stopRequested
            ? new(DebugOutcome.Stopped, null, [], [])
            : new(DebugOutcome.Failed, null, [new(DiagnosticKind.RuntimeError, null, null, _translator.Of("Run.WorkerEnded"))], []);
    }
}
