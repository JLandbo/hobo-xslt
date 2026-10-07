using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace HoboXslt.App;

public sealed class BreakpointMargin(EditorDocument document, BreakpointStore store) : AbstractMargin
{
    private const double MarginWidth = 18;
    private const double Radius = 4.5;

    // Anchors keep breakpoints on their lines while the text is edited.
    private readonly List<TextAnchor> _anchors = [];

    public event EventHandler? Changed;

    public int Count => Lines().Count();

    // Loading a file replaces the whole text and deletes the anchors, so the lines are kept in the store per file.
    public void SaveLines()
    {
        if (document.FilePath is { } file)
            store.Lines[file] = [.. Lines()];
    }

    public void LoadLines()
    {
        _anchors.Clear();
        if (document.FilePath is { } file && store.Lines.TryGetValue(file, out var lines))
        {
            foreach (var line in lines.Where(line => line <= Document.LineCount))
                Add(line);
        }
        InvalidateVisual();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    protected override Size MeasureOverride(Size availableSize) => new(MarginWidth, 0);

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) =>
        new PointHitTestResult(this, hitTestParameters.HitPoint);

    protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
    {
        if (oldTextView is not null)
            oldTextView.VisualLinesChanged -= TextView_VisualLinesChanged;
        base.OnTextViewChanged(oldTextView, newTextView);
        if (newTextView is not null)
            newTextView.VisualLinesChanged += TextView_VisualLinesChanged;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (TextView is not { VisualLinesValid: true } view)
            return;

        var brush = (Brush)FindResource("BreakpointBrush");
        var lines = Lines().ToHashSet();
        foreach (var visualLine in view.VisualLines)
        {
            var line = visualLine.FirstDocumentLine.LineNumber;
            if (!lines.Contains(line))
                continue;

            var center = new Point(MarginWidth / 2, visualLine.VisualTop - view.VerticalOffset + visualLine.Height / 2);
            if (document.FilePath is { } file && store.Unbound.Contains(new(file, line)))
                drawingContext.DrawEllipse(null, new Pen(brush, 1.5), center, Radius, Radius);
            else
                drawingContext.DrawEllipse(brush, null, center, Radius, Radius);
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (TextView is not { } view)
            return;

        view.EnsureVisualLines();
        var visualLine = view.GetVisualLineFromVisualTop(e.GetPosition(view).Y + view.VerticalOffset);
        if (visualLine is null)
            return;

        Toggle(visualLine.FirstDocumentLine.LineNumber);
        e.Handled = true;
    }

    private IEnumerable<int> Lines() => _anchors.Where(anchor => !anchor.IsDeleted).Select(anchor => anchor.Line);

    private void Toggle(int line)
    {
        _anchors.RemoveAll(anchor => anchor.IsDeleted);
        if (_anchors.RemoveAll(anchor => anchor.Line == line) == 0)
            Add(line);
        InvalidateVisual();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void Add(int line)
    {
        var anchor = Document.CreateAnchor(Document.GetLineByNumber(line).Offset);
        // A line break typed at the start of the line moves the breakpoint down with the text.
        anchor.MovementType = AnchorMovementType.AfterInsertion;
        _anchors.Add(anchor);
    }

    private void TextView_VisualLinesChanged(object? sender, EventArgs e) => InvalidateVisual();
}
