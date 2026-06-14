using Bleeb.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bleeb.Api.Controllers;

[ApiController]
public class BleebController(IOptions<BleebAccountsOptions> options) : ControllerBase
{
    [HttpGet("bar/{account}")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Bar(string account)
    {
        if (!TryGetEndpoints(account, out var endpoints))
        {
            return NotFound();
        }

        return Content(endpoints.Bar, "text/plain");
    }

    [HttpGet("baz/{account}")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Baz(string account)
    {
        if (!TryGetEndpoints(account, out var endpoints))
        {
            return NotFound();
        }

        return Content(endpoints.Baz, "text/plain");
    }

    private bool TryGetEndpoints(string account, out BleebAccountEndpoints endpoints)
    {
        return options.Value.Accounts.TryGetValue(account, out endpoints!);
    }
}
