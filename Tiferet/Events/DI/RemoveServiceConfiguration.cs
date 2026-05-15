using Tiferet.Interfaces;

namespace Tiferet.Events.DI;

public sealed record RemoveServiceConfigurationParams(string Id);

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
