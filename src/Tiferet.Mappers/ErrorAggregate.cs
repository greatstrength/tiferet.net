using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>
/// Aggregate for mutable error operations.
/// </summary>
public class ErrorAggregate : Aggregate<Error>
{
    public ErrorAggregate(Error domain) : base(domain) { }

    /// <summary>Rename the error.</summary>
    public void Rename(string name) => SetAttribute(nameof(Error.Name), name);

    /// <summary>
    /// Set or update the error message for the specified language.
    /// </summary>
    public void SetMessage(string lang, string text)
    {
        var messages = new List<ErrorMessage>(Domain.Messages ?? []);

        // Update in place if language exists.
        for (int i = 0; i < messages.Count; i++)
        {
            if (messages[i].Lang == lang)
            {
                messages[i] = messages[i] with { Text = text };
                SetAttribute(nameof(Error.Messages), (IReadOnlyList<ErrorMessage>)messages);
                return;
            }
        }

        // Add new message.
        messages.Add(new ErrorMessage(lang, text));
        SetAttribute(nameof(Error.Messages), (IReadOnlyList<ErrorMessage>)messages);
    }

    /// <summary>
    /// Remove the error message for the specified language.
    /// </summary>
    public void RemoveMessage(string lang)
    {
        var messages = (Domain.Messages ?? [])
            .Where(m => m.Lang != lang)
            .ToList();
        SetAttribute(nameof(Error.Messages), (IReadOnlyList<ErrorMessage>)messages);
    }
}
