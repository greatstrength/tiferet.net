using Tiferet.Domain.Error;

namespace Tiferet.Domain;

/// <summary>
/// Provides framework-level default error definitions as pre-constructed
/// <see cref="ErrorConfiguration"/> records. Used as fallback when error definitions
/// are not found in the configured error repository.
/// </summary>
public static class DefaultErrors
{
    private static ErrorConfiguration MakeDefault(string id, string name, string message)
    {
        var errorCode = id.ToUpperInvariant().Replace(' ', '_');
        return new ErrorConfiguration(
            Id: id, Name: name, ErrorCode: errorCode,
            Messages: [new ErrorMessageConfiguration("en_US", message)]);
    }

    private static readonly Dictionary<string, ErrorConfiguration> _errors = new()
    {
        [ErrorCodes.CommandParameterRequired] = MakeDefault(
            ErrorCodes.CommandParameterRequired,
            "Command Parameter Required",
            "Required parameter missing."),

        [ErrorCodes.FeatureNotFound] = MakeDefault(
            ErrorCodes.FeatureNotFound,
            "FeatureConfiguration Not Found",
            "FeatureConfiguration not found: {id}"),

        [ErrorCodes.ErrorNotFound] = MakeDefault(
            ErrorCodes.ErrorNotFound,
            "ErrorConfiguration Not Found",
            "ErrorConfiguration not found: {id}"),

        [ErrorCodes.AppInterfaceNotFound] = MakeDefault(
            ErrorCodes.AppInterfaceNotFound,
            "App Interface Not Found",
            "App interface not found: {id}"),

        [ErrorCodes.InvalidModelAttribute] = MakeDefault(
            ErrorCodes.InvalidModelAttribute,
            "Invalid Model Attribute",
            "Invalid attribute: {attribute}"),
    };

    /// <summary>All default error definitions.</summary>
    public static IReadOnlyDictionary<string, ErrorConfiguration> All => _errors;

    /// <summary>
    /// Get a default error by its error code, or null if not defined.
    /// </summary>
    /// <param name="errorCode">The error code to look up.</param>
    /// <returns>The error configuration, or null.</returns>
    public static ErrorConfiguration? Get(string errorCode)
    {
        return _errors.TryGetValue(errorCode, out var error) ? error : null;
    }
}
