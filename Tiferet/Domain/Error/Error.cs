namespace Tiferet.Domain.Error;

/// <summary>
/// An error object with multilingual messages and formatting capabilities.
/// </summary>
/// <param name="Id">The unique identifier of the error.</param>
/// <param name="Name">The name of the error.</param>
/// <param name="ErrorCode">The structured error code (derived from Id if not provided).</param>
/// <param name="Description">The description of the error.</param>
/// <param name="Messages">The error message translations.</param>
public sealed record ErrorConfiguration(
    string Id,
    string Name,
    string ErrorCode,
    string? Description = null,
    IReadOnlyList<ErrorMessageConfiguration>? Messages = null) : DomainObject
{
    /// <summary>
    /// Create an ErrorConfiguration with derivation logic for ErrorCode.
    /// </summary>
    public static ErrorConfiguration Create(
        string id,
        string name,
        string? errorCode = null,
        string? description = null,
        IReadOnlyList<ErrorMessageConfiguration>? messages = null)
    {
        // Derive ErrorCode from Id when not provided.
        errorCode ??= id.ToUpperInvariant().Replace(' ', '_');

        return new ErrorConfiguration(
            Id: id,
            Name: name,
            ErrorCode: errorCode,
            Description: description,
            Messages: messages);
    }

    /// <summary>
    /// Format the error message text for the specified language.
    /// </summary>
    public string? FormatMessage(string lang = "en_US", Dictionary<string, object>? args = null)
    {
        if (Messages is null) return null;

        foreach (var msg in Messages)
        {
            if (msg.Lang == lang)
                return msg.Format(args);
        }

        return null;
    }

    /// <summary>
    /// Format a complete error response for the specified language.
    /// </summary>
    public ErrorResponse? FormatResponse(string lang = "en_US", Dictionary<string, object>? args = null)
    {
        var message = FormatMessage(lang, args);
        if (message is null) return null;

        return new ErrorResponse(
            ErrorCode: Id,
            Name: Name,
            Message: message,
            Context: args is not null && args.Count > 0
                ? new Dictionary<string, object>(args)
                : null);
    }
}
