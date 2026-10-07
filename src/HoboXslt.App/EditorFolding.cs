using System.Windows.Controls;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;

namespace HoboXslt.App;

public static class EditorFolding
{
    // The margin and its markers are sized from the font size, so a small one keeps the margin narrow.
    private const double MarginFontSize = 9;
    private static readonly TimeSpan UpdateDelay = TimeSpan.FromMilliseconds(400);

    public static void Install(TextEditor editor)
    {
        var manager = FoldingManager.Install(editor.TextArea);
        var margin = editor.TextArea.LeftMargins.OfType<FoldingMargin>().Single();
        margin.SetValue(TextBlock.FontSizeProperty, MarginFontSize);
        margin.SetResourceReference(FoldingMargin.FoldingMarkerBrushProperty, "FaintBrush");
        margin.SetResourceReference(FoldingMargin.FoldingMarkerBackgroundBrushProperty, "PanelBrush");
        margin.SetResourceReference(FoldingMargin.SelectedFoldingMarkerBrushProperty, "AccentBrush");
        margin.SetResourceReference(FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty, "AccentSoftBrush");

        // Folds are rebuilt after a pause in typing, so a huge file does not stutter on every key.
        var strategy = new XmlFoldingStrategy();
        var timer = new DispatcherTimer { Interval = UpdateDelay };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            strategy.UpdateFoldings(manager, editor.Document);
        };
        editor.TextChanged += (_, _) =>
        {
            timer.Stop();
            timer.Start();
        };
    }

    public static void Reveal(TextEditor editor, int offset)
    {
        if (editor.TextArea.TextView.GetService(typeof(FoldingManager)) is not FoldingManager manager)
            return;

        foreach (var folding in manager.GetFoldingsContaining(offset))
            folding.IsFolded = false;
    }
}
