using Foo.Secondary.Api.Grpc;
using Foo.Secondary.Api.Options;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace Foo.Secondary.Api.Services;

public sealed class WorldGrpcService(IOptions<WorldOptions> options) : World.WorldBase
{
    public override Task<GetWorldReply> GetWorld(GetWorldRequest request, ServerCallContext context) =>
        Task.FromResult(new GetWorldReply { Message = options.Value.Message });
}
