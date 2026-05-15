using Tiferet.Interfaces;

namespace Tiferet.Events.Feature;

public sealed record RemoveFeatureParams(string Id);

public class RemoveFeature : DomainEvent<RemoveFeatureParams, string>
{
    private readonly IFeatureService _featureService;
    public RemoveFeature(IFeatureService featureService) => _featureService = featureService;

    public override string Execute(RemoveFeatureParams p)
    {
        _featureService.Delete(p.Id);
        return p.Id;
    }
}
