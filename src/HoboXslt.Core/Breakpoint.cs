namespace HoboXslt.Core;

public sealed record Breakpoint(string File, int Line)
{
    public bool Equals(Breakpoint? other) =>
        other is not null && Line == other.Line && FileKey.Comparer.Equals(File, other.File);

    public override int GetHashCode() => HashCode.Combine(FileKey.Comparer.GetHashCode(File), Line);
}
