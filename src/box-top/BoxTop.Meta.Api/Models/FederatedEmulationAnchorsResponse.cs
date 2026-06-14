namespace BoxTop.Meta.Api.Models;

public sealed record FederatedEmulationAnchorGroup(
    string ApiLogicalName,
    IReadOnlyList<MetaEmulationAnchorInfo> Anchors,
    string? Error);

public sealed record FederatedEmulationAnchorsResponse(
    IReadOnlyList<FederatedEmulationAnchorGroup> Apis);
