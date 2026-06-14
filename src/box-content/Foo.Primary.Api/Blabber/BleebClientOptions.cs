namespace Foo.Primary.Api.Blabber;

public sealed class BleebClientOptions
{
    public const string SectionName = "Bleeb";

    public Uri? BaseUri { get; init; }
}
