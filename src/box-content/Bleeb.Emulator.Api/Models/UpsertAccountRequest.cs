namespace Bleeb.Emulator.Api.Models;

public sealed class UpsertAccountRequest
{
    public required string Account { get; init; }

    public required string Bar { get; init; }

    public required string Baz { get; init; }
}
