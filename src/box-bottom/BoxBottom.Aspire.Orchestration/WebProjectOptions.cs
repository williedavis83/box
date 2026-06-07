using System.Diagnostics.CodeAnalysis;

namespace BoxBottom.Aspire.Orchestration;

/// <summary>
/// Arguments for <see cref="DistributedApplicationBuilderExtensions.AddWebProject"/>.
/// </summary>
public record WebProjectOptions : IStackProjectOptions
{
    public required string LogicalName { get; init; }

    public required string ProjectPath { get; init; }

    public string StackPrefix { get; init; } = string.Empty;

    public string StackName =>
        string.IsNullOrEmpty(StackPrefix) ? LogicalName : $"{StackPrefix}-{LogicalName}";

    public Dictionary<string, string> EnvironmentVariables { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    [SetsRequiredMembers]
    public WebProjectOptions(
        string logicalName,
        string projectPath,
        string stackPrefix = "",
        Dictionary<string, string>? environmentVariables = null)
    {
        LogicalName = logicalName;
        ProjectPath = projectPath;
        StackPrefix = stackPrefix;
        EnvironmentVariables = environmentVariables is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(environmentVariables, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Copies scalar options and starts with a new empty <see cref="EnvironmentVariables"/> dictionary.
    /// </summary>
    [SetsRequiredMembers]
    public WebProjectOptions(WebProjectOptions other)
    {
        ArgumentNullException.ThrowIfNull(other);

        LogicalName = other.LogicalName;
        ProjectPath = other.ProjectPath;
        StackPrefix = other.StackPrefix;
        EnvironmentVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
