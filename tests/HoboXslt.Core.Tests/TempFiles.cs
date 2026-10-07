namespace HoboXslt.Core.Tests;

public sealed class TempFiles : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("hoboxslt-").FullName;

    public string Write(string name, string content)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);
}
