using Tiferet.Domain.App;
using Tiferet.Interfaces;

namespace Tiferet.Events.App;

public sealed record ListAppInterfacesParams();

public class ListAppInterfaces : DomainEvent<ListAppInterfacesParams, IReadOnlyList<AppInterfaceConfiguration>>
{
    private readonly IAppService _appService;
    public ListAppInterfaces(IAppService appService) => _appService = appService;

    public override IReadOnlyList<AppInterfaceConfiguration> Execute(ListAppInterfacesParams p)
        => _appService.List().Select(a => a.ToDomainObject()).ToList();
}
