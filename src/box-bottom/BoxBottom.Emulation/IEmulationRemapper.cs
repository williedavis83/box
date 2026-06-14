using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace BoxBottom.Emulation;

public interface IEmulationRemapper
{
    void RemapInjection(IServiceCollection services, JsonDocument jsonDocument);
}
