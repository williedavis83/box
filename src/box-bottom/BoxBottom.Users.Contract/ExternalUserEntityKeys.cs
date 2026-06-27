namespace BoxBottom.Users.Contract;

public static class ExternalUserEntityKeys
{
    public const string TableName = "externalUsers";

    public static string BuildPartitionKey(string provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);

        return provider.Trim().ToLowerInvariant();
    }

    public static string BuildRowKey(string externalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);

        return externalId.Trim();
    }
}
