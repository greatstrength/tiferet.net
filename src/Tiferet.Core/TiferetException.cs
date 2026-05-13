using System.Text.Json;

namespace Tiferet.Core;

/// <summary>
/// Base exception for all Tiferet-related errors.
/// Carries a structured error code and arbitrary context key-value pairs.
/// </summary>
public class TiferetException : Exception
{
    /// <summary>The structured error code identifying this error.</summary>
    public string ErrorCode { get; }

    /// <summary>Additional context key-value pairs associated with the error.</summary>
    public IReadOnlyDictionary<string, object> Context { get; }

    /// <summary>
    /// Initializes a new <see cref="TiferetException"/>.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="message">Optional human-readable message.</param>
    /// <param name="context">Optional context pairs providing error detail.</param>
    public TiferetException(
        string errorCode,
        string? message = null,
        params (string Key, object Value)[] context)
        : base(BuildJsonMessage(errorCode, message, context))
    {
        ErrorCode = errorCode;
        Context = context.Length > 0
            ? context.ToDictionary(c => c.Key, c => c.Value)
            : new Dictionary<string, object>();
    }

    private static string BuildJsonMessage(
        string errorCode,
        string? message,
        (string Key, object Value)[] context)
    {
        var payload = new Dictionary<string, object?>
        {
            ["error_code"] = errorCode,
            ["message"] = message,
        };

        foreach (var (key, value) in context)
            payload[key] = value;

        return JsonSerializer.Serialize(payload);
    }
}
