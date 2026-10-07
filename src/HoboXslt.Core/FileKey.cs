namespace HoboXslt.Core;

public static class FileKey
{
    public static string? FromSystemId(string? systemId) =>
        Uri.TryCreate(systemId, UriKind.Absolute, out var uri) && uri.IsFile
            ? Path.GetFullPath(uri.LocalPath)
            : null;
}
