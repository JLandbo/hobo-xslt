using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace HoboXslt.App;

// A guide is drawn at the indentation of each enclosing line, so the guides follow the file's own indent width.
public sealed class IndentGuideRenderer : IBackgroundRenderer
{
    private const double DotPeriod = 3;

    private readonly Pen _pen;

    public IndentGuideRenderer(Brush brush)
    {
        _pen = new Pen(brush, 1) { DashStyle = new DashStyle([1, DotPeriod - 1], 0) };
        _pen.Freeze();
    }

    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        var lines = textView.VisualLines;
        if (lines.Count == 0)
            return;

        var document = textView.Document;
        var tabSize = textView.Options.IndentationSize;
        var open = Enclosing(document, lines[0].FirstDocumentLine, tabSize).Select(column => (column, top: lines[0].VisualTop)).ToList();
        // A guide ends at its last indented line, so blank lines after it get none.
        var lastBottom = lines[0].VisualTop;
        foreach (var line in lines)
        {
            if (Indent(document, line.FirstDocumentLine, tabSize) is not { } indent)
                continue;

            for (; open.Count > 0 && open[^1].column >= indent; open.RemoveAt(open.Count - 1))
                DrawGuide(textView, drawingContext, open[^1], lastBottom);
            lastBottom = line.VisualTop + line.Height;
            open.Add((indent, lastBottom));
        }

        foreach (var guide in open)
            DrawGuide(textView, drawingContext, guide, lastBottom);
    }

    private void DrawGuide(TextView textView, DrawingContext drawingContext, (int column, double top) guide, double bottom)
    {
        // Starting on the document's dot grid keeps the dots still while scrolling.
        var top = Math.Floor(guide.top / DotPeriod) * DotPeriod;
        var x = Math.Round(guide.column * textView.WideSpaceWidth - textView.ScrollOffset.X) + 0.5;
        if (bottom <= top || x < 0)
            return;

        var offset = textView.ScrollOffset.Y;
        drawingContext.DrawLine(_pen, new Point(x, top - offset), new Point(x, bottom - offset));
    }

    private static IEnumerable<int> Enclosing(TextDocument document, DocumentLine first, int tabSize)
    {
        var enclosing = new Stack<int>();
        var limit = Indent(document, first, tabSize) ?? int.MaxValue;
        for (var line = first.PreviousLine; line is not null && limit > 0; line = line.PreviousLine)
        {
            if (Indent(document, line, tabSize) is { } indent && indent < limit)
                enclosing.Push(limit = indent);
        }
        return enclosing;
    }

    private static int? Indent(TextDocument document, DocumentLine line, int tabSize)
    {
        var column = 0;
        for (var offset = line.Offset; offset < line.EndOffset; offset++)
        {
            switch (document.GetCharAt(offset))
            {
                case ' ':
                    column++;
                    break;
                case '\t':
                    column += tabSize - column % tabSize;
                    break;
                default:
                    return column;
            }
        }
        return null;
    }
}
