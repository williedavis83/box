using System.Diagnostics.CodeAnalysis;

namespace BoxBottom.Aspire.Orchestration;

/// <summary>
/// Arguments for <see cref="DistributedApplicationBuilderExtensions.AddApiProject"/>.
/// </summary>
public record ApiProjectOptions : IStackProjectOptions
{
    public required string LogicalName { get; init; }

    public required string ProjectPath { get; init; }

    public string StackPrefix { get; init; } = string.Empty;

    public string StackName =>
        string.IsNullOrEmpty(StackPrefix) ? LogicalName : $"{StackPrefix}-{LogicalName}";

    public string AspNetEnvironment { get; init; } = "Development";

    public bool GrpcOnlyAppChannel { get; init; }

    /// <summary>
    /// Dapr app IDs of APIs this project is allowed to invoke.
    /// </summary>
    public List<string> ApiReferences { get; init; } = [];

    public Dictionary<string, string> EnvironmentVariables { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    [SetsRequiredMembers]
    public ApiProjectOptions(
        string logicalName,
        string projectPath,
        string stackPrefix = "",
        string aspNetEnvironment = "Development",
        bool grpcOnlyAppChannel = false,
        List<string>? apiReferences = null,
        Dictionary<string, string>? environmentVariables = null)
    {
        LogicalName = logicalName;
        ProjectPath = projectPath;
        StackPrefix = stackPrefix;
        AspNetEnvironment = aspNetEnvironment;
        GrpcOnlyAppChannel = grpcOnlyAppChannel;
        ApiReferences = apiReferences is null ? [] : [..apiReferences];
        EnvironmentVariables = environmentVariables is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(environmentVariables, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Copies scalar options and <see cref="ApiReferences"/>, and starts with a new empty
    /// <see cref="EnvironmentVariables"/> dictionary.
    /// </summary>
    [SetsRequiredMembers]
    public ApiProjectOptions(ApiProjectOptions other)
    {
        ArgumentNullException.ThrowIfNull(other);

        LogicalName = other.LogicalName;
        ProjectPath = other.ProjectPath;
        StackPrefix = other.StackPrefix;
        AspNetEnvironment = other.AspNetEnvironment;
        GrpcOnlyAppChannel = other.GrpcOnlyAppChannel;
        ApiReferences = [..other.ApiReferences];
        EnvironmentVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
