using Tiferet.Domain.App;

namespace Tiferet.Mappers.App;

/// <summary>Aggregate for mutable app interface operations.</summary>
public class AppInterfaceAggregate : Aggregate<AppInterfaceConfiguration>
{
    public AppInterfaceAggregate(AppInterfaceConfiguration domain) : base(domain) { }

    public void Rename(string name) => SetAttribute(nameof(AppInterfaceConfiguration.Name), name);
    public void SetDescription(string? description) => SetAttribute(nameof(AppInterfaceConfiguration.Description), description);
    public void SetAssemblyName(string assemblyName) => SetAttribute(nameof(AppInterfaceConfiguration.AssemblyName), assemblyName);
    public void SetTypeName(string typeName) => SetAttribute(nameof(AppInterfaceConfiguration.TypeName), typeName);
    public void SetLoggerId(string loggerId) => SetAttribute(nameof(AppInterfaceConfiguration.LoggerId), loggerId);
    public void SetFlags(IReadOnlyList<string> flags) => SetAttribute(nameof(AppInterfaceConfiguration.Flags), flags);

    public void AddService(string serviceId, string assemblyName, string typeName,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        var dep = new AppServiceDependencyConfiguration(serviceId, assemblyName, typeName, parameters);
        var services = new List<AppServiceDependencyConfiguration>(Domain.Services ?? []) { dep };
        SetAttribute(nameof(AppInterfaceConfiguration.Services), (IReadOnlyList<AppServiceDependencyConfiguration>)services);
    }

    public AppServiceDependencyConfiguration? RemoveService(string serviceId)
    {
        var services = new List<AppServiceDependencyConfiguration>(Domain.Services ?? []);
        var dep = services.FirstOrDefault(d => d.ServiceId == serviceId);
        if (dep is null) return null;

        services.Remove(dep);
        SetAttribute(nameof(AppInterfaceConfiguration.Services), (IReadOnlyList<AppServiceDependencyConfiguration>)services);
        return dep;
    }

    public void SetService(string serviceId, string assemblyName, string typeName,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        RemoveService(serviceId);
        AddService(serviceId, assemblyName, typeName, parameters);
    }

    public void SetConstants(Dictionary<string, string?>? constants)
    {
        if (constants is null)
        {
            SetAttribute(nameof(AppInterfaceConfiguration.Constants), (IReadOnlyDictionary<string, string>?)null);
            return;
        }

        var merged = new Dictionary<string, string>(Domain.Constants ?? new Dictionary<string, string>());
        foreach (var (key, value) in constants)
        {
            if (value is null) merged.Remove(key);
            else merged[key] = value;
        }

        SetAttribute(nameof(AppInterfaceConfiguration.Constants), (IReadOnlyDictionary<string, string>)merged);
    }
}
