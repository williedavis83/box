using BoxBottom.Aspire.Orchestration;
using BoxPack.Aspire.Orchestration;

var builder = DistributedApplication.CreateBuilder(args);

var stackOperations = new StackOperations(
    builder,
    Constants.DefaultWebProjectPath,
    Constants.DefaultEdgeProjectPath);

var stacks = Orchestrator.Orchestrate(stackOperations);

stackOperations.OrchestratePlaywrightTool(stacks, @"..\..\box-test\BoxTest.Playwright");

builder.Build().Run();

internal class Constants
{
    public const string DefaultWebProjectPath = "../BoxTop.Web";
    public const string DefaultEdgeProjectPath = @"..\BoxTop.Edge\BoxTop.Edge.csproj";
}
