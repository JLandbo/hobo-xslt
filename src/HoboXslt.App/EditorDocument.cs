using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;

namespace HoboXslt.App;

public sealed class EditorDocument
{
    private readonly TextEditor _editor;
    private readonly TextBlock _title;
    private readonly string? _label;
    private readonly string _filter;

    public EditorDocument(TextEditor editor, TextBlock title, string? label, string filter)
    {
        _editor = editor;
        _title = title;
        _label = label;
        _filter = filter;
        DependencyPropertyDescriptor.FromProperty(TextEditor.IsModifiedProperty, typeof(TextEditor))
            .AddValueChanged(editor, (_, _) => UpdateTitle());
        UpdateTitle();
    }

    public string? FilePath { get; private set; }

    public bool IsModified => _editor.IsModified;

    public bool IsAt(string file) =>
        FilePath is not null && string.Equals(FilePath, Path.GetFullPath(file), StringComparison.OrdinalIgnoreCase);

    public void Open()
    {
        if (IsModified && !SaveBeforeReplace())
            return;

        var dialog = new OpenFileDialog { Filter = _filter };
        if (dialog.ShowDialog() == true)
            Load(dialog.FileName);
    }

    public bool Load(string path)
    {
        try
        {
            _editor.Load(path);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            ShowError($"Kunne ikke åbne {path}", e);
            return false;
        }

        FilePath = Path.GetFullPath(path);
        UpdateTitle();
        return true;
    }

    public bool Save()
    {
        var path = FilePath;
        if (path is null)
        {
            var dialog = new SaveFileDialog { Filter = _filter };
            if (dialog.ShowDialog() != true)
                return false;
            path = dialog.FileName;
        }

        try
        {
            _editor.Save(path);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            ShowError($"Kunne ikke gemme {path}", e);
            return false;
        }

        FilePath = path;
        UpdateTitle();
        return true;
    }

    public void GoTo(int line)
    {
        _editor.TextArea.Caret.Location = new(line, 1);
        _editor.ScrollToLine(_editor.TextArea.Caret.Line);
        _editor.Focus();
    }

    private bool SaveBeforeReplace() =>
        MessageBox.Show("Dokumentet er ændret og skal gemmes, før en anden fil åbnes.\nGem nu?", "hobo-xslt",
            MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK && Save();

    private void UpdateTitle()
    {
        var name = FilePath is null ? "ikke gemt" : Path.GetFileName(FilePath);
        _title.Text = $"{(_label is null ? "" : $"{_label} · ")}{name}{(_editor.IsModified ? " *" : "")}";
        _title.ToolTip = FilePath;
    }

    private static void ShowError(string message, Exception e) =>
        MessageBox.Show($"{message}:\n{e.Message}", "hobo-xslt", MessageBoxButton.OK, MessageBoxImage.Error);
}
