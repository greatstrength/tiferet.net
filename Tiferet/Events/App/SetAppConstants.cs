using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers.App;

namespace Tiferet.Events.App;

public sealed record SetAppConstantsParams(string Id, Dictionary<string, string?>? Constants = null);

public class SetAppConstants : DomainEvent<SetAppConstantsParams, string>
{
    private readonly IAppService _appService;
    public SetAppConstants(IAppService appService) => _appService = appService;

    public override string Execute(SetAppConstantsParams p)
    {
        var iface = VerifyNotNull(_appService.Get(p.Id),
            ErrorCodes.AppInterfaceNotFound, context: ("interfaceId", p.Id));

        iface.SetConstants(p.Constants);
        _appService.Save(iface);
        return p.Id;
    }
}
