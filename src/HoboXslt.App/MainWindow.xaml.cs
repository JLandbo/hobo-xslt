using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using HoboXslt.Core;
using HoboXslt.Core.Languages;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;

namespace HoboXslt.App;

public partial class MainWindow : Window
{
    private const string XmlFilter = "Document.XmlFilter";
    private const string XsltFilter = "Document.XsltFilter";

    private readonly Translator _translator;
    private readonly EditorDocument _xml;
    private readonly EditorDocument _xslt;
    private readonly BreakpointStore _breakpoints = new();
    // Texts set from code keep their setters, so a language switch can set them again.
    private readonly Dictionary<object, Action> _texts = [];
    private XPathEvaluator? _xpath;
    private WorkerSession? _session;
    private WorkerSession? _run;
    private WorkerSession? _spareWorker;
    private TextEditor? _activeEditor;
    private Button? _saveButton;

    public MainWindow(Translator translator)
    {
        _translator = translator;
        InitializeComponent();
        ApplySyntaxColors();
        _xml = new(XmlEditor, XmlTitle, XmlFilter, translator);
        _xslt = new(XsltEditor, XsltTitle, XsltFilter, translator);
        XsltMainTab.Tag = _xslt;
        SetUpEditor(XmlEditor);
        SetUpEditor(XsltEditor);
        SetUpEditor(OutputEditor);
        XsltEditor.TextArea.LeftMargins.Insert(0, NewBreakpointMargin(_xslt));
        AddPausedLineRenderer(XsltEditor);
        XmlEditor.TextChanged += (_, _) => UpdateXmlMeta();
        XsltTabs.SelectionChanged += (_, _) => UpdateXsltMeta();
        UpdateXmlMeta();
        UpdateXsltMeta();
        SetText(CaretText, "Status.Caret", 1, 1);
        SetText(StatusText, "Status.Starting");
        AddLanguageItems();
        translator.Changed += UpdateLanguage;
    }

    private void AddLanguageItems()
    {
        foreach (var translation in Translation.All)
        {
            var item = new MenuItem { Header = translation.Name, Tag = translation, IsChecked = translation == _translator.Current };
            item.Click += (_, _) => _translator.Use(translation);
            LanguageMenu.Items.Add(item);
        }
    }

    private void UpdateLanguage()
    {
        foreach (var set in _texts.Values)
            set();
        foreach (var document in XsltDocuments.Append(_xml))
            document.UpdateTitle();
        foreach (var item in LanguageMenu.Items.Cast<MenuItem>())
            item.IsChecked = (Translation)item.Tag == _translator.Current;
    }

    private void SetText(object target, Action set)
    {
        _texts[target] = set;
        set();
    }

    private void SetText(TextBlock block, string key, params object?[] values) =>
        SetText(block, () => block.Text = _translator.Format(key, values));

    private void ApplySyntaxColors()
    {
        var xml = HighlightingManager.Instance.GetDefinition("XML");
        HighlightingBrush Brush(string key) => new SimpleHighlightingBrush(((SolidColorBrush)FindResource(key)).Color);

        xml.GetNamedColor("XmlTag").Foreground = Brush("TagBrush");
        xml.GetNamedColor("AttributeName").Foreground = Brush("AttributeBrush");
        xml.GetNamedColor("AttributeValue").Foreground = Brush("ValueBrush");
        xml.GetNamedColor("Comment").Foreground = Brush("ProcessingInstructionBrush");
        xml.GetNamedColor("XmlDeclaration").Foreground = Brush("ProcessingInstructionBrush");
        xml.GetNamedColor("Entity").Foreground = Brush("ExpressionBrush");

        // The mockup draws the angle brackets of a tag in the faint color, the name in the tag color.
        var brackets = new HighlightingColor { Foreground = Brush("FaintBrush") };
        foreach (var span in xml.MainRuleSet.Spans.Where(span => span.SpanColor == xml.GetNamedColor("XmlTag")))
            span.StartColor = span.EndColor = brackets;
    }

