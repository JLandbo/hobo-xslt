using HoboXslt.Core.Languages;

namespace HoboXslt.Core.Tests;

public sealed class LanguageTests : IDisposable
{
    private readonly TempFiles _files = new();

    public void Dispose() => _files.Dispose();

    [Fact]
    public void Texts_WhenComparingLanguages_ThenDanishAndEnglishHaveTheSameKeys()
    {
        // Act
        var danish = Translation.Danish.Texts.Keys.Order();
        var english = Translation.English.Texts.Keys.Order();

        // Assert
        Assert.NotEmpty(danish);
        Assert.Equal(danish, english);
    }

    [Fact]
    public void Of_WhenKeyIsMissing_ThenFallsBackToDanish()
    {
        // Arrange
        var translation = Translation.Parse("Test", """{ "texts": {} }""");

        // Act
        var text = translation.Of("Menu.File");

        // Assert
        Assert.Equal("Filer", text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("{ not json")]
    public void Load_WhenSettingsFileIsMissingOrCorrupt_ThenLanguageIsDanish(string? content)
    {
        // Arrange
        var path = content is null ? Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "settings.json") : _files.Write("settings.json", content);

        // Act
        var translation = Translation.Find(Settings.Load(path).LanguageName);

        // Assert
        Assert.Same(Translation.Danish, translation);
    }
}
