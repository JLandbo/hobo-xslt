using System.Text.Json;

namespace HoboXslt.Core;

public sealed record Settings(string? LanguageName = null)
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static string DefaultPath { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "hobo-xslt", "settings.json");

    public static Settings Load(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(path), _options) ?? new();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
        {
            return new();
        }
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, _options));
    }
}
