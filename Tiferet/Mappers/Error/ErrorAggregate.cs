using Tiferet.Domain;
using Tiferet.Domain.Error;

namespace Tiferet.Mappers.Error;

/// <summary>Aggregate for mutable error operations.</summary>
public class ErrorAggregate : Aggregate<ErrorConfiguration>
{
    public ErrorAggregate(ErrorConfiguration domain) : base(domain) { }

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
        var instance = new ErrorConfiguration(
            Id: id,
            Name: name,
            ErrorCode: errorCode,
            Description: description,
            Messages: messages);

        // Validate — throws TiferetDomainException on failure.
        DomainObject.Validate(instance);

        // Return the constructed aggregate.
        return new ErrorAggregate(instance);
    }

    public void Rename(string name) => SetAttribute(nameof(ErrorConfiguration.Name), name);

    public void SetMessage(string lang, string text)
    {
        var messages = new List<ErrorMessageConfiguration>(Domain.Messages ?? []);

        for (int i = 0; i < messages.Count; i++)
        {
            if (messages[i].Lang == lang)
            {
                messages[i] = messages[i] with { Text = text };
                SetAttribute(nameof(ErrorConfiguration.Messages), (IReadOnlyList<ErrorMessageConfiguration>)messages);
                return;
            }
        }

        messages.Add(new ErrorMessageConfiguration(lang, text));
        SetAttribute(nameof(ErrorConfiguration.Messages), (IReadOnlyList<ErrorMessageConfiguration>)messages);
    }

    public void RemoveMessage(string lang)
    {
        var messages = (Domain.Messages ?? []).Where(m => m.Lang != lang).ToList();
        SetAttribute(nameof(ErrorConfiguration.Messages), (IReadOnlyList<ErrorMessageConfiguration>)messages);
    }
}
