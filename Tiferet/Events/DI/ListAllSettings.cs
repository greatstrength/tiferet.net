using Tiferet.Interfaces;
using Tiferet.Mappers.DI;

namespace Tiferet.Events.DI;

public sealed record ListAllSettingsParams();

public class ListAllSettings : DomainEvent<ListAllSettingsParams,
    (IReadOnlyList<ServiceConfigurationAggregate> Configurations, Dictionary<string, string> Constants)>
{
    private readonly IDIService _diService;
    public ListAllSettings(IDIService diService) => _diService = diService;

    public override (IReadOnlyList<ServiceConfigurationAggregate>, Dictionary<string, string>) Execute(ListAllSettingsParams p)
        => _diService.ListAll();
}
