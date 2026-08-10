using BoxBottom.Aspire.Orchestration;
using BoxPack.Aspire.Orchestration;

var builder = DistributedApplication.CreateBuilder(args);

var stackOperations = new StackOperations(
    builder,
    Constants.DefaultWebProjectPath,
    Constants.DefaultEdgeProjectPath,
    Constants.DefaultMetaProjectPath);

var stacks = Orchestrator.Orchestrate(stackOperations);

stackOperations.OrchestratePlaywrightTool(stacks, @"..\..\box-test\BoxTest.Playwright");

builder.Build().Run();

internal class Constants
{
    /// <summary>Default Vue shell used by <see cref="StackOperations.CreateStackDefinition"/>.</summary>
    public const string DefaultWebProjectPath = "../BoxTop.Web";

    public const string DefaultEdgeProjectPath = @"..\BoxTop.Edge\BoxTop.Edge.csproj";
    public const string DefaultMetaProjectPath = @"..\BoxTop.Meta.Api\BoxTop.Meta.Api.csproj";
}
