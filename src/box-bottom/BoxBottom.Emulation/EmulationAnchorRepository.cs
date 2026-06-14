namespace BoxBottom.Emulation;

public sealed class EmulationAnchorRepository : IEmulationAnchorRepository
{
    private readonly List<EmulationAnchorInfo> _anchors = [];
    private readonly HashSet<string> _activatedAnchorNames = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<EmulationAnchorInfo> GetAll() => _anchors.ToList();

    public void Add(EmulationAnchorInfo anchor)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        _anchors.Add(anchor);
    }

    public void MarkActivated(string anchorName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorName);
        _activatedAnchorNames.Add(anchorName);
    }

    public bool IsActivated(string anchorName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorName);
        return _activatedAnchorNames.Contains(anchorName);
    }
}
