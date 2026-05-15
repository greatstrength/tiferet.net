using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers.App;

namespace Tiferet.Events.App;

public sealed record GetAppInterfaceParams(string InterfaceId);

public class GetAppInterface : DomainEvent<GetAppInterfaceParams, AppInterfaceAggregate>
{
    private readonly IAppService _appService;
    public GetAppInterface(IAppService appService) => _appService = appService;

    public override AppInterfaceAggregate Execute(GetAppInterfaceParams p)
    {
        var iface = _appService.Get(p.InterfaceId);
        Verify(iface is not null, ErrorCodes.AppInterfaceNotFound,
            $"App interface not found: {p.InterfaceId}", ("interfaceId", p.InterfaceId));
        return iface!;
    }
}
