using net.sf.saxon.lib;
using net.sf.saxon.trace;

namespace HoboXslt.Core.Tests;

public sealed class TraceTests : IDisposable
{
    private readonly TempFiles _files = new();
    private readonly XsltRunner _runner = new();

    public void Dispose() => _files.Dispose();

    [Fact]
    public void Run_WhenTracing_ThenEnterReportsLinesInMainAndIncludedStylesheet()
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
        var xml = _files.Write("input.xml", "<root/>");
        var listener = new RecordingTraceListener();

        // Act
        var result = _runner.Run(xslt, xml, listener);

        // Assert
        Assert.Empty(result.Diagnostics);
        Assert.Contains((xslt, 4), listener.Entered);
        Assert.Contains((included, 3), listener.Entered);
    }

    [Fact]
    public async Task Run_WhenEnterBlocks_ThenReleasingFromAnotherThreadFinishesRun()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output omit-xml-declaration="yes"/>
              <xsl:template match="/">
                <out/>
              </xsl:template>
            </xsl:stylesheet>
            """);
        var xml = _files.Write("input.xml", "<root/>");
        var entered = new TaskCompletionSource();
        using var release = new ManualResetEventSlim();
        var listener = new RecordingTraceListener(() =>
        {
            entered.TrySetResult();
            release.Wait();
        });

        // Act
        var run = Task.Run(() => _runner.Run(xslt, xml, listener));
        await entered.Task.WaitAsync(TimeSpan.FromMinutes(1));
        var finishedWhileBlocked = run.IsCompleted;
        release.Set();
        var result = await run.WaitAsync(TimeSpan.FromMinutes(1));

        // Assert
        Assert.False(finishedWhileBlocked);
        Assert.Equal("<out/>", result.Output);
    }

    private sealed class RecordingTraceListener(Action? onEnter = null) : TraceListener
    {
        public List<(string? File, int Line)> Entered { get; } = [];

        public void enter(Traceable instruction, java.util.Map properties, net.sf.saxon.expr.XPathContext context)
        {
            var location = instruction.getLocation();
            Entered.Add((FileKey.FromSystemId(location.getSystemId()), location.getLineNumber()));
            onEnter?.Invoke();
        }

        public void setOutputDestination(Logger stream) { }
        public void open(net.sf.saxon.Controller controller) { }
        public void close() { }
        public void leave(Traceable instruction) { }
        public void startCurrentItem(net.sf.saxon.om.Item currentItem) { }
        public void endCurrentItem(net.sf.saxon.om.Item item) { }
        public void startRuleSearch() { }
        public void endRuleSearch(object rule, net.sf.saxon.trans.Mode mode, net.sf.saxon.om.Item item) { }
        public object? checkpoint() => null;
        public void recover(object rule, net.sf.saxon.trans.XPathException exception) { }
    }
}
