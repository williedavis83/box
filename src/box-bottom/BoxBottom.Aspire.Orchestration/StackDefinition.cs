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

    public ApiProjectOptions this[string logicalName] => _apis[logicalName];

    public void AddApi(ApiProjectOptions api)
    {
        ArgumentNullException.ThrowIfNull(api);
        ArgumentException.ThrowIfNullOrWhiteSpace(api.LogicalName);
        _apis.Add(api.LogicalName, api with { StackPrefix = _name, EnvironmentVariables = api.EnvironmentVariables });
    }

    public IEnumerable<ApiProjectOptions> GetApis() => _apis.Values;

    [SetsRequiredMembers]
    public StackDefinition(string name, WebProjectOptions web, EdgeProjectOptions edge)
    {
        _name = name;
        Web = web;
        Edge = edge;
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
        Web = new WebProjectOptions(original.Web);
        Edge = new EdgeProjectOptions(original.Edge);

        foreach (var (logicalName, api) in original._apis)
        {
            _apis.Add(logicalName, new ApiProjectOptions(api));
        }
    }

    private void ApplyStackPrefix(string stackPrefix)
    {
        Web = Web with { StackPrefix = stackPrefix };
        Edge = Edge with { StackPrefix = stackPrefix };

        foreach (var logicalName in _apis.Keys.ToList())
        {
            _apis[logicalName] = _apis[logicalName] with { StackPrefix = stackPrefix };
        }
    }
}
