namespace HoboXslt.Core.Tests;

public sealed class SettingsTests : IDisposable
{
    private readonly TempFiles _files = new();

    public void Dispose() => _files.Dispose();

    [Fact]
    public void Update_WhenLanguageChanges_ThenWordWrapIsKept()
    {
        // Arrange
        var path = _files.Write("settings.json", "{}");
        Settings.Update(path, settings => settings with { XsltWordWrap = true });

        // Act
        Settings.Update(path, settings => settings with { LanguageName = "English" });

        // Assert
        Assert.Equal(new Settings("English", XsltWordWrap: true), Settings.Load(path));
    }

    [Fact]
    public void Load_WhenLayoutWasSaved_ThenLayoutIsLoaded()
    {
        // Arrange
        var path = _files.Write("settings.json", "{}");
        var layout = new Layout(1400, 800.5, new PaneLayout(300, 520.25, 310, 240, false, true), new PaneLayout(250, 600, 400, 300, true, false));
        new Settings(Layout: layout).Save(path);

        // Act
        var loaded = Settings.Load(path).Layout;

        // Assert
        Assert.Equal(layout, loaded);
    }
}
