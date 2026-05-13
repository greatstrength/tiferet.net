using Tiferet.Core;
using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers;

namespace Tiferet.Events.App;

// *** Parameter records

public sealed record AddAppInterfaceParams(
    string Id, string Name, string AssemblyName, string TypeName,
    string? Description = null, string LoggerId = "default",
    IReadOnlyList<string>? Flags = null,
    IReadOnlyList<AppServiceDependency>? Services = null,
    IReadOnlyDictionary<string, string>? Constants = null);

public sealed record GetAppInterfaceParams(string InterfaceId);

public sealed record UpdateAppInterfaceParams(string Id, string Attribute, object? Value);

public sealed record SetAppConstantsParams(string Id, Dictionary<string, string?>? Constants = null);

public sealed record SetServiceDependencyParams(
    string Id, string ServiceId, string AssemblyName, string TypeName,
    IReadOnlyDictionary<string, string>? Parameters = null);

public sealed record RemoveServiceDependencyParams(string Id, string ServiceId);

public sealed record RemoveAppInterfaceParams(string Id);

public sealed record ListAppInterfacesParams();

// *** Events

public class AddAppInterface : DomainEvent<AddAppInterfaceParams, AppInterfaceAggregate>
{
    private readonly IAppService _appService;
    public AddAppInterface(IAppService appService) => _appService = appService;

    public override AppInterfaceAggregate Execute(AddAppInterfaceParams p)
    {
        var domain = new AppInterface(p.Id, p.Name, p.AssemblyName, p.TypeName,
            p.Description, p.LoggerId, p.Flags, p.Services, p.Constants);
        var aggregate = new AppInterfaceAggregate(domain);
        _appService.Save(aggregate);
        return aggregate;
    }
}

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

public class UpdateAppInterface : DomainEvent<UpdateAppInterfaceParams, string>
{
    private readonly IAppService _appService;
    public UpdateAppInterface(IAppService appService) => _appService = appService;

    public override string Execute(UpdateAppInterfaceParams p)
    {
        var iface = _appService.Get(p.Id);
        Verify(iface is not null, ErrorCodes.AppInterfaceNotFound, null, ("interfaceId", p.Id));

        // Dispatch to the appropriate typed setter.
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

public class SetAppConstants : DomainEvent<SetAppConstantsParams, string>
{
    private readonly IAppService _appService;
    public SetAppConstants(IAppService appService) => _appService = appService;

    public override string Execute(SetAppConstantsParams p)
    {
        var iface = _appService.Get(p.Id);
        Verify(iface is not null, ErrorCodes.AppInterfaceNotFound, null, ("interfaceId", p.Id));

        iface!.SetConstants(p.Constants);
        _appService.Save(iface);
        return p.Id;
    }
}

public class SetServiceDependency : DomainEvent<SetServiceDependencyParams, string>
{
    private readonly IAppService _appService;
    public SetServiceDependency(IAppService appService) => _appService = appService;

    public override string Execute(SetServiceDependencyParams p)
    {
        var iface = _appService.Get(p.Id);
        Verify(iface is not null, ErrorCodes.AppInterfaceNotFound, null, ("interfaceId", p.Id));

        iface!.SetService(p.ServiceId, p.AssemblyName, p.TypeName, p.Parameters);
        _appService.Save(iface);
        return p.Id;
    }
}

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

public class ListAppInterfaces : DomainEvent<ListAppInterfacesParams, IReadOnlyList<AppInterfaceAggregate>>
{
    private readonly IAppService _appService;
    public ListAppInterfaces(IAppService appService) => _appService = appService;

    public override IReadOnlyList<AppInterfaceAggregate> Execute(ListAppInterfacesParams p)
        => _appService.List();
}
