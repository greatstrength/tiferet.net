namespace Tiferet.Core;

/// <summary>
/// Provides framework-level default error definition skeletons.
/// Each entry is a dictionary with keys: "id", "name", "error_code", and "messages"
/// (where messages is a list of dictionaries with "lang" and "text").
/// Domain events hydrate these into <c>Error</c> records at runtime.
/// </summary>
public static class DefaultErrors
{
    private static readonly Dictionary<string, Dictionary<string, object>> _errors = new()
    {
        [ErrorCodes.CommandParameterRequired] = new()
        {
            ["id"] = ErrorCodes.CommandParameterRequired,
            ["name"] = "Command Parameter Required",
            ["error_code"] = ErrorCodes.CommandParameterRequired,
            ["messages"] = new List<Dictionary<string, string>>
            {
                new() { ["lang"] = "en_US", ["text"] = "Required parameter missing." },
            },
        },
        [ErrorCodes.FeatureNotFound] = new()
        {
            ["id"] = ErrorCodes.FeatureNotFound,
            ["name"] = "Feature Not Found",
            ["error_code"] = ErrorCodes.FeatureNotFound,
            ["messages"] = new List<Dictionary<string, string>>
            {
                new() { ["lang"] = "en_US", ["text"] = "Feature not found: {id}" },
            },
        },
        [ErrorCodes.ErrorNotFound] = new()
        {
            ["id"] = ErrorCodes.ErrorNotFound,
            ["name"] = "Error Not Found",
            ["error_code"] = ErrorCodes.ErrorNotFound,
            ["messages"] = new List<Dictionary<string, string>>
            {
                new() { ["lang"] = "en_US", ["text"] = "Error not found: {id}" },
            },
        },
        [ErrorCodes.AppInterfaceNotFound] = new()
        {
            ["id"] = ErrorCodes.AppInterfaceNotFound,
            ["name"] = "App Interface Not Found",
            ["error_code"] = ErrorCodes.AppInterfaceNotFound,
            ["messages"] = new List<Dictionary<string, string>>
            {
                new() { ["lang"] = "en_US", ["text"] = "App interface not found: {id}" },
            },
        },
        [ErrorCodes.InvalidModelAttribute] = new()
        {
            ["id"] = ErrorCodes.InvalidModelAttribute,
            ["name"] = "Invalid Model Attribute",
            ["error_code"] = ErrorCodes.InvalidModelAttribute,
            ["messages"] = new List<Dictionary<string, string>>
            {
                new() { ["lang"] = "en_US", ["text"] = "Invalid attribute: {attribute}" },
            },
        },
    };

    /// <summary>All default error definition skeletons.</summary>
    public static IReadOnlyDictionary<string, Dictionary<string, object>> All => _errors;

    /// <summary>
    /// Get a default error skeleton by its error code, or null if not defined.
    /// </summary>
    /// <param name="errorCode">The error code to look up.</param>
    /// <returns>The error skeleton dictionary, or null.</returns>
    public static Dictionary<string, object>? Get(string errorCode)
    {
        return _errors.TryGetValue(errorCode, out var error) ? error : null;
    }
}
