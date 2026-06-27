namespace BoxBottom.Auth.Contract.Models;

public sealed record ZeroAuthLoginRequest(
    string Provider,
    string ExternalId,
    string DisplayName,
    string Email);
