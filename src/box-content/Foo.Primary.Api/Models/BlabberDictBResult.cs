namespace Foo.Primary.Api.Models;

public sealed class BlabberDictBResult
{
    public Dictionary<string, BlabberResult> Accounts { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}
