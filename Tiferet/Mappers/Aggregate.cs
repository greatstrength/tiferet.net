using System.Reflection;
using Tiferet.Domain;
using Tiferet.Events;

namespace Tiferet.Mappers;

/// <summary>
/// Non-generic marker base for all aggregates.
/// Used as the constraint in <see cref="TransferObject{TAggregate}"/> and
/// <see cref="Tiferet.Repositories.YamlRepository{TAggregate}"/>.
/// </summary>
public abstract record Aggregate : DomainObject;

/// <summary>
/// Generic adapter base for mutable domain aggregates.
/// Holds a private mutable instance of the domain record (<typeparamref name="TDomain"/>)
/// and delegates public properties to it. Mutations use native <c>with {}</c> expressions
/// on the internal record — no reflection bypass of init-only on <c>this</c>.
/// </summary>
/// <typeparam name="TDomain">The domain record type wrapped by this aggregate.</typeparam>
public abstract record Aggregate<TDomain> : Aggregate
    where TDomain : DomainObject
{
    /// <summary>The internal domain record state.</summary>
    protected TDomain State { get; private set; }

    /// <summary>
    /// Initializes the aggregate with the given domain record.
    /// </summary>
    /// <param name="state">The domain record to wrap.</param>
    protected Aggregate(TDomain state) => State = state;

    /// <summary>
    /// Type-safe mutation via native <c>with {}</c> expression.
    /// Preferred path for concrete aggregate methods.
    /// </summary>
    /// <param name="mutator">A function that produces a new domain record from the current state.</param>
    protected void Mutate(Func<TDomain, TDomain> mutator) => State = mutator(State);

    /// <summary>
    /// Name-based mutation for dynamic/generic scenarios.
    /// Clones the internal record via the compiler-generated <c>&lt;Clone&gt;$</c> method,
    /// sets the property on the clone, and replaces the internal state.
    /// Throws <see cref="TiferetException"/> with <see cref="ErrorCodes.InvalidModelAttribute"/>
    /// if the property does not exist on the domain record.
    /// </summary>
    /// <param name="attribute">The property name to update.</param>
    /// <param name="value">The new property value.</param>
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

        // Clone the internal record via the compiler-generated <Clone>$ method.
        var cloneMethod = typeof(TDomain).GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance);
        var clone = (TDomain)cloneMethod!.Invoke(State, null)!;

        // Set the property on the clone.
        property.SetValue(clone, value);

        // Replace internal state with the mutated clone.
        State = clone;
    }

    /// <summary>
    /// Returns the internal domain record.
    /// Used for serialization, persistence, and transfer object construction.
    /// </summary>
    /// <returns>The wrapped domain record.</returns>
    public TDomain ToDomainObject() => State;

    /// <summary>
    /// Delegates equality to the internal <see cref="State"/> record,
    /// consistent with domain-object value equality.
    /// </summary>
    public virtual bool Equals(Aggregate<TDomain>? other)
        => other is not null && EqualityComparer<TDomain>.Default.Equals(State, other.State);

    /// <summary>
    /// Hash code delegates to the internal <see cref="State"/> record.
    /// </summary>
    public override int GetHashCode() => State.GetHashCode();
}
