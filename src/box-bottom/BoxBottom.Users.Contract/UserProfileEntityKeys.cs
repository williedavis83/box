namespace BoxBottom.Users.Contract;

public static class UserProfileEntityKeys
{
    public const string TableName = "userProfiles";

    public const string ProfileRowKey = "profile";

    /// <summary>
    /// Builds a partition key that spreads writes across partitions even when user IDs are sequential.
    /// The reversed GUID string keeps each user in a dedicated partition without a shared hot key.
    /// </summary>
    public static string BuildPartitionKey(Guid userId)
    {
        var guidN = userId.ToString("N");
        return string.Create(guidN.Length, guidN, static (span, source) =>
        {
            for (var i = 0; i < source.Length; i++)
            {
                span[i] = source[source.Length - 1 - i];
            }
        });
    }

    public static string BuildRowKey() => ProfileRowKey;
}
