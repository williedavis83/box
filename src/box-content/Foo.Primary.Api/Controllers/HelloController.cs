using BoxBottom.Dapr;
using Foo.Secondary.Api.Grpc;
using Microsoft.AspNetCore.Mvc;

namespace Foo.Primary.Api.Controllers;

[ApiController]
[Route("api")]
public class HelloController(IDaprGrpcInvokerFactory daprGrpcInvokerFactory) : ControllerBase
{
    private const string SecondaryApiAppId = "secondary-api";

    [HttpGet("Hello")]
    public async Task<IActionResult> Hello(CancellationToken cancellationToken)
    {
        var invoker = daprGrpcInvokerFactory.CreateInvoker(SecondaryApiAppId);
        var client = new World.WorldClient(invoker);

        var reply = await client.GetWorldAsync(
            new GetWorldRequest(),
            cancellationToken: cancellationToken);

        return Ok($"Hello, {reply.Message}!");
    }
}
