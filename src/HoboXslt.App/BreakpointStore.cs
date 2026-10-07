using HoboXslt.Core;

namespace HoboXslt.App;

public sealed class BreakpointStore
{
    public Dictionary<string, HashSet<int>> Lines { get; } = new(FileKey.Comparer);

    public HashSet<Breakpoint> Unbound { get; } = [];

    public IEnumerable<Breakpoint> All => Lines.SelectMany(file => file.Value.Select(line => new Breakpoint(file.Key, line)));
}
