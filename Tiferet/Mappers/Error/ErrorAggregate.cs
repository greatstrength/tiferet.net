using Tiferet.Domain;
using Tiferet.Domain.Error;

namespace Tiferet.Mappers.Error;

/// <summary>Aggregate for mutable error operations.</summary>
public record ErrorAggregate : Aggregate<ErrorConfiguration>
{
    public ErrorAggregate(ErrorConfiguration state) : base(state) { }

    // Delegated properties.
    public string Id => State.Id;
    public string Name => State.Name;
    public string ErrorCode => State.ErrorCode;
    public string? Description => State.Description;
    public IReadOnlyList<ErrorMessageConfiguration>? Messages => State.Messages;

    /// <summary>
    /// Create a new ErrorAggregate, deriving ErrorCode from Id when not provided.
    /// Validates the resulting domain record and throws on failure.
    /// </summary>
    public static ErrorAggregate Create(
        string id,
        string name,
        string? errorCode = null,
        string? description = null,
        IReadOnlyList<ErrorMessageConfiguration>? messages = null)
    {
        // Derive ErrorCode from Id when not provided.
        errorCode ??= id.ToUpperInvariant().Replace(' ', '_');

        // Construct the domain record.
        var record = new ErrorConfiguration(
            Id: id,
            Name: name,
            ErrorCode: errorCode,
            Description: description,
            Messages: messages);

        // Validate — throws TiferetDomainException on failure.
        DomainObject.Validate(record);

        // Return the aggregate wrapping the validated record.
        return new ErrorAggregate(record);
    }

    public void Rename(string name) => Mutate(s => s with { Name = name });

    public void SetMessage(string lang, string text)
    {
        var messages = new List<ErrorMessageConfiguration>(Messages ?? []);

        for (int i = 0; i < messages.Count; i++)
        {
            if (messages[i].Lang == lang)
            {
                messages[i] = messages[i] with { Text = text };
                Mutate(s => s with { Messages = messages });
                return;
            }
        }

        messages.Add(new ErrorMessageConfiguration(lang, text));
        Mutate(s => s with { Messages = messages });
    }

    public void RemoveMessage(string lang)
    {
        var messages = (Messages ?? []).Where(m => m.Lang != lang).ToList();
        Mutate(s => s with { Messages = (IReadOnlyList<ErrorMessageConfiguration>)messages });
    }

    /// <summary>
    /// Format the error message text for the specified language.
    /// Delegates to the domain record.
    /// </summary>
    public string? FormatMessage(string lang = "en_US", Dictionary<string, object>? args = null)
        => State.FormatMessage(lang, args);

    /// <summary>
    /// Format a complete error response for the specified language.
    /// Delegates to the domain record.
    /// </summary>
    public ErrorResponse? FormatResponse(string lang = "en_US", Dictionary<string, object>? args = null)
        => State.FormatResponse(lang, args);
}
