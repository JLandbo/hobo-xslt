using System.Text;
using HoboXslt.Core.Languages;

namespace HoboXslt.Core.Tests;

public sealed class WorkerSessionTests : IDisposable
{
    private static readonly string WorkerPath = Path.ChangeExtension(typeof(Program).Assembly.Location, ".exe");
    private static readonly TimeSpan StopTime = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(1);

    // Line 3 reports that the loop starts, so a test can stop it at once instead of letting it burn CPU; the looping instruction is on line 4 in both.
    private const string EndlessXPath = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/">
            <xsl:message>looping</xsl:message>
            <r><xsl:value-of select="count(for $i in 1 to 2000000000, $j in 1 to 2000000000 return $j)"/></r>
          </xsl:template>
        </xsl:stylesheet>
        """;

    private const string EndlessRecursion = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/">
            <xsl:message>looping</xsl:message>
            <r><xsl:call-template name="t"><xsl:with-param name="n" select="0"/></xsl:call-template></r>
          </xsl:template>
          <xsl:template name="t">
            <xsl:param name="n"/>
            <xsl:call-template name="t"><xsl:with-param name="n" select="$n + 1"/></xsl:call-template>
          </xsl:template>
        </xsl:stylesheet>
        """;

    private readonly TempFiles _files = new();
    private readonly Translator _translator = new(Translation.Danish);
    private readonly string _xml;

    public WorkerSessionTests() => _xml = _files.Write("input.xml", "<root/>");

    public void Dispose() => _files.Dispose();

    [Theory]
    [InlineData(EndlessXPath)]
    [InlineData(EndlessRecursion)]
    public async Task Run_WhenStoppedWhileEndless_ThenStopsWithoutOutputAndNextRunCompletes(string stylesheet)
    {
        // Arrange
        var xslt = _files.Write("main.xsl", stylesheet);
        using var session = new WorkerSession(WorkerPath, _translator);
        var looping = new TaskCompletionSource();
        session.Reported += _ => looping.TrySetResult();
        var run = session.Run(xslt, _xml);
        await looping.Task.WaitAsync(Timeout);

        // Act
        var result = await Stop(session, run);

        // Assert
        Assert.Equal((DebugOutcome.Stopped, null), (result.Outcome, result.Output));
        await AssertNextRunCompletes();
    }

    [Theory]
    [InlineData(EndlessXPath)]
    [InlineData(EndlessRecursion)]
    public async Task Debug_WhenStoppedWhileRunningEndless_ThenStopsWithoutOutputAndNextRunCompletes(string stylesheet)
    {
        // Arrange
        var xslt = _files.Write("main.xsl", stylesheet);
        using var session = new WorkerSession(WorkerPath, _translator);
        var paused = new TaskCompletionSource();
        session.Paused += _ => paused.TrySetResult();
        var run = session.Debug(xslt, _xml, [new Breakpoint(xslt, 4)]);
        await paused.Task.WaitAsync(Timeout);
        // Stopped right after Continue, before endless recursion can end the run on its own.
        session.Continue();

        // Act
        var result = await Stop(session, run);

        // Assert
        Assert.Equal((DebugOutcome.Stopped, null), (result.Outcome, result.Output));
        await AssertNextRunCompletes();
    }

