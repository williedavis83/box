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

        services.AddKeyedSingleton<IReadOnlyList<NamedBlabber>>(
            BlabberKeys.ListA,
            (sp, _) => CreateNamedBlabberList(sp, BlabberKeys.Foo, BlabberKeys.Fee));

        services.AddKeyedSingleton<IReadOnlyList<NamedBlabber>>(
            BlabberKeys.ListB,
            (sp, _) => CreateNamedBlabberListFromBleeb(sp, BlabberKeys.Foo, BlabberKeys.Fee));

        services.AddKeyedSingleton<IReadOnlyDictionary<string, IBlabber>>(
            BlabberKeys.DictA,
            (sp, _) => CreateBlabberDictionary(sp, BlabberKeys.Foo, BlabberKeys.Fee));

        services.AddKeyedSingleton<IReadOnlyDictionary<string, IBlabber>>(
            BlabberKeys.DictB,
            (sp, _) => CreateBlabberDictionaryFromBleeb(sp, BlabberKeys.Foo, BlabberKeys.Fee));

        return services;
    }

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

    private static IReadOnlyList<NamedBlabber> CreateNamedBlabberList(
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

    private static IReadOnlyDictionary<string, IBlabber> CreateBlabberDictionary(
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
