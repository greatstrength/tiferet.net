using Tiferet.Interfaces;
using Tiferet.Mappers.App;

namespace Tiferet.Events.App;

public sealed record RemoveAppInterfaceParams(string Id);

public class RemoveAppInterface : DomainEvent<RemoveAppInterfaceParams, string>
{
    private readonly IAppService _appService;
    public RemoveAppInterface(IAppService appService) => _appService = appService;

    public override string Execute(RemoveAppInterfaceParams p)
    {
        _appService.Delete(p.Id);
        return p.Id;
    }
}
