using Blabber.Lib;
using BoxBottom.Emulation;
using Foo.Primary.Shared.Blabber;
using Foo.Primary.Api.Models;
using Microsoft.AspNetCore.Mvc;

[assembly: EmulationAnchor(BlabberKeys.Foo, typeof(IBlabber), EmulationAnchorKind.Singleton)]
[assembly: EmulationAnchor(BlabberKeys.ListA, typeof(IReadOnlyList<NamedBlabber>), EmulationAnchorKind.List)]
[assembly: EmulationAnchor(BlabberKeys.DictA, typeof(IReadOnlyDictionary<string, IBlabber>), EmulationAnchorKind.Dictionary)]

namespace Foo.Primary.Api.Controllers;

[ApiController]
[Route("api")]
public class BlabberController : ControllerBase
{
    [HttpGet("Blabber/Foo")]
    [ProducesResponseType<BlabberResult>(StatusCodes.Status200OK)]
    public Task<BlabberResult> Foo(
        [FromKeyedServices(BlabberKeys.Foo)] IBlabber blabber,
        CancellationToken cancellationToken) =>
        BlabberResult.FromBlabberAsync(BlabberKeys.Foo, blabber, cancellationToken);

    [HttpGet("Blabber/Fee")]
    [ProducesResponseType<BlabberResult>(StatusCodes.Status200OK)]
    public Task<BlabberResult> Fee(
        [FromKeyedServices(BlabberKeys.Fee)] IBlabber blabber,
        CancellationToken cancellationToken) =>
        BlabberResult.FromBlabberAsync(BlabberKeys.Fee, blabber, cancellationToken);

    [HttpGet("Blabber/ListA")]
    [ProducesResponseType<List<BlabberResult>>(StatusCodes.Status200OK)]
    public Task<List<BlabberResult>> ListA(
        [FromKeyedServices(BlabberKeys.ListA)] IReadOnlyList<NamedBlabber> blabbers,
        CancellationToken cancellationToken) =>
        ToResultListAsync(blabbers, cancellationToken);

    [HttpGet("Blabber/ListB")]
    [ProducesResponseType<List<BlabberResult>>(StatusCodes.Status200OK)]
    public Task<List<BlabberResult>> ListB(
        [FromKeyedServices(BlabberKeys.ListB)] IReadOnlyList<NamedBlabber> blabbers,
        CancellationToken cancellationToken) =>
        ToResultListAsync(blabbers, cancellationToken);

    [HttpGet("Blabber/DictA")]
    [ProducesResponseType<BlabberDictAResult>(StatusCodes.Status200OK)]
    public async Task<BlabberDictAResult> DictA(
        [FromKeyedServices(BlabberKeys.DictA)] IReadOnlyDictionary<string, IBlabber> blabbers,
        CancellationToken cancellationToken)
    {
        var results = await ToResultDictionaryAsync(blabbers, cancellationToken);
        return new BlabberDictAResult { Accounts = results };
    }

    [HttpGet("Blabber/DictB")]
    [ProducesResponseType<BlabberDictBResult>(StatusCodes.Status200OK)]
    public async Task<BlabberDictBResult> DictB(
        [FromKeyedServices(BlabberKeys.DictB)] IReadOnlyDictionary<string, IBlabber> blabbers,
        CancellationToken cancellationToken)
    {
        var results = await ToResultDictionaryAsync(blabbers, cancellationToken);
        return new BlabberDictBResult { Accounts = results };
    }

    private static async Task<List<BlabberResult>> ToResultListAsync(
        IEnumerable<NamedBlabber> blabbers,
        CancellationToken cancellationToken)
    {
        var results = new List<BlabberResult>();

        foreach (var entry in blabbers)
        {
            results.Add(await BlabberResult.FromBlabberAsync(
                entry.Account,
                entry.Blabber,
                cancellationToken));
        }

        return results;
    }

    private static async Task<Dictionary<string, BlabberResult>> ToResultDictionaryAsync(
        IReadOnlyDictionary<string, IBlabber> blabbers,
        CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, BlabberResult>(StringComparer.OrdinalIgnoreCase);

        foreach (var (account, blabber) in blabbers)
        {
            results[account] = await BlabberResult.FromBlabberAsync(account, blabber, cancellationToken);
        }

        return results;
    }
}
