using Tiferet.Domain;
using Tiferet.Domain.DI;
using Tiferet.Events;

namespace Tiferet.Mappers.DI;

/// <summary>Aggregate for mutable flagged dependency operations.</summary>
public record FlaggedDependencyAggregate : Aggregate<FlaggedDependencyConfiguration>
{
    public FlaggedDependencyAggregate(FlaggedDependencyConfiguration state) : base(state) { }

    // Delegated properties.
    public string Flag => State.Flag;
    public string AssemblyName => State.AssemblyName;
    public string TypeName => State.TypeName;
    public IReadOnlyDictionary<string, string>? Parameters => State.Parameters;

    public void SetParameters(Dictionary<string, string?>? parameters)
    {
        if (parameters is null)
        {
            Mutate(s => s with { Parameters = new Dictionary<string, string>() });
            return;
        }

        var merged = new Dictionary<string, string>(Parameters ?? new Dictionary<string, string>());
        foreach (var (key, value) in parameters)
        {
            if (value is null) merged.Remove(key);
            else merged[key] = value;
        }

        Mutate(s => s with { Parameters = merged });
    }
}

/// <summary>Aggregate for mutable service configuration operations.</summary>
public record ServiceConfigurationAggregate : Aggregate<ServiceConfiguration>
{
    public ServiceConfigurationAggregate(ServiceConfiguration state) : base(state) { }

    // Delegated properties.
    public string Id => State.Id;
    public string? Name => State.Name;
    public string? AssemblyName => State.AssemblyName;
    public string? TypeName => State.TypeName;
    public IReadOnlyDictionary<string, string>? Parameters => State.Parameters;
    public IReadOnlyList<FlaggedDependencyConfiguration>? Dependencies => State.Dependencies;

    public void SetDefaultType(string? assemblyName, string? typeName,
        Dictionary<string, string?>? parameters = null)
    {
        IReadOnlyDictionary<string, string>? newParams;
        if (parameters is null)
        {
            newParams = new Dictionary<string, string>();
        }
        else
        {
            newParams = parameters
                .Where(kv => kv.Value is not null)
                .ToDictionary(kv => kv.Key, kv => kv.Value!);
        }

        Mutate(s => s with { AssemblyName = assemblyName, TypeName = typeName, Parameters = newParams });
    }

    public void SetDependency(string flag, string assemblyName, string typeName,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        var deps = new List<FlaggedDependencyConfiguration>(Dependencies ?? []);
        deps.RemoveAll(d => d.Flag == flag);
        deps.Add(new FlaggedDependencyConfiguration(flag, assemblyName, typeName, parameters));
        Mutate(s => s with { Dependencies = deps });
    }

    public void RemoveDependency(string flag)
    {
        var deps = (Dependencies ?? []).Where(d => d.Flag != flag).ToList();
        Mutate(s => s with { Dependencies = (IReadOnlyList<FlaggedDependencyConfiguration>)deps });
    }

    public Type? GetServiceType(params string[] flags)
    {
        foreach (var flag in flags)
        {
            var dep = GetDependency(flag);
            if (dep is not null)
                return ImportDependency.Resolve(dep.AssemblyName, dep.TypeName);
        }

        if (AssemblyName is not null && TypeName is not null)
            return ImportDependency.Resolve(AssemblyName, TypeName);

        return null;
    }

    /// <summary>
    /// Get the first flagged dependency matching any of the provided flags.
    /// Delegates to the domain record.
    /// </summary>
    public FlaggedDependencyConfiguration? GetDependency(params string[] flags)
        => State.GetDependency(flags);
}