    [Fact]
    public async Task Run_WhenMessageEmittedBeforeEndlessLoop_ThenMessageArrivesBeforeStopAndIsKept()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <xsl:message>hello</xsl:message>
                <r><xsl:value-of select="count(for $i in 1 to 2000000000, $j in 1 to 2000000000 return $j)"/></r>
              </xsl:template>
            </xsl:stylesheet>
            """);
        using var session = new WorkerSession(WorkerPath, _translator);
        var reported = new TaskCompletionSource<Diagnostic>();
        session.Reported += diagnostic => reported.TrySetResult(diagnostic);
        var run = session.Run(xslt, _xml);
        var message = await reported.Task.WaitAsync(Timeout);

        // Act
        var result = await Stop(session, run);

        // Assert
        Assert.Equal(new Diagnostic(DiagnosticKind.Message, xslt, 3, "hello"), message);
        Assert.Equal(DebugOutcome.Stopped, result.Outcome);
        Assert.Equal([message], result.Diagnostics);
    }

    [Fact]
    public async Task Run_WhenOutputWrittenBeforeEndlessLoop_ThenOutputArrivesBeforeStop()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output omit-xml-declaration="yes"/>
              <xsl:template match="/">
                <r><before/><xsl:value-of select="count(for $i in 1 to 2000000000, $j in 1 to 2000000000 return $j)"/></r>
              </xsl:template>
            </xsl:stylesheet>
            """);
        using var session = new WorkerSession(WorkerPath, _translator);
        var output = new StringBuilder();
        var arrived = new TaskCompletionSource();
        session.Written += text =>
        {
            output.Append(text);
            if (output.ToString().Contains("<before/>"))
                arrived.TrySetResult();
        };
        var run = session.Run(xslt, _xml);
        await arrived.Task.WaitAsync(Timeout);

        // Act
        var result = await Stop(session, run);

        // Assert
        Assert.Equal(DebugOutcome.Stopped, result.Outcome);
        Assert.Equal("<r><before/>", output.ToString());
    }

    [Fact]
    public async Task Run_WhenStoppedRightAfterOutput_ThenOutputIsKept()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output omit-xml-declaration="yes"/>
              <xsl:template match="/">
                <xsl:message>warm up</xsl:message>
                <xsl:message select="sum(for $i in 1 to 3000000 return $i)"/>
                <r><before/><xsl:message>written</xsl:message><xsl:value-of select="count(for $i in 1 to 2000000000, $j in 1 to 2000000000 return $j)"/></r>
              </xsl:template>
            </xsl:stylesheet>
            """);
        using var session = new WorkerSession(WorkerPath, _translator);
        var output = new StringBuilder();
        var reported = new TaskCompletionSource();
        session.Written += text => output.Append(text);
        // The first messages warm the worker up, so the stop lands while <before/> still waits for its batch.
        session.Reported += diagnostic =>
        {
            if (diagnostic.Text == "written")
                reported.TrySetResult();
        };
        var run = session.Run(xslt, _xml);
        await reported.Task.WaitAsync(Timeout);

        // Act
        var result = await Stop(session, run);

        // Assert
        Assert.Equal(DebugOutcome.Stopped, result.Outcome);
        Assert.Equal("<r><before/>", output.ToString());
    }

    [Fact]
    public async Task Debug_WhenPausedAfterOutput_ThenOutputHasArrived()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output omit-xml-declaration="yes"/>
              <xsl:template match="/">
                <before/>
                <after/>
              </xsl:template>
            </xsl:stylesheet>
            """);
        using var session = new WorkerSession(WorkerPath, _translator);
        var output = new StringBuilder();
        var paused = new TaskCompletionSource<string>();
        session.Written += text => output.Append(text);
        session.Paused += _ => paused.TrySetResult(output.ToString());

        // Act
        _ = session.Debug(xslt, _xml, [new Breakpoint(xslt, 5)]);
        var outputAtPause = await paused.Task.WaitAsync(Timeout);

        // Assert
        Assert.Equal("<before/>", outputAtPause);
    }

    private static async Task<DebugResult> Stop(WorkerSession session, Task<DebugResult> run)
    {
        Assert.False(run.IsCompleted);
        session.Stop();
        return await run.WaitAsync(StopTime);
    }

    private async Task AssertNextRunCompletes()
    {
        var xslt = _files.Write("next.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output omit-xml-declaration="yes"/>
              <xsl:template match="/"><done/></xsl:template>
            </xsl:stylesheet>
            """);
        using var session = new WorkerSession(WorkerPath, _translator);

        var result = await session.Run(xslt, _xml).WaitAsync(Timeout);

        Assert.Equal("<done/>", result.Output);
    }
}
