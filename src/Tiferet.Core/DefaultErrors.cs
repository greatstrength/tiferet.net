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
            ["Id"] = ErrorCodes.CommandParameterRequired,
            ["Name"] = "Command Parameter Required",
            ["ErrorCode"] = ErrorCodes.CommandParameterRequired,
            ["Messages"] = new List<Dictionary<string, string>>
            {
                new() { ["Lang"] = "en_US", ["Text"] = "Required parameter missing." },
            },
        },
        [ErrorCodes.FeatureNotFound] = new()
        {
            ["Id"] = ErrorCodes.FeatureNotFound,
            ["Name"] = "Feature Not Found",
            ["ErrorCode"] = ErrorCodes.FeatureNotFound,
            ["Messages"] = new List<Dictionary<string, string>>
            {
                new() { ["Lang"] = "en_US", ["Text"] = "Feature not found: {id}" },
            },
        },
        [ErrorCodes.ErrorNotFound] = new()
        {
            ["Id"] = ErrorCodes.ErrorNotFound,
            ["Name"] = "Error Not Found",
            ["ErrorCode"] = ErrorCodes.ErrorNotFound,
            ["Messages"] = new List<Dictionary<string, string>>
            {
                new() { ["Lang"] = "en_US", ["Text"] = "Error not found: {id}" },
            },
        },
        [ErrorCodes.AppInterfaceNotFound] = new()
        {
            ["Id"] = ErrorCodes.AppInterfaceNotFound,
            ["Name"] = "App Interface Not Found",
            ["ErrorCode"] = ErrorCodes.AppInterfaceNotFound,
            ["Messages"] = new List<Dictionary<string, string>>
            {
                new() { ["Lang"] = "en_US", ["Text"] = "App interface not found: {id}" },
            },
        },
        [ErrorCodes.InvalidModelAttribute] = new()
        {
            ["Id"] = ErrorCodes.InvalidModelAttribute,
            ["Name"] = "Invalid Model Attribute",
            ["ErrorCode"] = ErrorCodes.InvalidModelAttribute,
            ["Messages"] = new List<Dictionary<string, string>>
            {
                new() { ["Lang"] = "en_US", ["Text"] = "Invalid attribute: {attribute}" },
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
