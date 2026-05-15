using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers.App;

namespace Tiferet.Events.App;

public sealed record SetServiceDependencyParams(
    string Id, string ServiceId, string AssemblyName, string TypeName,
    IReadOnlyDictionary<string, string>? Parameters = null);

public class SetServiceDependency : DomainEvent<SetServiceDependencyParams, string>
{
    private readonly IAppService _appService;
    public SetServiceDependency(IAppService appService) => _appService = appService;

    public override string Execute(SetServiceDependencyParams p)
    {
        var iface = VerifyNotNull(_appService.Get(p.Id),
            ErrorCodes.AppInterfaceNotFound, context: ("interfaceId", p.Id));

        iface.SetService(p.ServiceId, p.AssemblyName, p.TypeName, p.Parameters);
        _appService.Save(iface);
        return p.Id;
    }
}
