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
}
