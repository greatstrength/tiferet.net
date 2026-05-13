using System.Reflection;
using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>
/// Configuration for a serialization role, controlling which properties
/// to include/exclude and how to serialize them.
/// </summary>
public sealed class RoleConfig
{
    /// <summary>Properties to exclude from serialization.</summary>
    public HashSet<string> Exclude { get; init; } = [];

    /// <summary>Properties to include (if set, only these are serialized).</summary>
    public HashSet<string>? Include { get; init; }

    /// <summary>Whether to use alias names (e.g., snake_case) in output.</summary>
    public bool ByAlias { get; init; }

    /// <summary>Whether to exclude null values (default: true).</summary>
    public bool ExcludeNull { get; init; } = true;
}

/// <summary>
/// Non-generic base class for transfer objects.
/// Provides role-based serialization via <see cref="Roles"/> and <see cref="ToDictionary"/>.
/// Use <see cref="TransferObject{TDomain, TAggregate}"/> for typed mapping to aggregates,
/// or extend this directly for composite DTOs that don't map to a single aggregate.
/// </summary>
public abstract class TransferObject
{
    /// <summary>
    /// Role definitions for serialization. Subclasses override to define roles
    /// like "ToModel" and "ToDataYaml".
    /// </summary>
    protected virtual Dictionary<string, RoleConfig> Roles { get; } = new();

    /// <summary>
    /// Serialize this transfer object to a dictionary, optionally applying
    /// a named role configuration.
    /// </summary>
    /// <param name="role">The serialization role to apply (optional).</param>
    /// <param name="overrides">Additional property values that override the serialized output.</param>
    /// <returns>A dictionary of property names to values.</returns>
    public virtual Dictionary<string, object?> ToDictionary(string? role = null, Dictionary<string, object?>? overrides = null)
    {
        // Resolve role config if provided.
        RoleConfig? config = null;
        if (role is not null && Roles.TryGetValue(role, out var rc))
            config = rc;

        // Get all public instance properties on the concrete transfer object.
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name != nameof(Roles) && p.CanRead);

        var result = new Dictionary<string, object?>();

        foreach (var prop in properties)
        {
            var name = prop.Name;

            // Apply include filter.
            if (config?.Include is not null && !config.Include.Contains(name))
                continue;

            // Apply exclude filter.
            if (config?.Exclude.Contains(name) == true)
                continue;

            var value = prop.GetValue(this);

            // Apply exclude-null filter.
            if (value is null && (config?.ExcludeNull ?? true))
                continue;

            // Use alias (camelCase → snake_case is not needed; ByAlias reserved for future use).
            result[name] = value;
        }

        // Apply overrides last so they always win.
        if (overrides is not null)
        {
            foreach (var (key, value) in overrides)
                result[key] = value;
        }

        return result;
    }
}

/// <summary>
/// Generic transfer object that bridges persistent configuration
/// and runtime domain aggregates.
/// Provides mapping to aggregates via <see cref="Map"/>
/// and construction from domain models via <see cref="FromModel{TTransfer}"/>.
/// </summary>
/// <typeparam name="TDomain">The domain record type.</typeparam>
/// <typeparam name="TAggregate">The target aggregate type.</typeparam>
public abstract class TransferObject<TDomain, TAggregate> : TransferObject
    where TDomain : DomainObject
    where TAggregate : Aggregate<TDomain>
{

    /// <summary>
    /// Map this transfer object to an aggregate instance.
    /// Serializes via the "ToModel" role and constructs the aggregate.
    /// </summary>
    /// <param name="overrides">Additional values merged into the data.</param>
    /// <returns>A new aggregate instance.</returns>
    public virtual TAggregate Map(Dictionary<string, object?>? overrides = null)
    {
        var data = ToDictionary("ToModel", overrides);
        return ConstructAggregate(data);
    }

    /// <summary>
    /// Create a transfer object from a domain model or aggregate.
    /// </summary>
    /// <typeparam name="TTransfer">The concrete transfer object type.</typeparam>
    /// <param name="model">The source domain record.</param>
    /// <param name="overrides">Additional values that take priority.</param>
    /// <returns>A new transfer object instance.</returns>
    public static TTransfer FromModel<TTransfer>(TDomain model, Dictionary<string, object?>? overrides = null)
        where TTransfer : TransferObject<TDomain, TAggregate>, new()
    {
        var transfer = new TTransfer();

        // Get properties from the domain model.
        var modelProps = typeof(TDomain).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Get settable properties on the transfer object.
        var transferProps = typeof(TTransfer).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name);

        // Copy matching properties from the domain model.
        foreach (var mp in modelProps)
        {
            if (transferProps.TryGetValue(mp.Name, out var tp))
            {
                var value = mp.GetValue(model);
                tp.SetValue(transfer, value);
            }
        }

        // Apply overrides.
        if (overrides is not null)
        {
            foreach (var (key, value) in overrides)
            {
                if (transferProps.TryGetValue(key, out var tp))
                    tp.SetValue(transfer, value);
            }
        }

        return transfer;
    }

    /// <summary>
    /// Construct an aggregate from a property dictionary using reflection.
    /// Finds a constructor on the aggregate whose domain record parameter can be built.
    /// </summary>
    private static TAggregate ConstructAggregate(Dictionary<string, object?> data)
    {
        // Build the domain record from the data dictionary.
        var domain = ConstructDomain(data);

        // Construct the aggregate with the domain record.
        return (TAggregate)Activator.CreateInstance(typeof(TAggregate), domain)!;
    }

    /// <summary>
    /// Construct a domain record from a property dictionary using reflection.
    /// Matches dictionary keys to constructor parameters.
    /// </summary>
    private static TDomain ConstructDomain(Dictionary<string, object?> data)
    {
        var ctors = typeof(TDomain).GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        // Find the primary constructor (records have one with all properties).
        var ctor = ctors.OrderByDescending(c => c.GetParameters().Length).First();
        var parameters = ctor.GetParameters();

        var args = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var key = param.Name!;

            // Try PascalCase match first, then case-insensitive.
            if (data.TryGetValue(key, out var value))
            {
                args[i] = ConvertValue(value, param.ParameterType);
            }
            else
            {
                // Try PascalCase key.
                var pascalKey = char.ToUpperInvariant(key[0]) + key[1..];
                if (data.TryGetValue(pascalKey, out value))
                {
                    args[i] = ConvertValue(value, param.ParameterType);
                }
                else if (param.HasDefaultValue)
                {
                    args[i] = param.DefaultValue;
                }
                else
                {
                    args[i] = param.ParameterType.IsValueType
                        ? Activator.CreateInstance(param.ParameterType)
                        : null;
                }
            }
        }

        return (TDomain)ctor.Invoke(args);
    }

    /// <summary>
    /// Convert a value to the target type, handling common type mismatches.
    /// </summary>
    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value is null)
            return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlying.IsAssignableFrom(value.GetType()))
            return value;

        return Convert.ChangeType(value, underlying);
    }
}
