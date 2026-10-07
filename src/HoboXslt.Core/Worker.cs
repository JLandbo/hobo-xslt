using System.Text;
using System.Text.Json;
using HoboXslt.Core.Languages;

namespace HoboXslt.Core;

// Saxon-HE cannot be interrupted from another thread, so each transformation runs in its own worker process that Stop kills.
public static class Worker
{
    public const string Argument = "--worker";

    internal static readonly UTF8Encoding Encoding = new(false);

    public static void Serve()
    {
        var input = new StreamReader(Console.OpenStandardInput(), Encoding);
        var output = new StreamWriter(Console.OpenStandardOutput(), Encoding) { AutoFlush = true };
        // Warms Saxon up while the request is on its way.
        var runner = new XsltRunner();
        if (input.ReadLine() is not { } line)
            return;

        var request = JsonSerializer.Deserialize<WorkerRequest>(line)!;
        DebugSession? session = null;
        if (request.Breakpoints is { } breakpoints)
        {
            session = new DebugSession(request.XsltPath, request.XmlPath, breakpoints, new Translator(Translation.Find(request.Language)));
            session.Paused += snapshot => Write(output, new(Paused: snapshot));
        }

        new Thread(() => ReadCommands(input, session)) { IsBackground = true }.Start();

        var result = session?.Start().Result ?? ToDebugResult(runner.Run(request.XsltPath, request.XmlPath));
        Write(output, new(Result: result));
    }

    private static DebugResult ToDebugResult(RunResult result) =>
        new(result.Output is null ? DebugOutcome.Failed : DebugOutcome.Completed, result.Output, result.Diagnostics, []);

    // The input closes when the app ends, also when it is killed, so a worker in an endless loop never outlives it.
    private static void ReadCommands(TextReader input, DebugSession? session)
    {
        while (input.ReadLine() is { } line)
            session?.Send(Enum.Parse<DebugCommand>(line));

        Environment.Exit(1);
    }

    private static void Write(TextWriter output, WorkerMessage message) => output.WriteLine(JsonSerializer.Serialize(message));
}

// Breakpoints is null for a plain run.
internal sealed record WorkerRequest(string XsltPath, string XmlPath, IReadOnlyList<Breakpoint>? Breakpoints, string Language);

internal sealed record WorkerMessage(PauseSnapshot? Paused = null, DebugResult? Result = null);
