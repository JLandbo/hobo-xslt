using System.Text;
using javax.xml.transform.stream;
using net.sf.saxon.expr.parser;
using net.sf.saxon.lib;
using net.sf.saxon.s9api;
using JavaFile = java.io.File;

namespace HoboXslt.Core;

// reported and written are called on the transformation thread as Saxon reports a diagnostic or writes output.
public sealed class XsltRunner(Action<Diagnostic>? reported = null, Action<string>? written = null)
{
    private readonly Processor _processor = new(false);

    public RunResult Run(string xsltPath, string xmlPath, TraceListener? traceListener = null) =>
        Run(xsltPath, xmlPath, traceListener, []);

    internal RunResult Run(string xsltPath, string xmlPath, TraceListener? traceListener, List<Diagnostic> diagnostics)
    {
        void Add(Diagnostic diagnostic)
        {
            diagnostics.Add(diagnostic);
            reported?.Invoke(diagnostic);
        }

        var compiler = _processor.newXsltCompiler();
        compiler.setErrorReporter(new CompileErrorReporter(Add));
        compiler.setCompileWithTracing(traceListener is not null);
        if (traceListener is not null)
        {
            // Saxon's miscellaneous optimizations inline literal variables, which hides them from the debugger.
            var info = compiler.getUnderlyingCompilerInfo();
            info.setOptimizerOptions(info.getOptimizerOptions().except(new OptimizerOptions(OptimizerOptions.MISCELLANEOUS)));
        }

        XsltExecutable executable;
        try
        {
            executable = compiler.compile(new StreamSource(new JavaFile(xsltPath)));
        }
        catch (SaxonApiException e)
        {
            if (!diagnostics.Any(d => d.Kind == DiagnosticKind.CompileError))
                Add(new(DiagnosticKind.CompileError, FileKey.FromSystemId(e.getSystemId()), ToLine(e.getLineNumber()), e.getMessage()));
            return new(null, diagnostics);
        }

        var transformer = executable.load30();
        transformer.setMessageHandler(new MessageCollector(Add));
        if (traceListener is not null)
            transformer.setTraceListener(traceListener);

        var writer = new OutputWriter(written);
        try
        {
            transformer.transform(new StreamSource(new JavaFile(xmlPath)), transformer.newSerializer(writer));
        }
        catch (SaxonApiException e)
        {
            Add(new(DiagnosticKind.RuntimeError, FileKey.FromSystemId(e.getSystemId()), ToLine(e.getLineNumber()), e.getMessage()));
            return new(null, diagnostics);
        }

        return new(writer.Text, diagnostics);
    }

    private static int? ToLine(int line) => line > 0 ? line : null;

    private static Diagnostic ToDiagnostic(DiagnosticKind kind, Location? location, string text) =>
        new(kind, FileKey.FromSystemId(location?.getSystemId()), ToLine(location?.getLineNumber() ?? -1), text);

    private sealed class CompileErrorReporter(Action<Diagnostic> add) : ErrorReporter
    {
        public void report(XmlProcessingError error)
        {
            if (!error.isWarning())
                add(ToDiagnostic(DiagnosticKind.CompileError, error.getLocation(), error.getMessage()));
        }
    }

    private sealed class OutputWriter(Action<string>? written) : java.io.Writer
    {
        private readonly StringBuilder _text = new();

        public string Text => _text.ToString();

        public override void write(char[] buffer, int offset, int length)
        {
            var text = new string(buffer, offset, length);
            _text.Append(text);
            written?.Invoke(text);
        }

        public override void flush() { }

        public override void close() { }
    }

    private sealed class MessageCollector(Action<Diagnostic> add) : java.util.function.Consumer
    {
        public void accept(object message)
        {
            var m = (Message)message;
            add(ToDiagnostic(DiagnosticKind.Message, m.getLocation(), m.getStringValue()));
        }

        public java.util.function.Consumer andThen(java.util.function.Consumer after) =>
            java.util.function.Consumer.__DefaultMethods.andThen(this, after);
    }
}
