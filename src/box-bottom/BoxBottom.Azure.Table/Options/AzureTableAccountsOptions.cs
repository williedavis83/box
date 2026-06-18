namespace BoxBottom.Azure.Table.Options;

public sealed class AzureTableAccountsOptions
    : Dictionary<string, AzureTableAccountOptions>
{
    public const string SectionName = "AzureTableAccounts";
}
