using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>
/// Aggregate for mutable app interface operations.
/// </summary>
public class AppInterfaceAggregate : Aggregate<AppInterface>
{
    public AppInterfaceAggregate(AppInterface domain) : base(domain) { }

    /// <summary>Add a service dependency.</summary>
    public void AddService(string serviceId, string assemblyName, string typeName,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        var dep = new AppServiceDependency(serviceId, assemblyName, typeName, parameters);
        var services = new List<AppServiceDependency>(Domain.Services ?? []) { dep };
        SetAttribute(nameof(AppInterface.Services), (IReadOnlyList<AppServiceDependency>)services);
    }

    /// <summary>Remove and return a service dependency by ID (idempotent).</summary>
    public AppServiceDependency? RemoveService(string serviceId)
    {
        var services = new List<AppServiceDependency>(Domain.Services ?? []);
        var dep = services.FirstOrDefault(d => d.ServiceId == serviceId);
        if (dep is null) return null;

        services.Remove(dep);
        SetAttribute(nameof(AppInterface.Services), (IReadOnlyList<AppServiceDependency>)services);
        return dep;
    }

    /// <summary>
    /// Set or update a service dependency (PUT semantics).
    /// </summary>
    public void SetService(string serviceId, string assemblyName, string typeName,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        // Remove existing, then add.
        RemoveService(serviceId);
        AddService(serviceId, assemblyName, typeName, parameters);
    }

    /// <summary>
    /// Update the constants dictionary.
    /// Null clears all. Keys with null values are removed.
    /// </summary>
    public void SetConstants(Dictionary<string, string?>? constants)
    {
        if (constants is null)
        {
            SetAttribute(nameof(AppInterface.Constants), (IReadOnlyDictionary<string, string>?)null);
            return;
        }

        var merged = new Dictionary<string, string>(Domain.Constants ?? new Dictionary<string, string>());
        foreach (var (key, value) in constants)
        {
            if (value is null)
                merged.Remove(key);
            else
                merged[key] = value;
        }

        SetAttribute(nameof(AppInterface.Constants), (IReadOnlyDictionary<string, string>)merged);
    }
}
