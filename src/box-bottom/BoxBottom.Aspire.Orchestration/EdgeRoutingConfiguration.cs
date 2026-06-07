namespace BoxBottom.Aspire.Orchestration;

public static class EdgeRoutingConfiguration
{
    public static string BuildApiClusterAddress(string aspireResourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(aspireResourceName);

        return $"http://{aspireResourceName}";
    }

    public static string BuildDefaultPrimaryRoutePath() => "/api/{**catch-all}";

    public static bool DefaultPrimaryRouteMatchesPath(string requestPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestPath);

        return requestPath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
    }

    public static string BuildRouteKey(string logicalName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalName);

        return logicalName.Trim().ToLowerInvariant().Replace('_', '-');
    }

    public static bool IsDefaultApiRoute(string logicalName) =>
        string.Equals(logicalName, "primary-api", StringComparison.OrdinalIgnoreCase);

    public static string BuildRoutePath(string logicalName, int apiCount)
    {
        if (apiCount == 1 || IsDefaultApiRoute(logicalName))
        {
            return BuildDefaultPrimaryRoutePath();
        }

        var normalized = BuildRouteKey(logicalName);
        if (normalized.EndsWith("-api", StringComparison.Ordinal))
        {
            normalized = normalized[..^4];
        }

        return $"/api/{normalized}/{{**catch-all}}";
    }

    public static string BuildRoutePathPrefix(string logicalName)
    {
        var normalized = BuildRouteKey(logicalName);
        if (normalized.EndsWith("-api", StringComparison.Ordinal))
        {
            normalized = normalized[..^4];
        }

        return $"/api/{normalized}";
    }
}
