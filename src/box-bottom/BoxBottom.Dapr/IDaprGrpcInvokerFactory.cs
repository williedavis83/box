using Grpc.Core;

namespace BoxBottom.Dapr;

public interface IDaprGrpcInvokerFactory
{
    CallInvoker CreateInvoker(string appId);
}
