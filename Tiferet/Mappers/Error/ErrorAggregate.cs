using Tiferet.Domain.Error;

namespace Tiferet.Mappers.Error;

/// <summary>Aggregate for mutable error operations.</summary>
public class ErrorAggregate : Aggregate<ErrorConfiguration>
{
    public ErrorAggregate(ErrorConfiguration domain) : base(domain) { }

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
