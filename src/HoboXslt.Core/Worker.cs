using System.Text;
using System.Text.Json;
using HoboXslt.Core.Languages;

namespace HoboXslt.Core;

// Saxon-HE cannot be interrupted from another thread, so each transformation runs in its own worker process that Stop kills.
public static class Worker
{
    public const string Argument = "--worker";

    internal const string StopCommand = "Stop";

    internal static readonly UTF8Encoding Encoding = new(false);

    private static readonly TimeSpan OutputInterval = TimeSpan.FromMilliseconds(100);

    public static void Serve()
    {
        var input = new StreamReader(Console.OpenStandardInput(), Encoding);
        var output = new StreamWriter(Console.OpenStandardOutput(), Encoding) { AutoFlush = true };
        var gate = new object();
        // Saxon writes output in many small pieces, so they are sent in batches to keep the UI responsive.
        var pending = new StringBuilder();

        void Write(WorkerMessage message)
        {
            lock (gate)
                output.WriteLine(JsonSerializer.Serialize(message));
        }

        void Flush()
        {
            lock (gate)
            {
                if (pending.Length == 0)
                    return;

                Write(new(Output: pending.ToString()));
                pending.Clear();
            }
        }

        // Warms Saxon up while the request is on its way.
        var runner = new XsltRunner(diagnostic => Write(new(Diagnostic: diagnostic)), text =>
        {
            lock (gate)
                pending.Append(text);
        });
        if (input.ReadLine() is not { } line)
            return;

        var request = JsonSerializer.Deserialize<WorkerRequest>(line)!;
        DebugSession? session = null;
        if (request.Breakpoints is { } breakpoints)
        {
            session = new DebugSession(request.XsltPath, request.XmlPath, breakpoints, new Translator(Translation.Find(request.Language)), runner);
            session.Paused += snapshot =>
            {
                Flush();
                Write(new(Paused: snapshot));
            };
        }

        new Thread(() => ReadCommands(input, session, Flush)) { IsBackground = true }.Start();

        using var timer = new Timer(_ => Flush(), null, OutputInterval, OutputInterval);
        var result = session?.Start().Result ?? ToDebugResult(runner.Run(request.XsltPath, request.XmlPath));
        Flush();
        // The output has already been sent in pieces.
        Write(new(Result: result with { Output = null }));
    }

    private static DebugResult ToDebugResult(RunResult result) =>
        new(result.Output is null ? DebugOutcome.Failed : DebugOutcome.Completed, result.Output, result.Diagnostics, []);

    // The input closes when the app ends, also when it is killed, so a worker in an endless loop never outlives it.
    // Stop is read here, beside the transformation, so the batched output is sent even while Saxon is stuck in a loop.
    private static void ReadCommands(TextReader input, DebugSession? session, Action flush)
    {
        while (input.ReadLine() is { } line)
        {
            if (line == StopCommand)
            {
                flush();
                break;
            }

            session?.Send(Enum.Parse<DebugCommand>(line));
        }

        Environment.Exit(1);
    }
}

// Breakpoints is null for a plain run.
internal sealed record WorkerRequest(string XsltPath, string XmlPath, IReadOnlyList<Breakpoint>? Breakpoints, string Language);

internal sealed record WorkerMessage(PauseSnapshot? Paused = null, DebugResult? Result = null, Diagnostic? Diagnostic = null, string? Output = null);
