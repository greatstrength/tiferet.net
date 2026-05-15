using Tiferet.Domain.Error;

namespace Tiferet.Assets;

/// <summary>
/// Provides framework-level default error definitions as pre-constructed
/// <see cref="ErrorConfiguration"/> records. Used as fallback when error definitions
/// are not found in the configured error repository.
/// </summary>
public static class DefaultErrors
{
    private static readonly Dictionary<string, ErrorConfiguration> _errors = new()
    {
        [ErrorCodes.CommandParameterRequired] = ErrorConfiguration.Create(
            id: ErrorCodes.CommandParameterRequired,
            name: "Command Parameter Required",
            messages: [new ErrorMessageConfiguration("en_US", "Required parameter missing.")]),

        [ErrorCodes.FeatureNotFound] = ErrorConfiguration.Create(
            id: ErrorCodes.FeatureNotFound,
            name: "FeatureConfiguration Not Found",
            messages: [new ErrorMessageConfiguration("en_US", "FeatureConfiguration not found: {id}")]),

        [ErrorCodes.ErrorNotFound] = ErrorConfiguration.Create(
            id: ErrorCodes.ErrorNotFound,
            name: "ErrorConfiguration Not Found",
            messages: [new ErrorMessageConfiguration("en_US", "ErrorConfiguration not found: {id}")]),

        [ErrorCodes.AppInterfaceNotFound] = ErrorConfiguration.Create(
            id: ErrorCodes.AppInterfaceNotFound,
            name: "App Interface Not Found",
            messages: [new ErrorMessageConfiguration("en_US", "App interface not found: {id}")]),

        [ErrorCodes.InvalidModelAttribute] = ErrorConfiguration.Create(
            id: ErrorCodes.InvalidModelAttribute,
            name: "Invalid Model Attribute",
            messages: [new ErrorMessageConfiguration("en_US", "Invalid attribute: {attribute}")]),
    };

    /// <summary>All default error definitions.</summary>
    public static IReadOnlyDictionary<string, ErrorConfiguration> All => _errors;

    /// <summary>
    /// Get a default error by its error code, or null if not defined.
    /// </summary>
    /// <param name="errorCode">The error code to look up.</param>
    /// <returns>The error record, or null.</returns>
    public static ErrorConfiguration? Get(string errorCode)
    {
        return _errors.TryGetValue(errorCode, out var error) ? error : null;
    }
}
