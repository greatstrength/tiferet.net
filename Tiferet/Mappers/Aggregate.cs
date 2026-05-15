using System.Reflection;
using Tiferet.Domain;
using Tiferet.Events;

namespace Tiferet.Mappers;

/// <summary>
/// Base class for mutable domain aggregates.
/// Wraps an immutable <typeparamref name="TDomain"/> record and exposes
/// validated mutation via <see cref="SetAttribute"/>.
/// </summary>
/// <typeparam name="TDomain">The domain record type.</typeparam>
public abstract class Aggregate<TDomain> where TDomain : DomainObject
{
    /// <summary>The underlying immutable domain record.</summary>
    public TDomain Domain { get; protected set; }

    /// <summary>
    /// Initializes the aggregate with the given domain record.
    /// </summary>
    protected Aggregate(TDomain domain)
    {
        Domain = domain;
    }

    /// <summary>
    /// Update a property on the underlying domain record by name,
    /// producing a new record via reflection-based <c>with</c> cloning.
    /// </summary>
    protected void SetAttribute(string attribute, object? value)
    {
        var property = typeof(TDomain).GetProperty(
            attribute,
            BindingFlags.Public | BindingFlags.Instance);

        if (property is null)
        {
            throw new TiferetException(
                ErrorCodes.InvalidModelAttribute,
                $"Property '{attribute}' does not exist on {typeof(TDomain).Name}.",
                ("attribute", attribute));
        }

        Domain = CloneWith(property, value);
    }

    private TDomain CloneWith(PropertyInfo property, object? value)
    {
        var cloneMethod = typeof(TDomain).GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException(
                $"{typeof(TDomain).Name} does not have a record clone method.");

        var clone = (TDomain)cloneMethod.Invoke(Domain, null)!;
        property.SetValue(clone, value);
        return clone;
    }
}
