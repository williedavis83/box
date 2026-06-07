namespace BoxBottom.Aspire.ServiceDefaults;

public sealed class StackProperties(string stackName)
{
    public string StackName { get; } = stackName;

    public string ResolveDaprAppId(string logicalName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalName);

        return string.IsNullOrEmpty(StackName) ? logicalName : $"{StackName}-{logicalName}";
    }
}
