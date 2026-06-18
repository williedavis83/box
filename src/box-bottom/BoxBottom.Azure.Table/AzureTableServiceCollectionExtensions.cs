using BoxBottom.Azure.Table.Internal;
using BoxBottom.Azure.Table.Options;
using BoxBottom.Azure.Table.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BoxBottom.Azure.Table;

public static class AzureTableServiceCollectionExtensions
{
    public static IServiceCollection AddAzureTableStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AzureTableUriOptions>(
            configuration.GetSection(AzureTableUriOptions.SectionName));
        services.Configure<AzureTableConnectionStringOptions>(
            configuration.GetSection(AzureTableConnectionStringOptions.SectionName));
        services.Configure<AzureTableAccountsOptions>(
            configuration.GetSection(AzureTableAccountsOptions.SectionName));
        services.Configure<AzureTableBootstrapOptions>(
            configuration.GetSection(AzureTableBootstrapOptions.SectionName));

        services.AddHostedService<AzureTableBootstrapHostedService>();

        return services;
    }

    public static IServiceCollection AddAzureTableAccount(
        this IServiceCollection services,
        string anchorKey)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorKey);

        services.AddKeyedSingleton<IAzureTableService>(
            anchorKey,
            (serviceProvider, _) =>
            {
                var factory = new AccountAzureTableServiceClientFactory(
                    anchorKey,
                    serviceProvider.GetRequiredService<IOptions<AzureTableConnectionStringOptions>>(),
                    serviceProvider.GetRequiredService<IOptions<AzureTableAccountsOptions>>());

                return new AzureTableService(factory);
            });

        return services;
    }

    public static IServiceCollection AddAzureTableDictionary(
        this IServiceCollection services,
        string dictionaryAnchorKey,
        params (string DictionaryKey, string MemberAnchorKey)[] members)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(dictionaryAnchorKey);
        ArgumentNullException.ThrowIfNull(members);

        if (members.Length == 0)
        {
            throw new ArgumentException("At least one dictionary member is required.", nameof(members));
        }

        services.AddKeyedSingleton<IReadOnlyDictionary<string, IAzureTableService>>(
            dictionaryAnchorKey,
            (serviceProvider, _) => members.ToDictionary(
                member => member.DictionaryKey,
                member => serviceProvider.GetRequiredKeyedService<IAzureTableService>(member.MemberAnchorKey),
                StringComparer.OrdinalIgnoreCase));

        return services;
    }
}
