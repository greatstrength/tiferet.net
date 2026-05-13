using Tiferet.Core;

namespace Tiferet.Domain;

/// <summary>
/// Provides framework-level default error definitions as pre-constructed
/// <see cref="Error"/> records. Used as fallback when error definitions
/// are not found in the configured error repository.
/// </summary>
public static class DefaultErrors
{
    private static readonly Dictionary<string, Error> _errors = new()
    {
        [ErrorCodes.CommandParameterRequired] = Error.Create(
            id: ErrorCodes.CommandParameterRequired,
            name: "Command Parameter Required",
            messages: [new ErrorMessage("en_US", "Required parameter missing.")]),

        [ErrorCodes.FeatureNotFound] = Error.Create(
            id: ErrorCodes.FeatureNotFound,
            name: "Feature Not Found",
            messages: [new ErrorMessage("en_US", "Feature not found: {id}")]),

        [ErrorCodes.ErrorNotFound] = Error.Create(
            id: ErrorCodes.ErrorNotFound,
            name: "Error Not Found",
            messages: [new ErrorMessage("en_US", "Error not found: {id}")]),

        [ErrorCodes.AppInterfaceNotFound] = Error.Create(
            id: ErrorCodes.AppInterfaceNotFound,
            name: "App Interface Not Found",
            messages: [new ErrorMessage("en_US", "App interface not found: {id}")]),

        [ErrorCodes.InvalidModelAttribute] = Error.Create(
            id: ErrorCodes.InvalidModelAttribute,
            name: "Invalid Model Attribute",
            messages: [new ErrorMessage("en_US", "Invalid attribute: {attribute}")]),
    };

    /// <summary>All default error definitions.</summary>
    public static IReadOnlyDictionary<string, Error> All => _errors;

    /// <summary>
    /// Get a default error by its error code, or null if not defined.
    /// </summary>
    /// <param name="errorCode">The error code to look up.</param>
    /// <returns>The error record, or null.</returns>
    public static Error? Get(string errorCode)
    {
        return _errors.TryGetValue(errorCode, out var error) ? error : null;
    }
}
