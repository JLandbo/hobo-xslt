using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using HoboXslt.Core;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace HoboXslt.App;

public partial class MainWindow : Window
{
    private const string XmlFilter = "XML-filer (*.xml)|*.xml|Alle filer (*.*)|*.*";
    private const string XsltFilter = "XSLT-filer (*.xsl;*.xslt)|*.xsl;*.xslt|Alle filer (*.*)|*.*";

    private readonly EditorDocument _xml;
    private readonly EditorDocument _xslt;
    private readonly BreakpointStore _breakpoints = new();
    private XsltRunner? _runner;
    private XPathEvaluator? _xpath;
    private DebugSession? _session;

    public MainWindow()
    {
        InitializeComponent();
        _xml = new(XmlEditor, XmlTitle, "XML-input", XmlFilter);
        _xslt = new(XsltEditor, XsltTitle, null, XsltFilter);
        XsltMainTab.Tag = _xslt;
        XsltEditor.TextArea.LeftMargins.Insert(0, new BreakpointMargin(_xslt, _breakpoints));
    }

    private IEnumerable<EditorDocument> XsltDocuments => XsltTabs.Items.Cast<TabItem>().Select(Document);

    private IEnumerable<TextEditor> XsltEditors => XsltTabs.Items.Cast<TabItem>().Select(tab => (TextEditor)tab.Content);

