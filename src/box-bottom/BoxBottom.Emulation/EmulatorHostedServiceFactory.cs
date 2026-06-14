using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public sealed class EmulatorHostedServiceFactory<TServiceType, TServiceTypeConfig>
    : IEmulatorHostedServiceFactory<TServiceTypeConfig>
    where TServiceType : class, IHostedService
{
    private readonly Func<TServiceTypeConfig, IServiceProvider, TServiceType> _createHostedService;

    public EmulatorHostedServiceFactory(Func<TServiceTypeConfig, IServiceProvider, TServiceType> createHostedService)
    {
        ArgumentNullException.ThrowIfNull(createHostedService);
        _createHostedService = createHostedService;
    }

    public TServiceType CreateSingleton(TServiceTypeConfig config, IServiceProvider serviceProvider) =>
        _createHostedService(config, serviceProvider);

    IHostedService IEmulatorHostedServiceFactory<TServiceTypeConfig>.CreateSingleton(
        TServiceTypeConfig config,
        IServiceProvider serviceProvider) =>
        CreateSingleton(config, serviceProvider);
}