    private void SetUpEditor(TextEditor editor)
    {
        editor.ShowLineNumbers = true;
        // Namespace URIs are attribute values in the mockup, not links.
        editor.Options.EnableHyperlinks = editor.Options.EnableEmailHyperlinks = false;
        foreach (var margin in editor.TextArea.LeftMargins)
        {
            if (DottedLineMargin.IsDottedLineMargin(margin))
                margin.Visibility = Visibility.Hidden;
            else if (margin is LineNumberMargin numbers)
                numbers.Margin = new(0, 0, 7, 0);
        }

        editor.TextArea.GotKeyboardFocus += (_, _) =>
        {
            _activeEditor = editor;
            if (editor != OutputEditor)
                _saveButton = editor == XmlEditor ? SaveXmlButton : SaveXsltButton;
            UpdateEditorStatus();
        };
        editor.TextArea.Caret.PositionChanged += (_, _) => UpdateEditorStatusFor(editor);
        editor.TextChanged += (_, _) => UpdateEditorStatusFor(editor);
    }

    private BreakpointMargin NewBreakpointMargin(EditorDocument document)
    {
        var margin = new BreakpointMargin(document, _breakpoints) { IsEnabled = _session is null };
        margin.Changed += (_, _) => UpdateXsltMeta();
        return margin;
    }

    private void AddPausedLineRenderer(TextEditor editor)
    {
        var view = editor.TextArea.TextView;
        view.BackgroundRenderers.Add(new PausedLineRenderer(view, (Brush)FindResource("PausedLineBrush")));
    }

    private void UpdateEditorStatusFor(TextEditor editor)
    {
        if (editor == _activeEditor)
            UpdateEditorStatus();
    }

    private void UpdateEditorStatus()
    {
        if (_activeEditor is not { } editor)
            return;

        var caret = editor.TextArea.Caret;
        SetText(CaretText, "Status.Caret", caret.Line, caret.Column);
        EncodingText.Text = (editor.Encoding ?? Encoding.UTF8).WebName.ToUpperInvariant();
        LineEndingText.Text = LineEnding(editor.Document);
    }

    private static string LineEnding(TextDocument document)
    {
        var line = document.GetLineByNumber(1);
        return document.GetText(line.EndOffset, line.DelimiterLength) switch
        {
            "\n" => "LF",
            "\r" => "CR",
            _ => "CRLF"
        };
    }

    private void UpdateXmlMeta() => SetText(XmlMeta, XmlEditor.LineCount == 1 ? "Pane.OneLine" : "Pane.Lines", XmlEditor.LineCount);

    private void UpdateXsltMeta()
    {
        if (XsltTabs.SelectedItem is not TabItem { Content: TextEditor editor })
            return;

        var count = BreakpointMarginOf(editor).Count;
        SetText(XsltMeta, count == 1 ? "Pane.OneBreakpoint" : "Pane.Breakpoints", count);
    }

    private IEnumerable<EditorDocument> XsltDocuments => XsltTabs.Items.Cast<TabItem>().Select(Document);

    private IEnumerable<TextEditor> XsltEditors => XsltTabs.Items.Cast<TabItem>().Select(tab => (TextEditor)tab.Content);

    private IEnumerable<BreakpointMargin> BreakpointMargins => XsltEditors.Select(BreakpointMarginOf);

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _spareWorker = NewWorker();
        try
        {
            _xpath = await Task.Run(() => new XPathEvaluator());
        }
        catch (Exception ex)
        {
            SetText(StatusText, "Status.StartFailed", ex.Message);
            return;
        }

        SetText(StatusText, "Status.Ready");
        SetIdle(true);
        XPathButton.IsEnabled = true;
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        (_session ?? _run)?.Stop();
        _spareWorker?.Dispose();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var button = (key, Keyboard.Modifiers) switch
        {
            (Key.F5, ModifierKeys.None) => ContinueButton.IsEnabled ? ContinueButton : DebugButton,
            (Key.F5, ModifierKeys.Shift) => StopButton,
            (Key.F10, ModifierKeys.None) => StepOverButton,
            (Key.F11, ModifierKeys.None) => StepIntoButton,
            (Key.F11, ModifierKeys.Shift) => StepOutButton,
            (Key.S, ModifierKeys.Control) => _saveButton,
            _ => null
        };
        if (button is null)
            return;

