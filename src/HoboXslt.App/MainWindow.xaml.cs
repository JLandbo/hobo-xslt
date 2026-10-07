using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using HoboXslt.Core;
using ICSharpCode.AvalonEdit.Highlighting;

namespace HoboXslt.App;

public partial class MainWindow : Window
{
    private const string XmlFilter = "XML-filer (*.xml)|*.xml|Alle filer (*.*)|*.*";
    private const string XsltFilter = "XSLT-filer (*.xsl;*.xslt)|*.xsl;*.xslt|Alle filer (*.*)|*.*";

    private readonly EditorDocument _xml;
    private readonly EditorDocument _xslt;
    private XsltRunner? _runner;

    public MainWindow()
    {
        InitializeComponent();
        _xml = new(XmlEditor, XmlTitle, "XML-input", XmlFilter);
        _xslt = new(XsltEditor, XsltTitle, "XSLT", XsltFilter);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _runner = await Task.Run(() => new XsltRunner());
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Saxon kunne ikke starte: {ex.Message}";
            return;
        }

        StatusText.Text = "Klar";
        RunButton.IsEnabled = true;
    }

    private void OpenXml_Click(object sender, RoutedEventArgs e) => _xml.Open();

    private void SaveXml_Click(object sender, RoutedEventArgs e) => _xml.Save();

    private void OpenXslt_Click(object sender, RoutedEventArgs e) => _xslt.Open();

    private void SaveXslt_Click(object sender, RoutedEventArgs e) => _xslt.Save();

    private async void Run_Click(object sender, RoutedEventArgs e)
    {
        if (_runner is not { } runner || !ReadyForRun(_xslt, "XSLT-stylesheetet") || !ReadyForRun(_xml, "XML-input"))
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

        var document = _xml.IsAt(file) ? _xml : _xslt;
        if (!document.IsAt(file) && !document.Load(file))
            return;

        // Deferred: the ListView takes focus back after the double-click handler returns.
        Dispatcher.BeginInvoke(() => document.GoTo(line), DispatcherPriority.Input);
    }
}
