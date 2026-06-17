using BoxBottom.Azure.Table;
using BoxBottom.Azure.Table.Models;
using Microsoft.AspNetCore.Mvc;

namespace Foo.Primary.Api.Controllers;

[ApiController]
[Route("api/AzureTable")]
public sealed class AzureTableBasicsController(IAzureTableService azureTableService) : ControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType<AzureTableConfigurationStatus>(StatusCodes.Status200OK)]
    public ActionResult<AzureTableConfigurationStatus> GetStatus() =>
        Ok(azureTableService.GetConfigurationStatus());

    [HttpGet("tables")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ListTables(CancellationToken cancellationToken)
    {
        if (!TryEnsureConfigured(out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        var tables = await azureTableService.ListTablesAsync(cancellationToken);
        return Ok(tables);
    }

    [HttpPost("tables/{tableName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> EnsureTable(string tableName, CancellationToken cancellationToken)
    {
        if (!TryEnsureConfigured(out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        await azureTableService.CreateTableIfNotExistsAsync(tableName, cancellationToken);
        return NoContent();
    }

    [HttpGet("tables/{tableName}/entities")]
    [ProducesResponseType<IReadOnlyList<AzureTableEntityResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> QueryEntities(
        string tableName,
        [FromQuery] int? maxResults,
        CancellationToken cancellationToken)
    {
        if (!TryEnsureConfigured(out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        var entities = await azureTableService.QueryEntitiesAsync(tableName, maxResults, cancellationToken);
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
        if (!TryEnsureConfigured(out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        var entity = await azureTableService.GetEntityAsync(tableName, partitionKey, rowKey, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPut("tables/{tableName}/entities")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpsertEntity(
        string tableName,
        [FromBody] AzureTableUpsertEntityRequest entity,
        CancellationToken cancellationToken)
    {
        if (!TryEnsureConfigured(out var notConfiguredResult))
        {
            return notConfiguredResult;
        }

        await azureTableService.UpsertEntityAsync(tableName, entity, cancellationToken);
        return NoContent();
    }

    private bool TryEnsureConfigured(out IActionResult notConfiguredResult)
    {
        var status = azureTableService.GetConfigurationStatus();
        if (status.IsConfigured)
        {
            notConfiguredResult = Ok();
            return true;
        }

        notConfiguredResult = StatusCode(
            StatusCodes.Status503ServiceUnavailable,
            new
            {
                message = "Azure Table Storage is not configured.",
                status,
            });
        return false;
    }
}
