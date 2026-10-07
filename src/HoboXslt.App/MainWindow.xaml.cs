using System.IO;
using System.Windows;
using System.Windows.Controls;
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
    private XsltRunner? _runner;
    private XPathEvaluator? _xpath;

    public MainWindow()
    {
        InitializeComponent();
        _xml = new(XmlEditor, XmlTitle, "XML-input", XmlFilter);
        _xslt = new(XsltEditor, XsltTitle, null, XsltFilter);
        XsltMainTab.Tag = _xslt;
    }

    private IEnumerable<EditorDocument> XsltDocuments => XsltTabs.Items.Cast<TabItem>().Select(Document);

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
        RunButton.IsEnabled = true;
        XPathButton.IsEnabled = true;
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
        _xslt.Open();
    }

    private void SaveXslt_Click(object sender, RoutedEventArgs e) => Document((TabItem)XsltTabs.SelectedItem).Save();

    private async void Run_Click(object sender, RoutedEventArgs e)
    {
        if (_runner is not { } runner || !XsltDocuments.All(d => ReadyForRun(d, "XSLT-stylesheetet")) || !ReadyForRun(_xml, "XML-input"))
            return;

        var (xsltPath, xmlPath) = (_xslt.FilePath!, _xml.FilePath!);
        RunButton.IsEnabled = false;
        StatusText.Text = "Kører…";
        ShowOutput(null);
        DiagnosticsList.Items.Clear();

        RunResult result;
        try
        {
            result = await Task.Run(() => runner.Run(xsltPath, xmlPath));
        }
        catch (Exception ex)
        {
            result = new(null, [new(DiagnosticKind.RuntimeError, null, null, ex.Message)]);
        }

        ShowOutput(result.Output);
        foreach (var diagnostic in result.Diagnostics)
            DiagnosticsList.Items.Add(new DiagnosticRow(diagnostic));

        StatusText.Text = result.Output is null ? "Kørsel fejlede" : "Kørsel fuldført";
        RunButton.IsEnabled = true;
    }

    private static bool ReadyForRun(EditorDocument document, string name)
    {
        if (document.FilePath is null)
        {
            MessageBox.Show($"{name} er ikke gemt. Gem filen før kørsel.", "hobo-xslt", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        return !document.IsModified || document.Save();
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
        var editor = new TextEditor { SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML") };
        var title = new TextBlock();
        var document = new EditorDocument(editor, title, null, XsltFilter);
        if (!document.Load(file))
            return null;

        var tab = new TabItem { Header = title, Content = editor, Tag = document };
        XsltTabs.Items.Add(tab);
        return tab;
    }

    // Deferred: the ListView takes focus back after the double-click handler returns.
    private void GoTo(EditorDocument document, int line) =>
        Dispatcher.BeginInvoke(() => document.GoTo(line), DispatcherPriority.Input);

    private static EditorDocument Document(TabItem tab) => (EditorDocument)tab.Tag;
}
