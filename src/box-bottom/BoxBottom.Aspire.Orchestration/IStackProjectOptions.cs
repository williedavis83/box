namespace BoxBottom.Aspire.Orchestration;

public interface IStackProjectOptions
{
    string LogicalName { get; }

    string ProjectPath { get; }

    string StackPrefix { get; }

    string StackName { get; }

    Dictionary<string, string> EnvironmentVariables { get; }
}
