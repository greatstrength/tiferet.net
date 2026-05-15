using Tiferet.Assets;
using Tiferet.Interfaces;
using Tiferet.Mappers.App;

namespace Tiferet.Events.App;

public sealed record RemoveServiceDependencyParams(string Id, string ServiceId);

public class RemoveServiceDependency : DomainEvent<RemoveServiceDependencyParams, string>
{
    private readonly IAppService _appService;
    public RemoveServiceDependency(IAppService appService) => _appService = appService;

    public override string Execute(RemoveServiceDependencyParams p)
    {
        var iface = _appService.Get(p.Id);
        Verify(iface is not null, ErrorCodes.AppInterfaceNotFound, null, ("interfaceId", p.Id));

        iface!.RemoveService(p.ServiceId);
        _appService.Save(iface);
        return p.Id;
    }
}
