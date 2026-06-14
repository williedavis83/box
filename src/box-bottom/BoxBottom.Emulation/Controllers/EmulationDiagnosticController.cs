using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoxBottom.Emulation.Controllers;

[ApiController]
[Route("api/emulation")]
public sealed class EmulationDiagnosticController(IEmulationAnchorRepository repository) : ControllerBase
{
    [HttpGet("anchors")]
    [ProducesResponseType<IReadOnlyList<EmulationAnchorInfoResponse>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<EmulationAnchorInfoResponse>> GetAnchors() =>
        Ok(repository.GetAll().Select(anchor => EmulationAnchorInfoResponse.From(anchor, repository)).ToList());
}
