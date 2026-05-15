using Tiferet.Interfaces;
using Tiferet.Mappers.App;

namespace Tiferet.Events.App;

public sealed record ListAppInterfacesParams();

public class ListAppInterfaces : DomainEvent<ListAppInterfacesParams, IReadOnlyList<AppInterfaceAggregate>>
{
    private readonly IAppService _appService;
    public ListAppInterfaces(IAppService appService) => _appService = appService;

    public override IReadOnlyList<AppInterfaceAggregate> Execute(ListAppInterfacesParams p)
        => _appService.List();
}
