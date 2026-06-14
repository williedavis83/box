using System.Reflection;

namespace BoxBottom.Emulation;

public static class EmulationAnchorScanner
{
    public static IReadOnlyList<EmulationAnchorInfo> Scan(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        var results = new List<EmulationAnchorInfo>();

        foreach (var assembly in assemblies.Where(static assembly => assembly is not null).Distinct())
        {
            var assemblyName = assembly.GetName().Name ?? assembly.FullName ?? "unknown";

            foreach (var attribute in assembly.GetCustomAttributes<EmulationAnchorAttribute>())
            {
                results.Add(new EmulationAnchorInfo(
                    assemblyName,
                    attribute.AnchorName,
                    attribute.ServiceType,
                    attribute.Kind,
                    attribute.Lifetime));
            }
        }

        return results;
    }

    public static IEnumerable<Assembly> CollectAssemblies(params Assembly[] assemblies)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var results = new List<Assembly>();

        void TryAdd(Assembly? assembly)
        {
            if (assembly is null)
            {
                return;
            }

            var key = assembly.FullName ?? assembly.GetName().Name ?? assembly.GetHashCode().ToString();
            if (seen.Add(key))
            {
                results.Add(assembly);
            }
        }

        TryAdd(Assembly.GetEntryAssembly());

        foreach (var assembly in assemblies)
        {
            TryAdd(assembly);
        }

        return results;
    }
}
