using Blabber.Lib;

namespace Blabber.Emulator;

public sealed class FakeBlabber : IBlabber
{
    private const string FakePrefix = "Fake ";
    private readonly IBlabber _real;

    public FakeBlabber(IBlabber real)
    {
        ArgumentNullException.ThrowIfNull(real);
        _real = real;
    }

    public async Task<string> Bar(CancellationToken cancellationToken = default) =>
        FakePrefix + await _real.Bar(cancellationToken);

    public async Task<string> Baz(CancellationToken cancellationToken = default) =>
        FakePrefix + await _real.Baz(cancellationToken);
}
