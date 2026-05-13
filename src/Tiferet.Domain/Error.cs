using System.Text.RegularExpressions;

namespace Tiferet.Domain;

/// <summary>
/// An error message object with language-specific text.
/// </summary>
/// <param name="Lang">The language of the error message text (e.g., "en_US").</param>
/// <param name="Text">The error message text, supporting named placeholders like {value}.</param>
public sealed record ErrorMessage(string Lang, string Text) : DomainObject
{
    /// <summary>
    /// Format the error message text with named-placeholder substitution.
    /// Replaces <c>{key}</c> with the corresponding value from <paramref name="args"/>.
    /// </summary>
    /// <param name="args">Named arguments for placeholder substitution.</param>
    /// <returns>The formatted error message text.</returns>
    public string Format(Dictionary<string, object>? args = null)
    {
        if (args is null || args.Count == 0)
            return Text;

        // Replace {key} placeholders with values from the dictionary.
        return Regex.Replace(Text, @"\{(\w+)\}", match =>
        {
            var key = match.Groups[1].Value;
            return args.TryGetValue(key, out var value) ? value?.ToString() ?? "" : match.Value;
        });
    }
}

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

/// <summary>
/// An error object with multilingual messages and formatting capabilities.
/// </summary>
/// <param name="Id">The unique identifier of the error.</param>
/// <param name="Name">The name of the error.</param>
/// <param name="ErrorCode">The structured error code (derived from Id if not provided).</param>
/// <param name="Description">The description of the error.</param>
/// <param name="Messages">The error message translations.</param>
public sealed record Error(
    string Id,
    string Name,
    string ErrorCode,
    string? Description = null,
    IReadOnlyList<ErrorMessage>? Messages = null) : DomainObject
{
    /// <summary>
    /// Create an Error with derivation logic for ErrorCode.
    /// Derives ErrorCode from Id (uppercased, spaces replaced with underscores) when not provided.
    /// </summary>
    /// <param name="id">The error identifier.</param>
    /// <param name="name">The error name.</param>
    /// <param name="errorCode">The error code (optional, derived from id).</param>
    /// <param name="description">The description.</param>
    /// <param name="messages">The error messages.</param>
    /// <returns>A fully-formed Error record.</returns>
    public static Error Create(
        string id,
        string name,
        string? errorCode = null,
        string? description = null,
        IReadOnlyList<ErrorMessage>? messages = null)
    {
        // Derive ErrorCode from Id when not provided.
        errorCode ??= id.ToUpperInvariant().Replace(' ', '_');

        return new Error(
            Id: id,
            Name: name,
            ErrorCode: errorCode,
            Description: description,
            Messages: messages);
    }

    /// <summary>
    /// Format the error message text for the specified language.
    /// </summary>
    /// <param name="lang">The language code (e.g., "en_US").</param>
    /// <param name="args">Named arguments for placeholder substitution.</param>
    /// <returns>The formatted message text, or null if no matching language found.</returns>
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
    /// <param name="lang">The language code.</param>
    /// <param name="args">Named arguments for placeholder substitution.</param>
    /// <returns>An <see cref="ErrorResponse"/> with the formatted message, or null if no message found.</returns>
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
