using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using HoboXslt.Core.Languages;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;

namespace HoboXslt.App;

public sealed class EditorDocument
{
    private readonly TextEditor _editor;
    private readonly TextBlock _title;
    private readonly string _filterKey;
    private readonly Translator _translator;

    public EditorDocument(TextEditor editor, TextBlock title, string filterKey, Translator translator)
    {
        _editor = editor;
        _title = title;
        _filterKey = filterKey;
        _translator = translator;
        DependencyPropertyDescriptor.FromProperty(TextEditor.IsModifiedProperty, typeof(TextEditor))
            .AddValueChanged(editor, (_, _) => UpdateTitle());
        UpdateTitle();
    }

    public string? FilePath { get; private set; }

    public bool IsModified => _editor.IsModified;

    public bool IsAt(string file) =>
        FilePath is not null && string.Equals(FilePath, Path.GetFullPath(file), StringComparison.OrdinalIgnoreCase);

    public bool Open()
    {
        if (IsModified && !SaveBeforeReplace())
            return false;

        var dialog = new OpenFileDialog { Filter = _translator.Of(_filterKey), Title = _translator.Of("Document.OpenTitle") };
        return dialog.ShowDialog() == true && Load(dialog.FileName);
    }

    public bool Load(string path)
    {
        try
        {
            _editor.Load(path);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            ShowError("Document.OpenFailed", path, e);
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
            var dialog = new SaveFileDialog { Filter = _translator.Of(_filterKey), Title = _translator.Of("Document.SaveTitle") };
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
            ShowError("Document.SaveFailed", path, e);
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
        MessageBox.Show(_translator.Of("Document.SaveBeforeReplace"), "hobo-xslt",
            MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK && Save();

    public void UpdateTitle()
    {
        var name = FilePath is null ? _translator.Of("Document.Unsaved") : Path.GetFileName(FilePath);
        _title.Text = $"{name}{(_editor.IsModified ? " *" : "")}";
        _title.ToolTip = FilePath;
    }

    private void ShowError(string key, string path, Exception e) =>
        MessageBox.Show(_translator.Format(key, path, e.Message), "hobo-xslt", MessageBoxButton.OK, MessageBoxImage.Error);
}
