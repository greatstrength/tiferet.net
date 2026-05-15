using Tiferet.Domain;
using Tiferet.Domain.App;
using Tiferet.Interfaces;

namespace Tiferet.Events.App;

public sealed record GetAppInterfaceParams(string InterfaceId);

public class GetAppInterface : DomainEvent<GetAppInterfaceParams, AppInterfaceConfiguration>
{
    private readonly IAppService _appService;
    public GetAppInterface(IAppService appService) => _appService = appService;

    public override AppInterfaceConfiguration Execute(GetAppInterfaceParams p)
    {
        var iface = _appService.Get(p.InterfaceId);
        Verify(iface is not null, ErrorCodes.AppInterfaceNotFound,
            $"App interface not found: {p.InterfaceId}", ("interfaceId", p.InterfaceId));
        return iface!.ToDomainObject();
    }
}
