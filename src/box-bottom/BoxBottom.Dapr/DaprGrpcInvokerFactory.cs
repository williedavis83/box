using Dapr.Client;
using Grpc.Core;
using Microsoft.Extensions.Configuration;

namespace BoxBottom.Dapr;

public sealed class DaprGrpcInvokerFactory(IConfiguration configuration) : IDaprGrpcInvokerFactory
{
    public CallInvoker CreateInvoker(string appId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appId);

        var daprGrpcPort = configuration["DAPR_GRPC_PORT"]
            ?? throw new InvalidOperationException("DAPR_GRPC_PORT is not configured.");

        return DaprClient.CreateInvocationInvoker(appId, $"http://127.0.0.1:{daprGrpcPort}");
    }
}
