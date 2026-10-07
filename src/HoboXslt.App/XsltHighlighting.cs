using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace HoboXslt.App;

public static class XsltHighlighting
{
    public static IHighlightingDefinition Definition { get; } = Load();

    private static IHighlightingDefinition Load()
    {
        using var reader = XmlReader.Create(typeof(XsltHighlighting).Assembly.GetManifestResourceStream("Highlighting/XSLT.xshd")!);
        return HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }
}
