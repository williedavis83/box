using System.Net.Http.Json;
using BoxTop.Meta.Api.Models;using BoxTop.Meta.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BoxTop.Meta.Api.Controllers;

[ApiController]
[Route("api/emulation")]
public sealed class FederatedEmulationDiagnosticController(
    IOptions<MetaCatalogOptions> catalogOptions,
    IHttpClientFactory httpClientFactory,
    IHostEnvironment environment) : ControllerBase
{
    private const string EmulationAnchorsPath = "/api/emulation/anchors";

    [HttpGet("anchors")]
    [ProducesResponseType<FederatedEmulationAnchorsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAnchors(CancellationToken cancellationToken)
    {
        if (environment.IsProduction())
        {
            return NotFound();
        }

        var httpClient = httpClientFactory.CreateClient();
        var groups = new List<FederatedEmulationAnchorGroup>();

        foreach (var api in catalogOptions.Value.Apis.Where(static api =>
                     !string.Equals(api.LogicalName, "meta", StringComparison.OrdinalIgnoreCase)))
        {
            var anchorsUrl = $"{api.Url.TrimEnd('/')}{EmulationAnchorsPath}";

            try
            {
                using var response = await httpClient.GetAsync(anchorsUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    groups.Add(new FederatedEmulationAnchorGroup(
                        api.LogicalName,
                        [],
                        $"Failed to retrieve emulation anchors from '{api.LogicalName}' ({(int)response.StatusCode})."));
                    continue;
                }

                var anchors = await response.Content.ReadFromJsonAsync<List<MetaEmulationAnchorInfo>>(
                    cancellationToken);
                groups.Add(new FederatedEmulationAnchorGroup(
                    api.LogicalName,
                    anchors ?? [],
                    null));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                groups.Add(new FederatedEmulationAnchorGroup(
                    api.LogicalName,
                    [],
                    $"Failed to retrieve emulation anchors from '{api.LogicalName}': {ex.Message}"));
            }
        }

        return Ok(new FederatedEmulationAnchorsResponse(groups));
    }
}
