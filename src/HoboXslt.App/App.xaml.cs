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
        if (e.Args is [Worker.Argument])
        {
            Worker.Serve();
            Shutdown();
            return;
        }

        var settings = Settings.Load(Settings.DefaultPath);
        var translator = new Translator(Translation.Find(settings.LanguageName));
        UseTexts(translator.Current);
        translator.Changed += () =>
        {
            UseTexts(translator.Current);
            SaveSettings(translator, settings => settings with { LanguageName = translator.Current.Name });
        };
        new MainWindow(translator, settings).Show();
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

    public static void SaveSettings(Translator translator, Func<Settings, Settings> change)
    {
        try
        {
            Settings.Update(Settings.DefaultPath, change);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(translator.Format("Settings.SaveFailed", e.Message), "hobo-xslt", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
