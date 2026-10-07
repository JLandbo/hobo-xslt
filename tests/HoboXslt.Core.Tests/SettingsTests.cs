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

    [Fact]
    public void Load_WhenLayoutHasTheOldFlatShape_ThenSizeIsKeptWithoutPanes()
    {
        // Arrange
        var path = _files.Write("settings.json", """
            { "layout": { "width": 1300, "height": 760, "xmlWidth": 400, "xsltWidth": 500, "outputWidth": 400, "bottomHeight": 180 } }
            """);

        // Act
        var layout = Settings.Load(path).Layout;

        // Assert
        Assert.Equal(new Layout(1300, 760, null, null), layout);
    }

    [Fact]
    public void IsValid_WhenAPaneWidthIsNegative_ThenFalse()
    {
        // Arrange
        var layout = new Layout(1400, 800, new PaneLayout(300, 520, 310, 240, false, false), new PaneLayout(-1, 520, 310, 240, false, false));

        // Act
        var valid = layout.IsValid();

        // Assert
        Assert.False(valid);
    }
}
