using Tiferet.Interfaces;

namespace Tiferet.Events.DI;

public sealed record SetServiceConstantsParams(Dictionary<string, string?>? Constants = null);

public class SetServiceConstants : DomainEvent<SetServiceConstantsParams, Dictionary<string, string>>
{
    private readonly IDIService _diService;
    public SetServiceConstants(IDIService diService) => _diService = diService;

    public override Dictionary<string, string> Execute(SetServiceConstantsParams p)
    {
        var (_, currentConstants) = _diService.ListAll();
        Dictionary<string, string> updated;

        if (p.Constants is null)
        {
            updated = new();
        }
        else
        {
            updated = new(currentConstants);
            foreach (var (key, value) in p.Constants)
            {
                if (value is null)
                    updated.Remove(key);
                else
                    updated[key] = value;
            }
        }

        _diService.SaveConstants(updated);
        return updated;
    }
}
