using javax.xml.transform.stream;
using net.sf.saxon.lib;
using net.sf.saxon.s9api;
using JavaFile = java.io.File;
using JavaStringWriter = java.io.StringWriter;

namespace HoboXslt.Core;

public sealed class XsltRunner
{
    private readonly Processor _processor = new(false);

    public RunResult Run(string xsltPath, string xmlPath, TraceListener? traceListener = null)
    {
        List<Diagnostic> diagnostics = [];

        var compiler = _processor.newXsltCompiler();
        compiler.setErrorReporter(new CompileErrorReporter(diagnostics));
        compiler.setCompileWithTracing(traceListener is not null);

        XsltExecutable executable;
        try
        {
            executable = compiler.compile(new StreamSource(new JavaFile(xsltPath)));
        }
        catch (SaxonApiException e)
        {
            if (!diagnostics.Any(d => d.Kind == DiagnosticKind.CompileError))
                diagnostics.Add(new(DiagnosticKind.CompileError, FileKey.FromSystemId(e.getSystemId()), ToLine(e.getLineNumber()), e.getMessage()));
            return new(null, diagnostics);
        }

        var transformer = executable.load30();
        transformer.setMessageHandler(new MessageCollector(diagnostics));
        if (traceListener is not null)
            transformer.setTraceListener(traceListener);

        var writer = new JavaStringWriter();
        try
        {
            transformer.transform(new StreamSource(new JavaFile(xmlPath)), transformer.newSerializer(writer));
        }
        catch (SaxonApiException e)
        {
            diagnostics.Add(new(DiagnosticKind.RuntimeError, FileKey.FromSystemId(e.getSystemId()), ToLine(e.getLineNumber()), e.getMessage()));
            return new(null, diagnostics);
        }

        return new(writer.toString(), diagnostics);
    }

    private static int? ToLine(int line) => line > 0 ? line : null;

    private static Diagnostic ToDiagnostic(DiagnosticKind kind, Location? location, string text) =>
        new(kind, FileKey.FromSystemId(location?.getSystemId()), ToLine(location?.getLineNumber() ?? -1), text);

    private sealed class CompileErrorReporter(List<Diagnostic> diagnostics) : ErrorReporter
    {
        public void report(XmlProcessingError error)
        {
            if (!error.isWarning())
                diagnostics.Add(ToDiagnostic(DiagnosticKind.CompileError, error.getLocation(), error.getMessage()));
        }
    }

    private sealed class MessageCollector(List<Diagnostic> diagnostics) : java.util.function.Consumer
    {
        public void accept(object message)
        {
            var m = (Message)message;
            diagnostics.Add(ToDiagnostic(DiagnosticKind.Message, m.getLocation(), m.getStringValue()));
        }

        public java.util.function.Consumer andThen(java.util.function.Consumer after) =>
            java.util.function.Consumer.__DefaultMethods.andThen(this, after);
    }
}
