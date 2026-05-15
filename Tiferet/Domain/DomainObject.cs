using System.ComponentModel.DataAnnotations;

namespace Tiferet.Domain;

/// <summary>
/// Abstract base record for all Tiferet domain models.
/// Provides value equality, immutability, and <c>with</c> expression support.
/// Domain objects are read-only; mutation logic lives on Aggregate subclasses.
/// </summary>
public abstract record DomainObject
{
    /// <summary>
    /// Validates the domain object using Data Annotations.
    /// Throws <see cref="TiferetDomainException"/> if any required fields fail validation.
    /// Internal so aggregate Create factories within the same assembly can call it.
    /// </summary>
    /// <param name="instance">The domain object instance to validate.</param>
    internal static void Validate(DomainObject instance)
    {
        // Run Data Annotations validation against the instance.
        var context = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);

        // Return early if no failures.
        if (results.Count == 0) return;

        // Map validation results to domain failures and throw.
        var failures = results
            .Select(r => new DomainValidationFailure(
                r.MemberNames.FirstOrDefault() ?? "unknown",
                r.ErrorMessage ?? "Validation failed."))
            .ToList();

        throw new TiferetDomainException(failures);
    }
}
