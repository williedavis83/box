using BoxBottom.Aspire.Orchestration;

namespace BoxPack.Aspire.Orchestration;

public static class Orchestrator
{
    private const string BoxStackName = "box";
    private const string BobStackName = "bob";
    private const string WorldMessageEnvironmentVariable = "World__Message";

    public static IReadOnlyDictionary<string, StackResources> Orchestrate(StackOperations stackOperations)
    {
        ArgumentNullException.ThrowIfNull(stackOperations);

        var boxStack = stackOperations.CreateStackDefinition(BoxStackName);

        boxStack.AddApi(new ApiProjectOptions(
            logicalName: "primary-api",
            projectPath: @"..\..\box-content\Foo.Primary.Api\Foo.Primary.Api.csproj",
            apiReferences: ["secondary-api"]));

        boxStack.AddApi(new ApiProjectOptions(
            logicalName: "secondary-api",
            projectPath: @"..\..\box-content\Foo.Secondary.Api\Foo.Secondary.Api.csproj",
            grpcOnlyAppChannel: true));

        var bobStack = boxStack with { Name = BobStackName };
        bobStack["secondary-api"].EnvironmentVariables[WorldMessageEnvironmentVariable] = "Bob";

        return new Dictionary<string, StackResources>(StringComparer.OrdinalIgnoreCase)
        {
            [BoxStackName] = stackOperations.OrchestrateStack(boxStack),
            [BobStackName] = stackOperations.OrchestrateStack(bobStack),
        };
    }
}
