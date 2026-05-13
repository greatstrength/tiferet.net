using Tiferet.Core;
using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>
/// Aggregate for mutable flagged dependency operations.
/// </summary>
public class FlaggedDependencyAggregate : Aggregate<FlaggedDependency>
{
    public FlaggedDependencyAggregate(FlaggedDependency domain) : base(domain) { }

    /// <summary>
    /// Update the parameters dictionary. Null clears all.
    /// </summary>
    public void SetParameters(Dictionary<string, string?>? parameters)
    {
        if (parameters is null)
        {
            SetAttribute(nameof(FlaggedDependency.Parameters),
                (IReadOnlyDictionary<string, string>?)new Dictionary<string, string>());
            return;
        }

        var merged = new Dictionary<string, string>(
            Domain.Parameters ?? new Dictionary<string, string>());
        foreach (var (key, value) in parameters)
        {
            if (value is null)
                merged.Remove(key);
            else
                merged[key] = value;
        }

        SetAttribute(nameof(FlaggedDependency.Parameters),
            (IReadOnlyDictionary<string, string>)merged);
    }
}

/// <summary>
/// Aggregate for mutable service configuration operations.
/// Includes <see cref="GetServiceType"/> for assembly-based type resolution.
/// </summary>
public class ServiceConfigurationAggregate : Aggregate<ServiceConfiguration>
{
    public ServiceConfigurationAggregate(ServiceConfiguration domain) : base(domain) { }

    /// <summary>
    /// Update the default type and parameters.
    /// </summary>
    public void SetDefaultType(string? assemblyName, string? typeName,
        Dictionary<string, string?>? parameters = null)
    {
        SetAttribute(nameof(ServiceConfiguration.AssemblyName), assemblyName);
        SetAttribute(nameof(ServiceConfiguration.TypeName), typeName);

        if (parameters is null)
        {
            SetAttribute(nameof(ServiceConfiguration.Parameters),
                (IReadOnlyDictionary<string, string>?)new Dictionary<string, string>());
        }
        else
        {
            var filtered = parameters
                .Where(kv => kv.Value is not null)
                .ToDictionary(kv => kv.Key, kv => kv.Value!);
            SetAttribute(nameof(ServiceConfiguration.Parameters),
                (IReadOnlyDictionary<string, string>)filtered);
        }
    }

    /// <summary>
    /// Set or update a flagged dependency.
    /// </summary>
    public void SetDependency(string flag, string assemblyName, string typeName,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        var deps = new List<FlaggedDependency>(Domain.Dependencies ?? []);

        // Replace existing dependency with same flag.
        deps.RemoveAll(d => d.Flag == flag);
        deps.Add(new FlaggedDependency(flag, assemblyName, typeName, parameters));

        SetAttribute(nameof(ServiceConfiguration.Dependencies),
            (IReadOnlyList<FlaggedDependency>)deps);
    }

    /// <summary>
    /// Remove a flagged dependency by its flag.
    /// </summary>
    public void RemoveDependency(string flag)
    {
        var deps = (Domain.Dependencies ?? [])
            .Where(d => d.Flag != flag)
            .ToList();
        SetAttribute(nameof(ServiceConfiguration.Dependencies),
            (IReadOnlyList<FlaggedDependency>)deps);
    }

    /// <summary>
    /// Get the service type based on the provided flags.
    /// Checks flagged dependencies first, then falls back to the default type.
    /// </summary>
    public Type? GetServiceType(params string[] flags)
    {
        // Check flagged dependencies first.
        foreach (var flag in flags)
        {
            var dep = Domain.GetDependency(flag);
            if (dep is not null)
                return ImportDependency.Execute(dep.AssemblyName, dep.TypeName);
        }

        // Fall back to default type.
        if (Domain.AssemblyName is not null && Domain.TypeName is not null)
            return ImportDependency.Execute(Domain.AssemblyName, Domain.TypeName);

        return null;
    }
}
