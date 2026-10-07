using System.IO;
using System.Windows;
using HoboXslt.Core;
using HoboXslt.Core.Languages;

namespace HoboXslt.App;

public partial class App : Application
{
    private ResourceDictionary? _texts;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var translator = new Translator(Translation.Find(Settings.Load(Settings.DefaultPath).LanguageName));
        UseTexts(translator.Current);
        translator.Changed += () =>
        {
            UseTexts(translator.Current);
            SaveLanguage(translator);
        };
        new MainWindow(translator).Show();
    }

    // XAML reads its texts as dynamic resources, so swapping the dictionary switches the language at once.
    private void UseTexts(Translation translation)
    {
        if (_texts is not null)
            Resources.MergedDictionaries.Remove(_texts);

        _texts = new();
        foreach (var key in Translation.Danish.Texts.Keys)
            _texts[key] = translation.Of(key);
        Resources.MergedDictionaries.Add(_texts);
    }

    private static void SaveLanguage(Translator translator)
    {
        try
        {
            new Settings(translator.Current.Name).Save(Settings.DefaultPath);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(translator.Format("Settings.SaveFailed", e.Message), "hobo-xslt", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
