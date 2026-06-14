using Blabber.Lib;

namespace Foo.Primary.Api.Models;

public sealed class BlabberResult
{
    public required string Account { get; init; }

    public required string Bar { get; init; }

    public required string Baz { get; init; }

    public static async Task<BlabberResult> FromBlabberAsync(
        string account,
        IBlabber blabber,
        CancellationToken cancellationToken = default) =>
        new()
        {
            Account = account,
            Bar = await blabber.Bar(cancellationToken),
            Baz = await blabber.Baz(cancellationToken),
        };
}
