using System.IO;
using HoboXslt.Core;

namespace HoboXslt.App;

public sealed record DiagnosticRow(Diagnostic Diagnostic)
{
    public string Location => Diagnostic switch
    {
        { File: null } => "",
        { Line: null } => Path.GetFileName(Diagnostic.File),
        _ => $"{Path.GetFileName(Diagnostic.File)}:{Diagnostic.Line}"
    };

    public string Kind => Diagnostic.Kind switch
    {
        DiagnosticKind.CompileError => "Kompileringsfejl",
        DiagnosticKind.RuntimeError => "Kørselsfejl",
        _ => "xsl:message"
    };

    public string Text => Diagnostic.Text;
}
