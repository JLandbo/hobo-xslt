using HoboXslt.Core.Languages;

namespace HoboXslt.Core.Tests;

public sealed class WorkerSessionTests : IDisposable
{
    private static readonly string WorkerPath = Path.ChangeExtension(typeof(Program).Assembly.Location, ".exe");
    private static readonly TimeSpan StopTime = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(1);

    // The looping instruction is on line 3 in both.
    private const string EndlessXPath = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/">
            <r><xsl:value-of select="count(for $i in 1 to 2000000000, $j in 1 to 2000000000 return $j)"/></r>
          </xsl:template>
        </xsl:stylesheet>
        """;

    private const string EndlessRecursion = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/">
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
        var run = session.Run(xslt, _xml);
        // Long enough for the worker to start and enter the loop.
        await Task.Delay(TimeSpan.FromSeconds(3));

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
        var run = session.Debug(xslt, _xml, [new Breakpoint(xslt, 3)]);
        await paused.Task.WaitAsync(Timeout);
        session.Continue();
        await Task.Delay(TimeSpan.FromMilliseconds(500));

        // Act
        var result = await Stop(session, run);

        // Assert
        Assert.Equal((DebugOutcome.Stopped, null), (result.Outcome, result.Output));
        await AssertNextRunCompletes();
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
