using Tiferet.Assets;
using Tiferet.Interfaces;
using Tiferet.Mappers.DI;

namespace Tiferet.Events.DI;

public sealed record SetDefaultServiceConfigurationParams(
    string Id, string? AssemblyName = null, string? TypeName = null,
    Dictionary<string, string?>? Parameters = null);

public class SetDefaultServiceConfiguration : DomainEvent<SetDefaultServiceConfigurationParams, ServiceConfigurationAggregate>
{
    private readonly IDIService _diService;
    public SetDefaultServiceConfiguration(IDIService diService) => _diService = diService;

    public override ServiceConfigurationAggregate Execute(SetDefaultServiceConfigurationParams p)
    {
        var config = _diService.GetConfiguration(p.Id);
        Verify(config is not null, ErrorCodes.ServiceConfigurationNotFound, null, ("id", p.Id));

        config!.SetDefaultType(p.AssemblyName, p.TypeName, p.Parameters);
        _diService.SaveConfiguration(config);
        return config;
    }
}
