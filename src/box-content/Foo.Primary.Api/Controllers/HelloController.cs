using BoxBottom.Aspire.ServiceDefaults;
using BoxBottom.Dapr;
using Foo.Secondary.Api.Grpc;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace Foo.Primary.Api.Controllers;

[ApiController]
[Route("api")]
public class HelloController(
    IDaprGrpcInvokerFactory daprGrpcInvokerFactory,
    StackProperties stackProperties) : ControllerBase
{
    private const string _secondaryApiLogicalName = "secondary-api";

    [HttpGet("Hello")]
    public async Task<IActionResult> Hello(CancellationToken cancellationToken)
    {
        try
        {
            var secondaryApiAppId = stackProperties.ResolveDaprAppId(_secondaryApiLogicalName);
            var invoker = daprGrpcInvokerFactory.CreateInvoker(secondaryApiAppId);
            var client = new World.WorldClient(invoker);

            var reply = await client.GetWorldAsync(
                new GetWorldRequest(),
                cancellationToken: cancellationToken);

            return Ok($"Hello, {reply.Message}!");
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Cancelled)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
    }
}
