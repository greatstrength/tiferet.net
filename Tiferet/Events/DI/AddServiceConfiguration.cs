using Tiferet.Assets;
using Tiferet.Domain.DI;
using Tiferet.Interfaces;
using Tiferet.Mappers.DI;

namespace Tiferet.Events.DI;

public sealed record AddServiceConfigurationParams(
    string Id, string? AssemblyName = null, string? TypeName = null,
    IReadOnlyDictionary<string, string>? Parameters = null,
    IReadOnlyList<FlaggedDependencyConfiguration>? Dependencies = null);

public class AddServiceConfiguration : DomainEvent<AddServiceConfigurationParams, ServiceConfigurationAggregate>
{
    private readonly IDIService _diService;
    public AddServiceConfiguration(IDIService diService) => _diService = diService;

    public override ServiceConfigurationAggregate Execute(AddServiceConfigurationParams p)
    {
        Verify(!_diService.ConfigurationExists(p.Id),
            ErrorCodes.ConfigurationAlreadyExists, null, ("id", p.Id));

        var hasDefault = p.AssemblyName is not null && p.TypeName is not null;
        var hasDeps = p.Dependencies is not null && p.Dependencies.Count > 0;
        Verify(hasDefault || hasDeps, ErrorCodes.InvalidServiceConfiguration);

        var domain = new ServiceConfiguration(p.Id, AssemblyName: p.AssemblyName,
            TypeName: p.TypeName, Parameters: p.Parameters, Dependencies: p.Dependencies);
        var aggregate = new ServiceConfigurationAggregate(domain);
        _diService.SaveConfiguration(aggregate);
        return aggregate;
    }
}
