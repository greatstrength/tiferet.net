using Tiferet.Core;
using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers;

namespace Tiferet.Events.DI;

// *** Parameter records

public sealed record ListAllSettingsParams();

public sealed record AddServiceConfigurationParams(
    string Id, string? AssemblyName = null, string? TypeName = null,
    IReadOnlyDictionary<string, string>? Parameters = null,
    IReadOnlyList<FlaggedDependency>? Dependencies = null);

public sealed record SetDefaultServiceConfigurationParams(
    string Id, string? AssemblyName = null, string? TypeName = null,
    Dictionary<string, string?>? Parameters = null);

public sealed record SetServiceDependencyParams(
    string Id, string Flag, string AssemblyName, string TypeName,
    IReadOnlyDictionary<string, string>? Parameters = null);

public sealed record RemoveServiceDependencyParams(string Id, string Flag);

public sealed record RemoveServiceConfigurationParams(string Id);

public sealed record SetServiceConstantsParams(Dictionary<string, string?>? Constants = null);

// *** Events

public class ListAllSettings : DomainEvent<ListAllSettingsParams,
    (IReadOnlyList<ServiceConfigurationAggregate> Configurations, Dictionary<string, string> Constants)>
{
    private readonly IDIService _diService;
    public ListAllSettings(IDIService diService) => _diService = diService;

    public override (IReadOnlyList<ServiceConfigurationAggregate>, Dictionary<string, string>) Execute(ListAllSettingsParams p)
        => _diService.ListAll();
}

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

public class SetServiceDependency : DomainEvent<SetServiceDependencyParams, string>
{
    private readonly IDIService _diService;
    public SetServiceDependency(IDIService diService) => _diService = diService;

    public override string Execute(SetServiceDependencyParams p)
    {
        var config = _diService.GetConfiguration(p.Id);
        Verify(config is not null, ErrorCodes.ServiceConfigurationNotFound, null, ("id", p.Id));

        config!.SetDependency(p.Flag, p.AssemblyName, p.TypeName, p.Parameters);
        _diService.SaveConfiguration(config);
        return p.Id;
    }
}

public class RemoveServiceDependency : DomainEvent<RemoveServiceDependencyParams, string>
{
    private readonly IDIService _diService;
    public RemoveServiceDependency(IDIService diService) => _diService = diService;

    public override string Execute(RemoveServiceDependencyParams p)
    {
        var config = _diService.GetConfiguration(p.Id);
        Verify(config is not null, ErrorCodes.ServiceConfigurationNotFound, null, ("id", p.Id));

        config!.RemoveDependency(p.Flag);

        var hasDefault = config.Domain.AssemblyName is not null && config.Domain.TypeName is not null;
        var hasDeps = config.Domain.Dependencies is not null && config.Domain.Dependencies.Count > 0;
        Verify(hasDefault || hasDeps, ErrorCodes.InvalidServiceConfiguration);

        _diService.SaveConfiguration(config);
        return p.Id;
    }
}

public class RemoveServiceConfiguration : DomainEvent<RemoveServiceConfigurationParams, string>
{
    private readonly IDIService _diService;
    public RemoveServiceConfiguration(IDIService diService) => _diService = diService;

    public override string Execute(RemoveServiceConfigurationParams p)
    {
        _diService.DeleteConfiguration(p.Id);
        return p.Id;
    }
}

public class SetServiceConstants : DomainEvent<SetServiceConstantsParams, Dictionary<string, string>>
{
    private readonly IDIService _diService;
    public SetServiceConstants(IDIService diService) => _diService = diService;

    public override Dictionary<string, string> Execute(SetServiceConstantsParams p)
    {
        var (_, currentConstants) = _diService.ListAll();
        Dictionary<string, string> updated;

        if (p.Constants is null)
        {
            updated = new();
        }
        else
        {
            updated = new(currentConstants);
            foreach (var (key, value) in p.Constants)
            {
                if (value is null)
                    updated.Remove(key);
                else
                    updated[key] = value;
            }
        }

        _diService.SaveConstants(updated);
        return updated;
    }
}
