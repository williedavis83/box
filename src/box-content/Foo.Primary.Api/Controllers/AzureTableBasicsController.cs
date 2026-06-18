using BoxBottom.Azure.Table;
using BoxBottom.Azure.Table.Models;
using BoxBottom.Emulation;
using Foo.Primary.Api.Models;
using Foo.Primary.Shared.AzureTable;
using Microsoft.AspNetCore.Mvc;

[assembly: EmulationAnchor(AzureTableKeys.Orders, typeof(IAzureTableService), EmulationAnchorKind.Singleton)]
[assembly: EmulationAnchor(AzureTableKeys.Analytics, typeof(IAzureTableService), EmulationAnchorKind.Singleton)]
[assembly: EmulationAnchor(AzureTableKeys.GeoUsEast, typeof(IAzureTableService), EmulationAnchorKind.Singleton)]
[assembly: EmulationAnchor(AzureTableKeys.GeoEuWest, typeof(IAzureTableService), EmulationAnchorKind.Singleton)]
[assembly: EmulationAnchor(
    AzureTableKeys.GeoReplicas,
    typeof(IReadOnlyDictionary<string, IAzureTableService>),
    EmulationAnchorKind.Dictionary)]

namespace Foo.Primary.Api.Controllers;

[ApiController]
[Route("api/AzureTable")]
public sealed class AzureTableBasicsController(
    [FromKeyedServices(AzureTableKeys.Orders)] IAzureTableService ordersTableService,
    [FromKeyedServices(AzureTableKeys.Analytics)] IAzureTableService analyticsTableService,
    [FromKeyedServices(AzureTableKeys.GeoReplicas)] IReadOnlyDictionary<string, IAzureTableService> geoReplicaTableServices)
    : ControllerBase
{
    private const string DemoTableName = "boxBasics";
    private const string DemoPartitionKey = "demo";
    private const string DemoRowKey = "sample";

    [HttpGet("scenarios")]
    [ProducesResponseType<AzureTableScenariosStatus>(StatusCodes.Status200OK)]
    public ActionResult<AzureTableScenariosStatus> GetScenarios() =>
        Ok(new AzureTableScenariosStatus(
            new AzureTableAccountScenarioStatus(AzureTableKeys.Orders, ordersTableService.GetConfigurationStatus()),
            new AzureTableAccountScenarioStatus(AzureTableKeys.Analytics, analyticsTableService.GetConfigurationStatus()),
            geoReplicaTableServices
                .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
                .Select(entry => new AzureTableGeoReplicaScenarioStatus(
                    entry.Key,
                    entry.Value.GetConfigurationStatus()))
                .ToList()));

    [HttpGet("accounts/{anchorKey}/status")]
    [ProducesResponseType<AzureTableConfigurationStatus>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AzureTableConfigurationStatus> GetAccountStatus(string anchorKey)
    {
        if (!TryResolveAccountService(anchorKey, out var tableService))
        {
            return NotFound();
        }

        return Ok(tableService.GetConfigurationStatus());
    }

    [HttpGet("accounts/{anchorKey}/tables")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ListAccountTables(string anchorKey, CancellationToken cancellationToken)
    {
        if (!TryResolveAccountService(anchorKey, out var tableService))
        {
            return NotFound();
        }

        if (!TryEnsureConfigured(tableService, out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        var tables = await tableService.ListTablesAsync(cancellationToken);
        return Ok(tables);
    }

    [HttpPost("accounts/{anchorKey}/tables/{tableName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> EnsureAccountTable(
        string anchorKey,
        string tableName,
        CancellationToken cancellationToken)
    {
        if (!TryResolveAccountService(anchorKey, out var tableService))
        {
            return NotFound();
        }

        if (!TryEnsureConfigured(tableService, out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        await tableService.CreateTableIfNotExistsAsync(tableName, cancellationToken);
        return NoContent();
    }

    [HttpPut("accounts/{anchorKey}/tables/{tableName}/entities")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpsertAccountEntity(
        string anchorKey,
        string tableName,
        [FromBody] AzureTableUpsertEntityRequest entity,
        CancellationToken cancellationToken)
    {
        if (!TryResolveAccountService(anchorKey, out var tableService))
        {
            return NotFound();
        }

        if (!TryEnsureConfigured(tableService, out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        await tableService.UpsertEntityAsync(tableName, entity, cancellationToken);
        return NoContent();
    }

    [HttpPost("scenarios/geo-demo")]
    [ProducesResponseType<IReadOnlyList<AzureTableGeoDemoResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RunGeoDemo(
        [FromQuery] string message,
        CancellationToken cancellationToken)
    {
        var demoMessage = string.IsNullOrWhiteSpace(message) ? "hello-from-geo-demo" : message;
        var results = new List<AzureTableGeoDemoResult>();

        foreach (var (region, tableService) in geoReplicaTableServices.OrderBy(entry => entry.Key))
        {
            if (!tableService.GetConfigurationStatus().IsConfigured)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        message = $"Azure Table geo replica '{region}' is not configured.",
                    });
            }
        }

        foreach (var (region, tableService) in geoReplicaTableServices.OrderBy(entry => entry.Key))
        {
            await tableService.CreateTableIfNotExistsAsync(DemoTableName, cancellationToken);
            await tableService.UpsertEntityAsync(
                DemoTableName,
                new AzureTableUpsertEntityRequest
                {
                    PartitionKey = DemoPartitionKey,
                    RowKey = DemoRowKey,
                    Properties = new Dictionary<string, object?>
                    {
                        ["message"] = demoMessage,
                        ["region"] = region,
                    },
                },
                cancellationToken);

            results.Add(new AzureTableGeoDemoResult(
                region,
                DemoTableName,
                DemoPartitionKey,
                DemoRowKey));
        }

        return Ok(results);
    }

    [HttpGet("status")]
    [ProducesResponseType<AzureTableConfigurationStatus>(StatusCodes.Status200OK)]
    public ActionResult<AzureTableConfigurationStatus> GetStatus() =>
        Ok(ordersTableService.GetConfigurationStatus());

    [HttpGet("tables")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<IActionResult> ListTables(CancellationToken cancellationToken) =>
        ListAccountTables(AzureTableKeys.Orders, cancellationToken);

    [HttpPost("tables/{tableName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<IActionResult> EnsureTable(string tableName, CancellationToken cancellationToken) =>
        EnsureAccountTable(AzureTableKeys.Orders, tableName, cancellationToken);

    [HttpGet("tables/{tableName}/entities")]
    [ProducesResponseType<IReadOnlyList<AzureTableEntityResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> QueryEntities(
        string tableName,
        [FromQuery] int? maxResults,
        CancellationToken cancellationToken)
    {
        if (!TryEnsureConfigured(ordersTableService, out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        var entities = await ordersTableService.QueryEntitiesAsync(tableName, maxResults, cancellationToken);
        return Ok(entities);
    }

    [HttpGet("tables/{tableName}/entities/{partitionKey}/{rowKey}")]
    [ProducesResponseType<AzureTableEntityResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetEntity(
        string tableName,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken)
    {
        if (!TryEnsureConfigured(ordersTableService, out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        var entity = await ordersTableService.GetEntityAsync(tableName, partitionKey, rowKey, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPut("tables/{tableName}/entities")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public Task<IActionResult> UpsertEntity(
        string tableName,
        [FromBody] AzureTableUpsertEntityRequest entity,
        CancellationToken cancellationToken) =>
        UpsertAccountEntity(AzureTableKeys.Orders, tableName, entity, cancellationToken);

    private bool TryResolveAccountService(string anchorKey, out IAzureTableService tableService)
    {
        if (string.Equals(anchorKey, AzureTableKeys.Orders, StringComparison.OrdinalIgnoreCase))
        {
            tableService = ordersTableService;
            return true;
        }

        if (string.Equals(anchorKey, AzureTableKeys.Analytics, StringComparison.OrdinalIgnoreCase))
        {
            tableService = analyticsTableService;
            return true;
        }

        if (geoReplicaTableServices.TryGetValue(anchorKey, out tableService!))
        {
            return true;
        }

        tableService = null!;
        return false;
    }

    private static bool TryEnsureConfigured(IAzureTableService tableService, out IActionResult notConfiguredResult)
    {
        var status = tableService.GetConfigurationStatus();
        if (status.IsConfigured)
        {
            notConfiguredResult = new OkResult();
            return true;
        }

        notConfiguredResult = new ObjectResult(new
        {
            message = "Azure Table Storage is not configured.",
            status,
        })
        {
            StatusCode = StatusCodes.Status503ServiceUnavailable,
        };
        return false;
    }
}
