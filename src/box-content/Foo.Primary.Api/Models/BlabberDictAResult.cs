namespace Foo.Primary.Api.Models;

public sealed class BlabberDictAResult
{
    public Dictionary<string, BlabberResult> Accounts { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}
