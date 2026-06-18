using BoxBottom.Azure.Table.Models;

namespace Foo.Primary.Api.Models;

public sealed record AzureTableAccountScenarioStatus(
    string AnchorKey,
    AzureTableConfigurationStatus Status);

public sealed record AzureTableGeoReplicaScenarioStatus(
    string Region,
    AzureTableConfigurationStatus Status);

public sealed record AzureTableScenariosStatus(
    AzureTableAccountScenarioStatus Orders,
    AzureTableAccountScenarioStatus Analytics,
    IReadOnlyList<AzureTableGeoReplicaScenarioStatus> GeoReplicas);

public sealed record AzureTableGeoDemoResult(
    string Region,
    string TableName,
    string PartitionKey,
    string RowKey);
