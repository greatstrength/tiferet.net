using Tiferet.Domain;
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
        var iface = VerifyNotNull(_appService.Get(p.Id),
            ErrorCodes.AppInterfaceNotFound, context: ("interfaceId", p.Id));

        iface.RemoveService(p.ServiceId);
        _appService.Save(iface);
        return p.Id;
    }
}
