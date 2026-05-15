using Tiferet.Domain;
using Tiferet.Domain.DI;
using Tiferet.Interfaces;

namespace Tiferet.Events.DI;

public sealed record SetDefaultServiceConfigurationParams(
    string Id, string? AssemblyName = null, string? TypeName = null,
    Dictionary<string, string?>? Parameters = null);

public class SetDefaultServiceConfiguration : DomainEvent<SetDefaultServiceConfigurationParams, ServiceConfiguration>
{
    private readonly IDIService _diService;
    public SetDefaultServiceConfiguration(IDIService diService) => _diService = diService;

    public override ServiceConfiguration Execute(SetDefaultServiceConfigurationParams p)
    {
        var config = VerifyNotNull(_diService.GetConfiguration(p.Id),
            ErrorCodes.ServiceConfigurationNotFound, context: ("id", p.Id));

        config.SetDefaultType(p.AssemblyName, p.TypeName, p.Parameters);
        _diService.SaveConfiguration(config);
        return config.ToDomainObject();
    }
}
