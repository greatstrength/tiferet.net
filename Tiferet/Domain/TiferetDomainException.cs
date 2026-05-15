using System.ComponentModel.DataAnnotations;

namespace Tiferet.Domain;

/// <summary>
/// A single field-level validation failure from a domain object.
/// </summary>
/// <param name="Member">The field or property that failed validation.</param>
/// <param name="Message">The human-readable validation message.</param>
public record DomainValidationFailure(string Member, string Message);

/// <summary>
/// Raised when a domain object fails structural validation (Data Annotations).
/// Specific to model state — not a general framework error.
/// </summary>
public class TiferetDomainException : Exception
{
    /// <summary>The structured list of field-level validation failures.</summary>
    public IReadOnlyList<DomainValidationFailure> Failures { get; }

    /// <summary>
    /// Initializes a new <see cref="TiferetDomainException"/>.
    /// </summary>
    /// <param name="failures">The validation failures that caused this exception.</param>
    public TiferetDomainException(IReadOnlyList<DomainValidationFailure> failures)
        : base(BuildMessage(failures))
    {
        Failures = failures;
    }

    private static string BuildMessage(IReadOnlyList<DomainValidationFailure> failures)
        => string.Join("; ", failures.Select(f => $"{f.Member}: {f.Message}"));
}
