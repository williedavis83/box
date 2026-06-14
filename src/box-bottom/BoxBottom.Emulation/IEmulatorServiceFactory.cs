using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Emulation;

public interface IEmulatorServiceFactory<TConfig>
{
    object CreateSingleton(TConfig config, IServiceProvider serviceProvider);

    object CreateList(IReadOnlyList<TConfig> configs, IServiceProvider serviceProvider);

    object CreateDictionary(
        IReadOnlyDictionary<string, TConfig> configs,
        IServiceProvider serviceProvider);
}
