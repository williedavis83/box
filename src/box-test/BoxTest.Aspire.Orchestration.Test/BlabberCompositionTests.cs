using Blabber.Emulator;
using Blabber.Lib;
using BoxBottom.Emulation;
using Foo.Primary.Api.Blabber;
using Foo.Primary.Shared.Blabber;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoxTest.Aspire.Orchestration.Test;

public class BlabberCompositionTests
{
    private static readonly Uri BleebBaseUri = new("http://bleeb.test/");

    [Fact]
    public void ListB_AndDictB_UseRealBleebBlabbers_WhenFooAnchorIsEmulated()
    {
        var services = CreateServices();
        EmulationKeyedServiceReplacement.ReplaceKeyed(
            services,
            typeof(IBlabber),
            BlabberKeys.Foo,
            EmulationAnchorLifetime.Singleton,
            (_, _) => new FakeBlabber(new Blabber.Lib.Blabber("foo", BleebBaseUri, new HttpClient())));

        using var provider = services.BuildServiceProvider();

        var listB = provider.GetRequiredKeyedService<IReadOnlyList<NamedBlabber>>(BlabberKeys.ListB);
        var dictB = provider.GetRequiredKeyedService<IReadOnlyDictionary<string, IBlabber>>(BlabberKeys.DictB);

        Assert.IsNotType<FakeBlabber>(listB.Single(item => item.Account == BlabberKeys.Foo).Blabber);
        Assert.IsNotType<FakeBlabber>(dictB[BlabberKeys.Foo]);
        Assert.IsType<Blabber.Lib.Blabber>(listB.Single(item => item.Account == BlabberKeys.Foo).Blabber);
        Assert.IsType<Blabber.Lib.Blabber>(dictB[BlabberKeys.Foo]);
    }

    [Fact]
    public void ListA_AndDictA_UseKeyedBlabbers_WhenNotReplacedByEmulation()
    {
        var services = CreateServices();
        EmulationKeyedServiceReplacement.ReplaceKeyed(
            services,
            typeof(IBlabber),
            BlabberKeys.Foo,
            EmulationAnchorLifetime.Singleton,
            (_, _) => new FakeBlabber(new Blabber.Lib.Blabber("foo", BleebBaseUri, new HttpClient())));

        using var provider = services.BuildServiceProvider();

        var listA = provider.GetRequiredKeyedService<IReadOnlyList<NamedBlabber>>(BlabberKeys.ListA);
        var dictA = provider.GetRequiredKeyedService<IReadOnlyDictionary<string, IBlabber>>(BlabberKeys.DictA);

        Assert.IsType<FakeBlabber>(listA.Single(item => item.Account == BlabberKeys.Foo).Blabber);
        Assert.IsType<FakeBlabber>(dictA[BlabberKeys.Foo]);
    }

    private static ServiceCollection CreateServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Bleeb:BaseUri"] = BleebBaseUri.ToString(),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddBlabbers(configuration);
        return services;
    }
}
