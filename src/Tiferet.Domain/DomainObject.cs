namespace Tiferet.Domain;

/// <summary>
/// Abstract base record for all Tiferet domain models.
/// Provides value equality, immutability, and <c>with</c> expression support.
/// Domain objects are read-only; mutation logic lives on Aggregate subclasses.
/// </summary>
public abstract record DomainObject;
