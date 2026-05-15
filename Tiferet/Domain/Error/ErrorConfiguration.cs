using System.ComponentModel.DataAnnotations;

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
    [Required] string Id,
    [Required] string Name,
    [Required] string ErrorCode,
    string? Description = null,
    IReadOnlyList<ErrorMessageConfiguration>? Messages = null) : DomainObject
{
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
