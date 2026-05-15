using Tiferet.Domain;
using Tiferet.Domain.App;

namespace Tiferet.Mappers.App;

/// <summary>Aggregate for mutable app interface operations.</summary>
public record AppInterfaceAggregate : Aggregate<AppInterfaceConfiguration>
{
    public AppInterfaceAggregate(AppInterfaceConfiguration state) : base(state) { }

    // Delegated properties.
    public string Id => State.Id;
    public string Name => State.Name;
    public string AssemblyName => State.AssemblyName;
    public string TypeName => State.TypeName;
    public string? Description => State.Description;
    public string LoggerId => State.LoggerId;
    public IReadOnlyList<string>? Flags => State.Flags;
    public IReadOnlyList<AppServiceDependencyConfiguration>? Services => State.Services;
    public IReadOnlyDictionary<string, string>? Constants => State.Constants;

    public void Rename(string name) => Mutate(s => s with { Name = name });
    public void SetDescription(string? description) => Mutate(s => s with { Description = description });
    public void SetAssemblyName(string assemblyName) => Mutate(s => s with { AssemblyName = assemblyName });
    public void SetTypeName(string typeName) => Mutate(s => s with { TypeName = typeName });
    public void SetLoggerId(string loggerId) => Mutate(s => s with { LoggerId = loggerId });
    public void SetFlags(IReadOnlyList<string> flags) => Mutate(s => s with { Flags = flags });

    public void AddService(string serviceId, string assemblyName, string typeName,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        var dep = new AppServiceDependencyConfiguration(serviceId, assemblyName, typeName, parameters);
        var services = new List<AppServiceDependencyConfiguration>(Services ?? []) { dep };
        Mutate(s => s with { Services = services });
    }

    public AppServiceDependencyConfiguration? RemoveService(string serviceId)
    {
        var services = new List<AppServiceDependencyConfiguration>(Services ?? []);
        var dep = services.FirstOrDefault(d => d.ServiceId == serviceId);
        if (dep is null) return null;

        services.Remove(dep);
        Mutate(s => s with { Services = services });
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
            Mutate(s => s with { Constants = null });
            return;
        }

        var merged = new Dictionary<string, string>(Constants ?? new Dictionary<string, string>());
        foreach (var (key, value) in constants)
        {
            if (value is null) merged.Remove(key);
            else merged[key] = value;
        }

        Mutate(s => s with { Constants = merged });
    }

    /// <summary>
    /// Get the service dependency by service ID.
    /// Delegates to the domain record.
    /// </summary>
    public AppServiceDependencyConfiguration? GetService(string serviceId)
        => State.GetService(serviceId);
}
