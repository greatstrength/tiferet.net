using System.Reflection;
using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>
/// Well-known serialization role names.
/// </summary>
public static class SerializationRoles
{
    public const string ToModel = "ToModel";
    public const string ToDataYaml = "ToDataYaml";
}

/// <summary>
/// Configuration for a serialization role.
/// </summary>
public sealed class RoleConfig
{
    public HashSet<string> Exclude { get; init; } = [];
    public HashSet<string>? Include { get; init; }
    public bool ByAlias { get; init; }
    public bool ExcludeNull { get; init; } = true;
}

/// <summary>
/// Non-generic base class for transfer objects.
/// </summary>
public abstract class TransferObject
{
    protected virtual Dictionary<string, RoleConfig> Roles { get; } = new();

    public virtual Dictionary<string, object?> ToDictionary(string? role = null, Dictionary<string, object?>? overrides = null)
    {
        RoleConfig? config = null;
        if (role is not null && Roles.TryGetValue(role, out var rc))
            config = rc;

        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name != nameof(Roles) && p.CanRead);

        var result = new Dictionary<string, object?>();

        foreach (var prop in properties)
        {
            var name = prop.Name;
            if (config?.Include is not null && !config.Include.Contains(name)) continue;
            if (config?.Exclude.Contains(name) == true) continue;

            var value = prop.GetValue(this);
            if (value is null && (config?.ExcludeNull ?? true)) continue;

            result[name] = value;
        }

        if (overrides is not null)
        {
            foreach (var (key, value) in overrides)
                result[key] = value;
        }

        return result;
    }
}

/// <summary>
/// Generic transfer object that bridges persistent configuration and runtime domain aggregates.
/// Concrete subclasses must override <see cref="Map"/> to construct the aggregate
/// from the domain record (adapter pattern).
/// </summary>
public abstract class TransferObject<TAggregate> : TransferObject
    where TAggregate : Aggregate
{
    public abstract TAggregate Map(Dictionary<string, object?>? overrides = null);
}
