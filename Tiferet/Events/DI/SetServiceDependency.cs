using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers.DI;

namespace Tiferet.Events.DI;

public sealed record SetServiceDependencyParams(
    string Id, string Flag, string AssemblyName, string TypeName,
    IReadOnlyDictionary<string, string>? Parameters = null);

public class SetServiceDependency : DomainEvent<SetServiceDependencyParams, string>
{
    private readonly IDIService _diService;
    public SetServiceDependency(IDIService diService) => _diService = diService;

    public override string Execute(SetServiceDependencyParams p)
    {
        var config = VerifyNotNull(_diService.GetConfiguration(p.Id),
            ErrorCodes.ServiceConfigurationNotFound, context: ("id", p.Id));

        config.SetDependency(p.Flag, p.AssemblyName, p.TypeName, p.Parameters);
        _diService.SaveConfiguration(config);
        return p.Id;
    }
}
