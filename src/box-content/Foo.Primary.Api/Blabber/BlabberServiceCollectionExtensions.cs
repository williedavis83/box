using Blabber.Lib;
using Foo.Primary.Shared.Blabber;
using Microsoft.Extensions.Options;

namespace Foo.Primary.Api.Blabber;

public static class BlabberServiceCollectionExtensions
{
    public const string HttpClientName = "Bleeb";

    public static IServiceCollection AddBlabbers(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<BleebClientOptions>(configuration.GetSection(BleebClientOptions.SectionName));
        services.AddHttpClient(HttpClientName);

        services.AddKeyedSingleton<IBlabber>(BlabberKeys.Foo, CreateBlabberFactory(BlabberKeys.Foo));
        services.AddKeyedSingleton<IBlabber>(BlabberKeys.Fee, CreateBlabberFactory(BlabberKeys.Fee));

        AddAnchoredBlabberList(services, BlabberKeys.ListA, BlabberKeys.Foo, BlabberKeys.Fee);
        AddRealBleebBlabberList(services, BlabberKeys.ListB, BlabberKeys.Foo, BlabberKeys.Fee);

        AddAnchoredBlabberDictionary(services, BlabberKeys.DictA, BlabberKeys.Foo, BlabberKeys.Fee);
        AddRealBleebBlabberDictionary(services, BlabberKeys.DictB, BlabberKeys.Foo, BlabberKeys.Fee);

        return services;
    }

    /// <summary>
    /// Registers a keyed list composed from anchored singleton blabbers (emulation may replace members).
    /// </summary>
    private static void AddAnchoredBlabberList(
        IServiceCollection services,
        string listKey,
        params string[] accountKeys) =>
        services.AddKeyedSingleton<IReadOnlyList<NamedBlabber>>(
            listKey,
            (sp, _) => CreateNamedBlabberListFromAnchors(sp, accountKeys));

    /// <summary>
    /// Registers a keyed list that always uses real Bleeb clients, bypassing anchored singleton resolution.
    /// </summary>
    private static void AddRealBleebBlabberList(
        IServiceCollection services,
        string listKey,
        params string[] accounts) =>
        services.AddKeyedSingleton<IReadOnlyList<NamedBlabber>>(
            listKey,
            (sp, _) => CreateNamedBlabberListFromBleeb(sp, accounts));

    /// <summary>
    /// Registers a keyed dictionary composed from anchored singleton blabbers (emulation may replace members).
    /// </summary>
    private static void AddAnchoredBlabberDictionary(
        IServiceCollection services,
        string dictionaryKey,
        params string[] accountKeys) =>
        services.AddKeyedSingleton<IReadOnlyDictionary<string, IBlabber>>(
            dictionaryKey,
            (sp, _) => CreateBlabberDictionaryFromAnchors(sp, accountKeys));

    /// <summary>
    /// Registers a keyed dictionary that always uses real Bleeb clients, bypassing anchored singleton resolution.
    /// </summary>
    private static void AddRealBleebBlabberDictionary(
        IServiceCollection services,
        string dictionaryKey,
        params string[] accounts) =>
        services.AddKeyedSingleton<IReadOnlyDictionary<string, IBlabber>>(
            dictionaryKey,
            (sp, _) => CreateBlabberDictionaryFromBleeb(sp, accounts));

    private static Func<IServiceProvider, object?, IBlabber> CreateBlabberFactory(string account) =>
        (serviceProvider, _) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<BleebClientOptions>>().Value;
            if (options.BaseUri is null)
            {
                throw new InvalidOperationException(
                    $"Bleeb base URI is not configured. Set '{BleebClientOptions.SectionName}:BaseUri'.");
            }

            var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName);
            return new global::Blabber.Lib.Blabber(account, options.BaseUri, httpClient);
        };

    private static IReadOnlyList<NamedBlabber> CreateNamedBlabberListFromAnchors(
        IServiceProvider serviceProvider,
        params string[] accounts) =>
        accounts
            .Select(account => new NamedBlabber(
                account,
                serviceProvider.GetRequiredKeyedService<IBlabber>(account)))
            .ToList();

    private static IReadOnlyList<NamedBlabber> CreateNamedBlabberListFromBleeb(
        IServiceProvider serviceProvider,
        params string[] accounts) =>
        accounts
            .Select(account => new NamedBlabber(
                account,
                CreateBlabberFactory(account)(serviceProvider, null)))
            .ToList();

    private static IReadOnlyDictionary<string, IBlabber> CreateBlabberDictionaryFromAnchors(
        IServiceProvider serviceProvider,
        params string[] accounts) =>
        accounts.ToDictionary(
            account => account,
            account => serviceProvider.GetRequiredKeyedService<IBlabber>(account),
            StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyDictionary<string, IBlabber> CreateBlabberDictionaryFromBleeb(
        IServiceProvider serviceProvider,
        params string[] accounts) =>
        accounts.ToDictionary(
            account => account,
            account => CreateBlabberFactory(account)(serviceProvider, null),
            StringComparer.OrdinalIgnoreCase);
}
