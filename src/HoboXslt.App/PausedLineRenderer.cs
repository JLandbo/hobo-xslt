using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace HoboXslt.App;

public sealed class PausedLineRenderer(TextView view, Brush brush, bool fullWidth) : IBackgroundRenderer
{
    private ISegment? _range;

    public KnownLayer Layer => KnownLayer.Background;

    public void Show(ISegment range)
    {
        _range = range;
        view.InvalidateLayer(Layer);
    }

    public void Clear()
    {
        _range = null;
        view.InvalidateLayer(Layer);
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (_range is not { } range || range.EndOffset > textView.Document.TextLength)
            return;

        foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, range))
            drawingContext.DrawRectangle(brush, null, fullWidth ? new Rect(0, rect.Top, textView.ActualWidth, rect.Height) : rect);
    }
}
