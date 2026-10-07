using System.Collections.Concurrent;

namespace HoboXslt.Core.Tests;

public sealed class DebugSessionTests : IDisposable
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(1);

    private const string CalledTemplate = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/">
            <xsl:call-template name="t">
              <xsl:with-param name="p" select="42"/>
            </xsl:call-template>
            <after/>
          </xsl:template>
          <xsl:template name="t">
            <xsl:param name="p"/>
            <xsl:variable name="v" select="$p * 2"/>
            <inner><xsl:value-of select="$v"/></inner>
          </xsl:template>
        </xsl:stylesheet>
        """;

    private readonly TempFiles _files = new();
    private readonly BlockingCollection<PauseSnapshot> _pauses = [];

    public void Dispose()
    {
        _pauses.Dispose();
        _files.Dispose();
    }

    [Fact]
    public async Task Start_WhenBreakpointOnLine_ThenPausesAtFileAndLine()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", CalledTemplate);
        using var session = CreateSession(xslt, new Breakpoint(xslt, 11));

        // Act
        var run = session.Start();
        var pause = NextPause();
        session.Continue();
        var result = await run.WaitAsync(Timeout);

        // Assert
        Assert.Equal((xslt, 11), (pause.File, pause.Line));
        Assert.Equal(DebugOutcome.Completed, result.Outcome);
    }

    [Fact]
    public void Start_WhenBreakpointPathDiffersInCase_ThenPauses()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", CalledTemplate);
        using var session = CreateSession(xslt, new Breakpoint(xslt.ToUpperInvariant(), 11));

        // Act
        session.Start();
        var pause = NextPause();

        // Assert
        Assert.Equal(11, pause.Line);
    }

    [Fact]
    public void Start_WhenBreakpointInIncludedStylesheet_ThenPauseReportsIncludedFile()
    {
        // Arrange
        var included = _files.Write("included.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template name="t">
                <inner/>
              </xsl:template>
            </xsl:stylesheet>
            """);
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:include href="included.xsl"/>
              <xsl:template match="/">
                <xsl:call-template name="t"/>
              </xsl:template>
            </xsl:stylesheet>
            """);
        using var session = CreateSession(xslt, new Breakpoint(included, 3));

        // Act
        session.Start();
        var pause = NextPause();

        // Assert
        Assert.Equal((included, 3), (pause.File, pause.Line));
    }

    [Theory]
    [InlineData(3, nameof(DebugSession.StepInto), 8)]
    [InlineData(3, nameof(DebugSession.StepOver), 6)]
    [InlineData(9, nameof(DebugSession.StepOut), 6)]
    public void Step_WhenPausedNearCalledTemplate_ThenPausesAtExpectedLine(int breakpointLine, string command, int expectedLine)
    {
        // Arrange
        var xslt = _files.Write("main.xsl", CalledTemplate);
        using var session = CreateSession(xslt, new Breakpoint(xslt, breakpointLine));
        session.Start();
        NextPause();

        // Act
        Action step = command switch
        {
            nameof(DebugSession.StepInto) => session.StepInto,
            nameof(DebugSession.StepOver) => session.StepOver,
            nameof(DebugSession.StepOut) => session.StepOut,
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };
        step();
        var pause = NextPause();

        // Assert
        Assert.Equal(expectedLine, pause.Line);
    }

    [Fact]
    public async Task Stop_WhenPaused_ThenSessionEndsStopped()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", CalledTemplate);
        using var session = CreateSession(xslt, new Breakpoint(xslt, 11));
        var run = session.Start();
        NextPause();

        // Act
        session.Stop();
        var result = await run.WaitAsync(Timeout);

        // Assert
        Assert.Equal(DebugOutcome.Stopped, result.Outcome);
    }

    [Fact]
    public async Task Stop_WhenRunningInfiniteIterate_ThenSessionEndsStopped()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <xsl:iterate select="1 to 2147483647">
                  <xsl:if test=". lt 0"><never/></xsl:if>
                </xsl:iterate>
              </xsl:template>
            </xsl:stylesheet>
            """);
        using var session = CreateSession(xslt, new Breakpoint(xslt, 3));
        var run = session.Start();
        NextPause();
        session.Continue();

        // Act
        session.Stop();
        var result = await run.WaitAsync(Timeout);

        // Assert
        Assert.Equal(DebugOutcome.Stopped, result.Outcome);
    }

    [Fact]
    public async Task Stop_WhenMessageEmittedBeforeStop_ThenResultKeepsMessage()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <xsl:message>hello</xsl:message>
                <out/>
              </xsl:template>
            </xsl:stylesheet>
            """);
        using var session = CreateSession(xslt, new Breakpoint(xslt, 4));
        var run = session.Start();
        NextPause();

        // Act
        session.Stop();
        var result = await run.WaitAsync(Timeout);

        // Assert
        Assert.Contains(result.Diagnostics, d => d is { Kind: DiagnosticKind.Message, Text: "hello" });
    }

    [Theory]
    [InlineData("for-each")]
    [InlineData("iterate")]
    public async Task Continue_WhenBreakpointOnOneLineLoop_ThenPausesOnEveryIteration(string loop)
    {
        // Arrange
        var xslt = _files.Write("main.xsl", $"""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <xsl:{loop} select="1 to 3"><xsl:value-of select="."/></xsl:{loop}>
              </xsl:template>
            </xsl:stylesheet>
            """);
        using var session = CreateSession(xslt, new Breakpoint(xslt, 3));
        session.Paused += _ => session.Continue();

        // Act
        await session.Start().WaitAsync(Timeout);

        // Assert
        Assert.Equal(3, _pauses.Count);
    }

    [Fact]
    public async Task Start_WhenInfiniteRecursion_ThenSessionEndsFailedWithDiagnostic()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <xsl:call-template name="r"/>
              </xsl:template>
              <xsl:template name="r">
                <xsl:call-template name="r"/>
              </xsl:template>
            </xsl:stylesheet>
            """);
        using var session = CreateSession(xslt);

        // Act
        var result = await session.Start().WaitAsync(Timeout);

        // Assert
        Assert.Equal(DebugOutcome.Failed, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Kind == DiagnosticKind.RuntimeError);
    }

    [Fact]
    public void Paused_WhenVariableHasLiteralValue_ThenSnapshotListsIt()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <xsl:variable name="lit" select="5"/>
                <out><xsl:value-of select="$lit"/></out>
              </xsl:template>
            </xsl:stylesheet>
            """);
        using var session = CreateSession(xslt, new Breakpoint(xslt, 4));

        // Act
        session.Start();
        var pause = NextPause();

        // Assert
        Assert.Contains(new Variable("lit", "5"), pause.Variables);
    }

    [Fact]
    public void Paused_WhenInsideCalledTemplate_ThenSnapshotListsParamAndVariableValues()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", CalledTemplate);
        using var session = CreateSession(xslt, new Breakpoint(xslt, 11));

        // Act
        session.Start();
        var pause = NextPause();

        // Assert
        Assert.Contains(new Variable("p", "42"), pause.Variables);
        Assert.Contains(new Variable("v", "84"), pause.Variables);
    }

    [Fact]
    public void Paused_WhenVariableValueCannotBeRead_ThenValueIsUnavailable()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <xsl:variable name="f" select="if (/root) then concat#2 else substring-before#2"/>
                <out><xsl:value-of select="$f('a', 'b'), $f('c', 'd')"/></out>
              </xsl:template>
            </xsl:stylesheet>
            """);
        using var session = CreateSession(xslt, new Breakpoint(xslt, 4));

        // Act
        session.Start();
        var pause = NextPause();

        // Assert
        Assert.Contains(new Variable("f", null), pause.Variables);
    }

    [Fact]
    public async Task Start_WhenBreakpointOnLineWithoutInstruction_ThenReportedUnboundAndRunCompletes()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", CalledTemplate);
        var unbound = new Breakpoint(xslt, 13);
        using var session = CreateSession(xslt, new Breakpoint(xslt, 11), unbound);
        var run = session.Start();
        NextPause();

        // Act
        session.Continue();
        var result = await run.WaitAsync(Timeout);

        // Assert
        Assert.Equal(DebugOutcome.Completed, result.Outcome);
        Assert.Equal([unbound], result.UnboundBreakpoints);
    }

    [Fact]
    public async Task Dispose_WhenPaused_ThenBackgroundRunEnds()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", CalledTemplate);
        var session = CreateSession(xslt, new Breakpoint(xslt, 11));
        var run = session.Start();
        NextPause();

        // Act
        session.Dispose();
        var result = await run.WaitAsync(Timeout);

        // Assert
        Assert.Equal(DebugOutcome.Stopped, result.Outcome);
    }

    private DebugSession CreateSession(string xslt, params Breakpoint[] breakpoints)
    {
        var session = new DebugSession(xslt, _files.Write("input.xml", "<root/>"), breakpoints);
        session.Paused += _pauses.Add;
        return session;
    }

    private PauseSnapshot NextPause() =>
        _pauses.TryTake(out var pause, Timeout) ? pause : throw new TimeoutException("The session did not pause.");
}
