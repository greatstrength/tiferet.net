using Tiferet.Domain.DI;
using Tiferet.Events;

namespace Tiferet.Mappers.DI;

/// <summary>Aggregate for mutable flagged dependency operations.</summary>
public class FlaggedDependencyAggregate : Aggregate<FlaggedDependencyConfiguration>
{
    public FlaggedDependencyAggregate(FlaggedDependencyConfiguration domain) : base(domain) { }

    public void SetParameters(Dictionary<string, string?>? parameters)
    {
        if (parameters is null)
        {
            SetAttribute(nameof(FlaggedDependencyConfiguration.Parameters),
                (IReadOnlyDictionary<string, string>?)new Dictionary<string, string>());
            return;
        }

        var merged = new Dictionary<string, string>(Domain.Parameters ?? new Dictionary<string, string>());
        foreach (var (key, value) in parameters)
        {
            if (value is null) merged.Remove(key);
            else merged[key] = value;
        }

        SetAttribute(nameof(FlaggedDependencyConfiguration.Parameters), (IReadOnlyDictionary<string, string>)merged);
    }
}

/// <summary>Aggregate for mutable service configuration operations.</summary>
public class ServiceConfigurationAggregate : Aggregate<ServiceConfiguration>
{
    public ServiceConfigurationAggregate(ServiceConfiguration domain) : base(domain) { }

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

    public void SetDependency(string flag, string assemblyName, string typeName,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        var deps = new List<FlaggedDependencyConfiguration>(Domain.Dependencies ?? []);
        deps.RemoveAll(d => d.Flag == flag);
        deps.Add(new FlaggedDependencyConfiguration(flag, assemblyName, typeName, parameters));
        SetAttribute(nameof(ServiceConfiguration.Dependencies), (IReadOnlyList<FlaggedDependencyConfiguration>)deps);
    }

    public void RemoveDependency(string flag)
    {
        var deps = (Domain.Dependencies ?? []).Where(d => d.Flag != flag).ToList();
        SetAttribute(nameof(ServiceConfiguration.Dependencies), (IReadOnlyList<FlaggedDependencyConfiguration>)deps);
    }

    public Type? GetServiceType(params string[] flags)
    {
        foreach (var flag in flags)
        {
            var dep = Domain.GetDependency(flag);
            if (dep is not null)
                return ImportDependency.Resolve(dep.AssemblyName, dep.TypeName);
        }

        if (Domain.AssemblyName is not null && Domain.TypeName is not null)
            return ImportDependency.Resolve(Domain.AssemblyName, Domain.TypeName);

        return null;
    }
}