        e.Handled = true;
        if (button.IsEnabled)
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }

    private async void XPathBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await EvaluateXPath();
    }

    private async void EvaluateXPath_Click(object sender, RoutedEventArgs e) => await EvaluateXPath();

    private async Task EvaluateXPath()
    {
        if (_xpath is not { } xpath || !XPathButton.IsEnabled)
            return;

        var (expression, xml) = (XPathBox.Text, XmlEditor.Text);
        XPathButton.IsEnabled = false;

        XPathResult result;
        try
        {
            result = await Task.Run(() => xpath.Evaluate(expression, xml));
        }
        catch (Exception ex)
        {
            result = new([], ex.Message);
        }

        SetText(XPathResultBox, () => XPathResultBox.Text =
            result.Error ?? (result.Items.Count == 0 ? _translator.Of("XPath.EmptySequence") : string.Join(Environment.NewLine, result.Items)));
        XPathResultBox.Foreground = (Brush)FindResource(result.Error is null ? "ValueBrush" : "BreakpointBrush");
        XPathButton.IsEnabled = true;
    }

    private void OpenXml_Click(object sender, RoutedEventArgs e) => _xml.Open();

    private void SaveXml_Click(object sender, RoutedEventArgs e) => _xml.Save();

    private void OpenXslt_Click(object sender, RoutedEventArgs e)
    {
        XsltTabs.SelectedItem = XsltMainTab;
        var margin = BreakpointMarginOf(XsltEditor);
        margin.SaveLines();
        if (_xslt.Open())
            margin.LoadLines();
        RunFileText.Text = _xslt.FilePath;
    }

    private void SaveXslt_Click(object sender, RoutedEventArgs e)
    {
        Document((TabItem)XsltTabs.SelectedItem).Save();
        RunFileText.Text = _xslt.FilePath;
    }

    private void Licenses_Click(object sender, RoutedEventArgs e) => new LicensesWindow(_translator) { Owner = this }.ShowDialog();

    private async void Run_Click(object sender, RoutedEventArgs e)
    {
        if (!ReadyToStart())
            return;

        using var session = TakeWorker();
        ShowLive(session);
        _run = session;
        SetIdle(false);
        StopButton.IsEnabled = true;
        SetText(StatusText, "Status.Running");
        ClearResult();

        DebugResult result;
        try
        {
            result = await session.Run(_xslt.FilePath!, _xml.FilePath!);
        }
        catch (Exception ex)
        {
            result = new(DebugOutcome.Failed, null, [new(DiagnosticKind.RuntimeError, null, null, ex.Message)], []);
        }

        _run = null;
        StopButton.IsEnabled = false;
        DiagnosticsTab.IsSelected = true;
        await ShowResult(result);
        SetEndStatus(result.Outcome switch
        {
            DebugOutcome.Completed => "Status.RunCompleted",
            DebugOutcome.Stopped => "Status.RunStopped",
            _ => "Status.RunFailed"
        }, result.Outcome);
        SetIdle(true);
    }

    private async void Debug_Click(object sender, RoutedEventArgs e)
    {
        if (!ReadyToStart())
            return;

        foreach (var margin in BreakpointMargins)
            margin.SaveLines();

        using var session = TakeWorker();
        ShowLive(session);
        session.Paused += snapshot => Dispatcher.BeginInvoke(() => ShowPause(session, snapshot));
        _session = session;
        SetIdle(false);
        SetEditorsReadOnly(true);
        OpenXmlButton.IsEnabled = OpenXsltButton.IsEnabled = false;
        StopButton.IsEnabled = true;
        SetText(StatusText, "Status.Debugging");
        ClearResult();
        ShowUnbound([]);

        DebugResult result;
        try
        {
            result = await session.Debug(_xslt.FilePath!, _xml.FilePath!, _breakpoints.All);
        }
        catch (Exception ex)
        {
            result = new(DebugOutcome.Failed, null, [new(DiagnosticKind.RuntimeError, null, null, ex.Message)], []);
        }

        _session = null;
        SetPaused(false);
        StopButton.IsEnabled = false;
        SetEditorsReadOnly(false);
        OpenXmlButton.IsEnabled = OpenXsltButton.IsEnabled = true;
        VariablesList.ItemsSource = null;
        DiagnosticsTab.IsSelected = true;
        await ShowResult(result);
        // Breakpoints never reached by a stopped or failed run are not known to be unbound.
        ShowUnbound(result.Outcome == DebugOutcome.Completed ? result.UnboundBreakpoints : []);
        SetEndStatus(result.Outcome switch
        {
            DebugOutcome.Completed => "Status.DebugCompleted",
            DebugOutcome.Stopped => "Status.DebugStopped",
            _ => "Status.DebugFailed"
        }, result.Outcome);
        SetIdle(true);
    }

    // A worker started ahead has Saxon loaded by the time Run or Debug needs it.
    private WorkerSession TakeWorker()
    {
        var worker = _spareWorker ?? NewWorker();
        _spareWorker = NewWorker();
        return worker;
    }

    private WorkerSession NewWorker() => new(Environment.ProcessPath!, _translator);

    private void ShowPause(WorkerSession session, PauseSnapshot snapshot)
    {
        if (_session != session)
            return;

        SetPaused(true);
        SetText(PauseText, "Debug.PausedAt", Path.GetFileName(snapshot.File), snapshot.Line);
        SetText(StatusText, "Status.DebugPaused");
        VariablesList.ItemsSource = snapshot.Variables;
        VariablesTab.IsSelected = true;
        if (ShowXsltLine(snapshot.File, snapshot.Line) is { Content: TextEditor editor })
            PausedLineRendererOf(editor).Show(snapshot.Line);
    }

    private void Continue_Click(object sender, RoutedEventArgs e) => Resume(session => session.Continue());

    private void StepInto_Click(object sender, RoutedEventArgs e) => Resume(session => session.StepInto());

    private void StepOver_Click(object sender, RoutedEventArgs e) => Resume(session => session.StepOver());

    private void StepOut_Click(object sender, RoutedEventArgs e) => Resume(session => session.StepOut());

    private void Resume(Action<WorkerSession> command)
    {
        if (_session is not { } session)
            return;

        SetPaused(false);
        SetText(StatusText, "Status.Debugging");
        command(session);
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        if ((_session ?? _run) is not { } session)
            return;

        SetPaused(false);
        StopButton.IsEnabled = false;
        SetText(StatusText, "Status.Stopping");
        session.Stop();
    }

    private void SetIdle(bool idle) => RunButton.IsEnabled = DebugButton.IsEnabled = idle;

    private void SetPaused(bool paused)
    {
        ContinueButton.IsEnabled = StepIntoButton.IsEnabled = StepOverButton.IsEnabled = StepOutButton.IsEnabled = paused;
        PauseBadge.Visibility = paused ? Visibility.Visible : Visibility.Collapsed;
        if (!paused)
        {
            foreach (var editor in XsltEditors)
                PausedLineRendererOf(editor).Clear();
        }
    }

    private void SetEditorsReadOnly(bool readOnly)
    {
        XmlEditor.IsReadOnly = readOnly;
        foreach (var editor in XsltEditors)
        {
            editor.IsReadOnly = readOnly;
            // The session copies the breakpoints at start, so toggles during it would not reach it.
            BreakpointMarginOf(editor).IsEnabled = !readOnly;
        }
    }

    private void ShowUnbound(IEnumerable<Breakpoint> unbound)
    {
        _breakpoints.Unbound.Clear();
        _breakpoints.Unbound.UnionWith(unbound);
        foreach (var margin in BreakpointMargins)
            margin.InvalidateVisual();
    }

    private bool ReadyToStart() =>
        XsltDocuments.All(d => ReadyForRun(d, "Run.XsltNotSaved")) && ReadyForRun(_xml, "Run.XmlNotSaved");

    private bool ReadyForRun(EditorDocument document, string notSavedKey)
    {
        if (document.FilePath is null)
        {
            MessageBox.Show(_translator.Of(notSavedKey), "hobo-xslt", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        return !document.IsModified || document.Save();
    }

    private void ShowLive(WorkerSession session)
    {
        session.Reported += diagnostic => Dispatcher.BeginInvoke(() => DiagnosticsList.Items.Add(new DiagnosticRow(diagnostic)));
        session.Written += text => Dispatcher.BeginInvoke(() => OutputEditor.AppendText(text));
    }

    private void ClearResult()
    {
        OutputEditor.Text = "";
        OutputEditor.SyntaxHighlighting = null;
        OutputMeta.Text = "";
        DiagnosticsList.Items.Clear();
    }

    // The live output is already in the editor; only how it is shown is decided when the run ends.
    private async Task ShowResult(DebugResult result)
    {
        var output = OutputEditor.Text;
        // Checking a large output takes a while, so it runs in the background.
        var isXml = await Task.Run(() => IsWellFormedXml(output));
        OutputEditor.SyntaxHighlighting = isXml ? HighlightingManager.Instance.GetDefinition("XML") : null;
        OutputMeta.Text = result.Output is null ? "" : DateTime.Now.ToString("HH:mm");
        DiagnosticsList.Items.Clear();
        foreach (var diagnostic in result.Diagnostics)
            DiagnosticsList.Items.Add(new DiagnosticRow(diagnostic));
    }

    private void SetEndStatus(string key, DebugOutcome outcome)
    {
        var incomplete = outcome != DebugOutcome.Completed && OutputEditor.Document.TextLength > 0;
        SetText(StatusText, () => StatusText.Text = incomplete ? _translator.Format("Status.IncompleteOutput", _translator.Of(key)) : _translator.Of(key));
    }

    private static bool IsWellFormedXml(string text)
    {
        try
        {
            using var reader = XmlReader.Create(new StringReader(text), new() { DtdProcessing = DtdProcessing.Ignore });
            while (reader.Read()) { }
            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }

    private void Diagnostic_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (((ListViewItem)sender).Content is not DiagnosticRow { Diagnostic: { File: { } file, Line: { } line } })
            return;

        if (_xml.IsAt(file))
            GoTo(_xml, line);
        else
            ShowXsltLine(file, line);
    }

    public TabItem? ShowXsltLine(string file, int line)
    {
        var tab = XsltTabs.Items.Cast<TabItem>().FirstOrDefault(t => Document(t).IsAt(file)) ?? OpenXsltTab(file);
        if (tab is null)
            return null;

        XsltTabs.SelectedItem = tab;
        GoTo(Document(tab), line);
        return tab;
    }

    private TabItem? OpenXsltTab(string file)
    {
        var editor = new TextEditor { SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML"), IsReadOnly = _session is not null };
        var title = new TextBlock();
        var document = new EditorDocument(editor, title, XsltFilter, _translator);
        SetUpEditor(editor);
        var margin = NewBreakpointMargin(document);
        editor.TextArea.LeftMargins.Insert(0, margin);
        AddPausedLineRenderer(editor);
        if (!document.Load(file))
            return null;

        margin.LoadLines();
        var tab = new TabItem { Header = title, Content = editor, Tag = document };
        XsltTabs.Items.Add(tab);
        return tab;
    }

    // Deferred: the ListView takes focus back after the double-click handler returns.
    private void GoTo(EditorDocument document, int line) =>
        Dispatcher.BeginInvoke(() => document.GoTo(line), DispatcherPriority.Input);

    private static EditorDocument Document(TabItem tab) => (EditorDocument)tab.Tag;

    private static BreakpointMargin BreakpointMarginOf(TextEditor editor) => editor.TextArea.LeftMargins.OfType<BreakpointMargin>().Single();

    private static PausedLineRenderer PausedLineRendererOf(TextEditor editor) =>
        editor.TextArea.TextView.BackgroundRenderers.OfType<PausedLineRenderer>().Single();
}
