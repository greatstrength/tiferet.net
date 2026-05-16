using Tiferet.Domain.DI;
using Tiferet.Interfaces;

namespace Tiferet.Events.DI;

public sealed record ListAllSettingsParams();

public class ListAllSettings : DomainEvent<ListAllSettingsParams,
    (IReadOnlyList<ServiceConfiguration> Configurations, Dictionary<string, string> Constants)>
{
    private readonly IDIService _diService;
    public ListAllSettings(IDIService diService) => _diService = diService;

    public override (IReadOnlyList<ServiceConfiguration>, Dictionary<string, string>) Execute(ListAllSettingsParams p)
    {
        var (aggregates, constants) = _diService.ListAll();
        return (aggregates.Select(a => a.ToDomainObject()).ToList(), constants);
    }
}
