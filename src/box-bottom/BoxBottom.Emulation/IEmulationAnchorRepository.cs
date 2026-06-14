namespace BoxBottom.Emulation;

public interface IEmulationAnchorRepository
{
    IReadOnlyList<EmulationAnchorInfo> GetAll();

    void Add(EmulationAnchorInfo anchor);

    void MarkActivated(string anchorName);

    bool IsActivated(string anchorName);
}
