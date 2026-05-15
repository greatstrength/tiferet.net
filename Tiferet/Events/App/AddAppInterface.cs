using Tiferet.Domain;
using Tiferet.Domain.App;
using Tiferet.Interfaces;
using Tiferet.Mappers.App;

namespace Tiferet.Events.App;

public sealed record AddAppInterfaceParams(
    string Id, string Name, string AssemblyName, string TypeName,
    string? Description = null, string LoggerId = "default",
    IReadOnlyList<string>? Flags = null,
    IReadOnlyList<AppServiceDependencyConfiguration>? Services = null,
    IReadOnlyDictionary<string, string>? Constants = null);

public class AddAppInterface : DomainEvent<AddAppInterfaceParams, AppInterfaceAggregate>
{
    private readonly IAppService _appService;
    public AddAppInterface(IAppService appService) => _appService = appService;

    public override AppInterfaceAggregate Execute(AddAppInterfaceParams p)
    {
        var record = new AppInterfaceConfiguration(p.Id, p.Name, p.AssemblyName, p.TypeName,
            p.Description, p.LoggerId, p.Flags, p.Services, p.Constants);
        var aggregate = new AppInterfaceAggregate(record);
        _appService.Save(aggregate);
        return aggregate;
    }
}
