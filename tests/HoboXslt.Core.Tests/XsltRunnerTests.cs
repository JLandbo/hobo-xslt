namespace HoboXslt.Core.Tests;

public sealed class XsltRunnerTests : IDisposable
{
    private readonly TempFiles _files = new();
    private readonly XsltRunner _runner = new();

    public void Dispose() => _files.Dispose();

    [Fact]
    public void Run_WhenXsltAndXmlAreValid_ThenReturnsOutput()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output omit-xml-declaration="yes"/>
              <xsl:template match="/">
                <out><xsl:value-of select="/root/a"/></out>
              </xsl:template>
            </xsl:stylesheet>
            """);
        var xml = _files.Write("input.xml", "<root><a>hello</a></root>");

        // Act
        var result = _runner.Run(xslt, xml);

        // Assert
        Assert.Equal("<out>hello</out>", result.Output);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Run_WhenXsltHasSyntaxError_ThenReturnsCompileDiagnosticWithoutOutput()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <xsl:value-of select="1 +"/>
              </xsl:template>
            </xsl:stylesheet>
            """);
        var xml = _files.Write("input.xml", "<root/>");

        // Act
        var result = _runner.Run(xslt, xml);

        // Assert
        Assert.Null(result.Output);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal((DiagnosticKind.CompileError, xslt, 3), (diagnostic.Kind, diagnostic.File, diagnostic.Line));
    }

    [Fact]
    public void Run_WhenXslMessage_ThenReturnsMessageDiagnostic()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <xsl:message select="'hi'"/>
                <out/>
              </xsl:template>
            </xsl:stylesheet>
            """);
        var xml = _files.Write("input.xml", "<root/>");

        // Act
        var result = _runner.Run(xslt, xml);

        // Assert
        Assert.NotNull(result.Output);
        Assert.Equal([new Diagnostic(DiagnosticKind.Message, xslt, 3, "hi")], result.Diagnostics);
    }

    [Fact]
    public void Run_WhenRuntimeError_ThenReturnsRuntimeDiagnosticWithoutOutput()
    {
        // Arrange
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <xsl:value-of select="error((), string(/root))"/>
              </xsl:template>
            </xsl:stylesheet>
            """);
        var xml = _files.Write("input.xml", "<root>boom</root>");

        // Act
        var result = _runner.Run(xslt, xml);

        // Assert
        Assert.Null(result.Output);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal((DiagnosticKind.RuntimeError, xslt, 3, "boom"), (diagnostic.Kind, diagnostic.File, diagnostic.Line, diagnostic.Text));
    }

    [Fact]
    public void Run_WhenIncludedStylesheetHasError_ThenDiagnosticPointsToIncludedFile()
    {
        // Arrange
        var included = _files.Write("included.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template name="t">
                <xsl:value-of select="1 +"/>
              </xsl:template>
            </xsl:stylesheet>
            """);
        var xslt = _files.Write("main.xsl", """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:include href="included.xsl"/>
            </xsl:stylesheet>
            """);
        var xml = _files.Write("input.xml", "<root/>");

        // Act
        var result = _runner.Run(xslt, xml);

        // Assert
        Assert.Null(result.Output);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal((DiagnosticKind.CompileError, included, 3), (diagnostic.Kind, diagnostic.File, diagnostic.Line));
    }
}