    private IEnumerable<BreakpointMargin> BreakpointMargins => XsltEditors.Select(BreakpointMarginOf);

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            (_runner, _xpath) = await Task.Run(() => (new XsltRunner(), new XPathEvaluator()));
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Saxon kunne ikke starte: {ex.Message}";
            return;
        }

        StatusText.Text = "Klar";
        SetIdle(true);
        XPathButton.IsEnabled = true;
    }

    private void Window_Closed(object? sender, EventArgs e) => _session?.Stop();

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

        XPathResultBox.Text = result.Error ?? (result.Items.Count == 0 ? "Tom sekvens" : string.Join(Environment.NewLine, result.Items));
        XPathResultBox.Foreground = result.Error is null ? SystemColors.ControlTextBrush : Brushes.Firebrick;
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
    }

    private void SaveXslt_Click(object sender, RoutedEventArgs e) => Document((TabItem)XsltTabs.SelectedItem).Save();

    private async void Run_Click(object sender, RoutedEventArgs e)
    {
        if (_runner is not { } runner || !ReadyToStart())
            return;

        var (xsltPath, xmlPath) = (_xslt.FilePath!, _xml.FilePath!);
        SetIdle(false);
        StatusText.Text = "Kører…";
        ShowResult(null, []);

        RunResult result;
        try
        {
            result = await Task.Run(() => runner.Run(xsltPath, xmlPath));
        }
        catch (Exception ex)
        {
            result = new(null, [new(DiagnosticKind.RuntimeError, null, null, ex.Message)]);
        }

        ShowResult(result.Output, result.Diagnostics);
        StatusText.Text = result.Output is null ? "Kørsel fejlede" : "Kørsel fuldført";
        SetIdle(true);
    }

    private async void Debug_Click(object sender, RoutedEventArgs e)
    {
        if (!ReadyToStart())
            return;

        foreach (var margin in BreakpointMargins)
            margin.SaveLines();

        var session = new DebugSession(_xslt.FilePath!, _xml.FilePath!, _breakpoints.All);
        session.Paused += snapshot => Dispatcher.BeginInvoke(() => ShowPause(session, snapshot));
        _session = session;
        SetIdle(false);
        SetEditorsReadOnly(true);
        StopButton.IsEnabled = true;
        StatusText.Text = "Debugger kører…";
        ShowResult(null, []);
        ShowUnbound([]);

        DebugResult result;
        try
        {
            result = await session.Start();
        }
        catch (Exception ex)
        {
            result = new(DebugOutcome.Failed, null, [new(DiagnosticKind.RuntimeError, null, null, ex.Message)], []);
        }

        _session = null;
        SetPaused(false);
        StopButton.IsEnabled = false;
        SetEditorsReadOnly(false);
        VariablesList.ItemsSource = null;
        DiagnosticsTab.IsSelected = true;
        ShowResult(result.Output, result.Diagnostics);
        // Breakpoints never reached by a stopped or failed run are not known to be unbound.
        ShowUnbound(result.Outcome == DebugOutcome.Completed ? result.UnboundBreakpoints : []);
        StatusText.Text = result.Outcome switch
        {
            DebugOutcome.Completed => "Debugsession fuldført",
            DebugOutcome.Stopped => "Debugsession stoppet",
            _ => "Debugsession fejlede"
        };
        SetIdle(true);
    }

    private void ShowPause(DebugSession session, PauseSnapshot snapshot)
    {
        if (_session != session)
            return;

        SetPaused(true);
        StatusText.Text = $"Pauset ved {Path.GetFileName(snapshot.File)}:{snapshot.Line}";
        VariablesList.ItemsSource = snapshot.Variables;
        VariablesTab.IsSelected = true;
        ShowXsltLine(snapshot.File, snapshot.Line);
    }

    private void Continue_Click(object sender, RoutedEventArgs e) => Resume(session => session.Continue());

    private void StepInto_Click(object sender, RoutedEventArgs e) => Resume(session => session.StepInto());

    private void StepOver_Click(object sender, RoutedEventArgs e) => Resume(session => session.StepOver());

    private void StepOut_Click(object sender, RoutedEventArgs e) => Resume(session => session.StepOut());

    private void Resume(Action<DebugSession> command)
    {
        if (_session is not { } session)
            return;

        SetPaused(false);
        StatusText.Text = "Debugger kører…";
        command(session);
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        if (_session is not { } session)
            return;

        SetPaused(false);
        StopButton.IsEnabled = false;
        StatusText.Text = "Stopper…";
        session.Stop();
    }

    private void SetIdle(bool idle) => RunButton.IsEnabled = DebugButton.IsEnabled = idle;

    private void SetPaused(bool paused) =>
        ContinueButton.IsEnabled = StepIntoButton.IsEnabled = StepOverButton.IsEnabled = StepOutButton.IsEnabled = paused;

    private void SetEditorsReadOnly(bool readOnly)
    {
        XmlEditor.IsReadOnly = readOnly;
        foreach (var editor in XsltEditors)
            editor.IsReadOnly = readOnly;
    }

    private void ShowUnbound(IEnumerable<Breakpoint> unbound)
    {
        _breakpoints.Unbound.Clear();
        _breakpoints.Unbound.UnionWith(unbound);
        foreach (var margin in BreakpointMargins)
            margin.InvalidateVisual();
    }

    private bool ReadyToStart() =>
        XsltDocuments.All(d => ReadyForRun(d, "XSLT-stylesheetet")) && ReadyForRun(_xml, "XML-input");

    private static bool ReadyForRun(EditorDocument document, string name)
    {
        if (document.FilePath is null)
        {
            MessageBox.Show($"{name} er ikke gemt. Gem filen før kørsel.", "hobo-xslt", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        return !document.IsModified || document.Save();
    }

    private void ShowResult(string? output, IReadOnlyList<Diagnostic> diagnostics)
    {
        ShowOutput(output);
        DiagnosticsList.Items.Clear();
        foreach (var diagnostic in diagnostics)
            DiagnosticsList.Items.Add(new DiagnosticRow(diagnostic));
    }

    private void ShowOutput(string? output)
    {
        OutputEditor.Text = output ?? "";
        OutputEditor.SyntaxHighlighting = output is not null && IsWellFormedXml(output)
            ? HighlightingManager.Instance.GetDefinition("XML")
            : null;
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

    public void ShowXsltLine(string file, int line)
    {
        var tab = XsltTabs.Items.Cast<TabItem>().FirstOrDefault(t => Document(t).IsAt(file)) ?? OpenXsltTab(file);
        if (tab is null)
            return;

        XsltTabs.SelectedItem = tab;
        GoTo(Document(tab), line);
    }

    private TabItem? OpenXsltTab(string file)
    {
        var editor = new TextEditor { SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML"), IsReadOnly = _session is not null };
        var title = new TextBlock();
        var document = new EditorDocument(editor, title, null, XsltFilter);
        var margin = new BreakpointMargin(document, _breakpoints);
        editor.TextArea.LeftMargins.Insert(0, margin);
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
}
