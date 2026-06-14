using Bleeb.Emulator.Api.Models;

namespace Bleeb.Emulator.Api.Services;

public sealed class BleebAccountStore
{
    private readonly Dictionary<string, BleebAccountEndpoints> _accounts =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Lock _lock = new();

    public void Upsert(string account, string bar, string baz)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(account);

        var endpoints = new BleebAccountEndpoints
        {
            Bar = bar,
            Baz = baz,
        };

        lock (_lock)
        {
            _accounts[account] = endpoints;
        }
    }

    public bool TryGet(string account, out BleebAccountEndpoints endpoints)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(account);

        lock (_lock)
        {
            return _accounts.TryGetValue(account, out endpoints!);
        }
    }
}
