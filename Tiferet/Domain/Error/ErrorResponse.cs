namespace Tiferet.Domain.Error;

/// <summary>
/// A structured error response with formatted message and optional context.
/// </summary>
/// <param name="ErrorCode">The error code.</param>
/// <param name="Name">The error name.</param>
/// <param name="Message">The formatted error message.</param>
/// <param name="Context">Optional additional context from the error arguments.</param>
public sealed record ErrorResponse(
    string ErrorCode,
    string Name,
    string Message,
    IReadOnlyDictionary<string, object>? Context = null);
