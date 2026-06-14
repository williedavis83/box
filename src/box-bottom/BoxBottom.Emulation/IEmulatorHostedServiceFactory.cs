using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoxBottom.Emulation;

public interface IEmulatorHostedServiceFactory<TConfig>
{
    IHostedService CreateSingleton(TConfig config, IServiceProvider serviceProvider);
}
