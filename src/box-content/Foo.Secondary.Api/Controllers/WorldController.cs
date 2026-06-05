using Foo.Secondary.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Foo.Secondary.Api.Controllers;

[ApiController]
[Route("api")]
public class WorldController(IOptions<WorldOptions> options) : ControllerBase
{
    [HttpGet("World")]
    public IActionResult World() => Ok(options.Value.Message);
}
