using System.Diagnostics.CodeAnalysis;

namespace BoxBottom.Aspire.Orchestration;

public record StackDefinition
{
    private readonly Dictionary<string, ApiProjectOptions> _apis;
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            if (string.Equals(_name, value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _name = value;
            ApplyStackPrefix(value);
        }
    }

    private WebProjectOptions? _web;
    public required WebProjectOptions Web 
    {
        get => _web!; 
        set => _web = value with { StackPrefix = Name, EnvironmentVariables = value.EnvironmentVariables }; 
    }

    private EdgeProjectOptions? _edge;
    public required EdgeProjectOptions Edge
    {
        get => _edge!;
        set => _edge = value with { StackPrefix = Name, EnvironmentVariables = value.EnvironmentVariables };
    }

    private MetaProjectOptions? _meta;
    public required MetaProjectOptions Meta
    {
        get => _meta!;
        set => _meta = value with { StackPrefix = Name, EnvironmentVariables = value.EnvironmentVariables };
    }

    public ApiProjectOptions this[string logicalName] => _apis[logicalName];

    /// <summary>
    /// Marks this stack as an integration stack: its resources start only on demand and are
    /// grouped under the Playwright tool in the Aspire dashboard.
    /// </summary>
    public bool IsIntegrationStack { get; private set; }

    /// <summary>
    /// Marks this stack as an integration stack. Call before <see cref="StackOperations.OrchestrateStack"/>.
    /// </summary>
    public StackDefinition AsIntegrationStack()
    {
        IsIntegrationStack = true;
        return this;
    }

    public void AddApi(ApiProjectOptions api)
    {
        ArgumentNullException.ThrowIfNull(api);
        ArgumentException.ThrowIfNullOrWhiteSpace(api.LogicalName);
        _apis.Add(api.LogicalName, api with { StackPrefix = _name, EnvironmentVariables = api.EnvironmentVariables });
    }

    /// <summary>
    /// Enables Dapr sidecars based on AppHost config and <see cref="ApiProjectOptions.ApiReferences"/>.
    /// When <paramref name="defaultStart"/> is true, every API gets a sidecar. Otherwise only
    /// APIs that reference another API (and their callees) get sidecars.
    /// </summary>
    public void ApplyDaprSidecarFromApiReferences(bool defaultStart = false)
    {
        if (defaultStart)
        {
            foreach (var api in _apis.Values)
            {
                api.EnableDaprSidecar = true;
            }

            return;
        }

        foreach (var api in _apis.Values)
        {
            foreach (var referencedLogicalName in api.ApiReferences)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(referencedLogicalName);

                if (!_apis.TryGetValue(referencedLogicalName, out var referencedApi))
                {
                    throw new InvalidOperationException(
                        $"API '{api.LogicalName}' references unknown API '{referencedLogicalName}' in stack '{Name}'.");
                }

                api.EnableDaprSidecar = true;
                referencedApi.EnableDaprSidecar = true;
            }
        }
    }

    public IEnumerable<ApiProjectOptions> GetApis() => _apis.Values;

    [SetsRequiredMembers]
    public StackDefinition(string name, WebProjectOptions web, EdgeProjectOptions edge, MetaProjectOptions meta)
    {
        _name = name;
        Web = web;
        Edge = edge;
        Meta = meta;
        _apis = new Dictionary<string, ApiProjectOptions>(StringComparer.OrdinalIgnoreCase);

    }

    /// <summary>
    /// Copies <paramref name="original"/> into a new <see cref="StackDefinition"/> with a distinct API dictionary.
    /// Environment variables are not copied. Record <c>with</c> expressions use this constructor,
    /// e.g. <c>stack with { Name = "name2" }</c>.
    /// </summary>
    [SetsRequiredMembers]
    protected StackDefinition(StackDefinition original)
    {
        ArgumentNullException.ThrowIfNull(original);

        _apis = new Dictionary<string, ApiProjectOptions>(StringComparer.OrdinalIgnoreCase);
        _name = original._name;
        IsIntegrationStack = original.IsIntegrationStack;
        Web = new WebProjectOptions(original.Web);
        Edge = new EdgeProjectOptions(original.Edge);
        Meta = new MetaProjectOptions(original.Meta);

        foreach (var (logicalName, api) in original._apis)
        {
            _apis.Add(logicalName, new ApiProjectOptions(api));
        }
    }

    private void ApplyStackPrefix(string stackPrefix)
    {
        Web = Web with { StackPrefix = stackPrefix };
        Edge = Edge with { StackPrefix = stackPrefix };
        Meta = Meta with { StackPrefix = stackPrefix };

        foreach (var logicalName in _apis.Keys.ToList())
        {
            _apis[logicalName] = _apis[logicalName] with { StackPrefix = stackPrefix };
        }
    }
}
