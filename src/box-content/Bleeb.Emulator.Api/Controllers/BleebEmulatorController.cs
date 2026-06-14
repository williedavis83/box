using Bleeb.Emulator.Api.Models;
using Bleeb.Emulator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bleeb.Emulator.Api.Controllers;

[ApiController]
public class BleebEmulatorController(BleebAccountStore accountStore) : ControllerBase
{
    [HttpPost("account")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpsertAccount([FromBody] UpsertAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Account)
            || string.IsNullOrWhiteSpace(request.Bar)
            || string.IsNullOrWhiteSpace(request.Baz))
        {
            return BadRequest();
        }

        accountStore.Upsert(request.Account, request.Bar, request.Baz);
        return NoContent();
    }

    [HttpGet("bar/{account}")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Bar(string account)
    {
        if (!accountStore.TryGet(account, out var endpoints))
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
        if (!accountStore.TryGet(account, out var endpoints))
        {
            return NotFound();
        }

        return Content(endpoints.Baz, "text/plain");
    }
}
