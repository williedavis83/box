using BoxTop.Meta.Api.Models;
using BoxTop.Meta.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BoxTop.Meta.Api.Controllers;

[ApiController]
[Route("api")]
public class MetaController(
    IOptions<MetaCatalogOptions> catalogOptions,
    IHttpClientFactory httpClientFactory) : ControllerBase
{
    private const string OpenApiDocumentPath = "/openapi/v1.json";

    [HttpGet("apis")]
    [ProducesResponseType<IEnumerable<MetaApiInfo>>(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<MetaApiInfo>> GetApis()
    {
        var apis = catalogOptions.Value.Apis
            .Select(entry => new MetaApiInfo(entry.LogicalName, entry.Url));

        return Ok(apis);
    }

    [HttpGet("open-api/{apiLogicalName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetOpenApi(string apiLogicalName, CancellationToken cancellationToken)
    {
        var entry = catalogOptions.Value.Apis.FirstOrDefault(api =>
            string.Equals(api.LogicalName, apiLogicalName, StringComparison.OrdinalIgnoreCase));

        if (entry is null)
        {
            return NotFound();
        }

        var openApiUrl = $"{entry.Url.TrimEnd('/')}{OpenApiDocumentPath}";
        var httpClient = httpClientFactory.CreateClient();
        using var response = await httpClient.GetAsync(openApiUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                $"Failed to retrieve OpenAPI document from '{entry.LogicalName}'.");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return Content(content, "application/json");
    }
}
