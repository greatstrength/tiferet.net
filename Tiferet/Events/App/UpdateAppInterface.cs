using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers.App;

namespace Tiferet.Events.App;

public sealed record UpdateAppInterfaceParams(string Id, string Attribute, object? Value);

public class UpdateAppInterface : DomainEvent<UpdateAppInterfaceParams, string>
{
    private readonly IAppService _appService;
    public UpdateAppInterface(IAppService appService) => _appService = appService;

    public override string Execute(UpdateAppInterfaceParams p)
    {
        var iface = _appService.Get(p.Id);
        Verify(iface is not null, ErrorCodes.AppInterfaceNotFound, null, ("interfaceId", p.Id));

        switch (p.Attribute)
        {
            case "Name": iface!.Rename((string)p.Value!); break;
            case "Description": iface!.SetDescription((string?)p.Value); break;
            case "AssemblyName": iface!.SetAssemblyName((string)p.Value!); break;
            case "TypeName": iface!.SetTypeName((string)p.Value!); break;
            case "LoggerId": iface!.SetLoggerId((string)p.Value!); break;
            case "Flags": iface!.SetFlags((IReadOnlyList<string>)p.Value!); break;
            default:
                RaiseError(ErrorCodes.InvalidModelAttribute,
                    $"Invalid attribute: {p.Attribute}", ("attribute", p.Attribute));
                break;
        }

        _appService.Save(iface!);
        return p.Id;
    }
}
