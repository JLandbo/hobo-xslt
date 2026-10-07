using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace HoboXslt.App;

public sealed class PausedLineRenderer(TextView view, Brush brush) : IBackgroundRenderer
{
    private int? _line;

    public KnownLayer Layer => KnownLayer.Background;

    public void Show(int line)
    {
        _line = line;
        view.InvalidateLayer(Layer);
    }

    public void Clear()
    {
        _line = null;
        view.InvalidateLayer(Layer);
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (_line is not { } line || line < 1 || line > textView.Document.LineCount)
            return;

        if (textView.GetVisualLine(line) is not { } visualLine)
            return;

        var top = visualLine.VisualTop - textView.ScrollOffset.Y;
        drawingContext.DrawRectangle(brush, null, new Rect(0, top, textView.ActualWidth, visualLine.Height));
    }
}
