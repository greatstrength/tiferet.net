using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers.DI;

namespace Tiferet.Events.DI;

public sealed record RemoveServiceDependencyParams(string Id, string Flag);

public class RemoveServiceDependency : DomainEvent<RemoveServiceDependencyParams, string>
{
    private readonly IDIService _diService;
    public RemoveServiceDependency(IDIService diService) => _diService = diService;

    public override string Execute(RemoveServiceDependencyParams p)
    {
        var config = _diService.GetConfiguration(p.Id);
        Verify(config is not null, ErrorCodes.ServiceConfigurationNotFound, null, ("id", p.Id));

        config!.RemoveDependency(p.Flag);

        var hasDefault = config.AssemblyName is not null && config.TypeName is not null;
        var hasDeps = config.Dependencies is not null && config.Dependencies.Count > 0;
        Verify(hasDefault || hasDeps, ErrorCodes.InvalidServiceConfiguration);

        _diService.SaveConfiguration(config);
        return p.Id;
    }
}
