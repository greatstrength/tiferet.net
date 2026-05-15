using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers.Feature;

namespace Tiferet.Events.Feature;

public sealed record GetFeatureParams(string Id);

public class GetFeature : DomainEvent<GetFeatureParams, FeatureAggregate>
{
    private readonly IFeatureService _featureService;
    public GetFeature(IFeatureService featureService) => _featureService = featureService;

    public override FeatureAggregate Execute(GetFeatureParams p)
    {
        var feature = _featureService.Get(p.Id);
        Verify(feature is not null, ErrorCodes.FeatureNotFound,
            $"FeatureConfiguration not found: {p.Id}", ("featureId", p.Id));
        return feature!;
    }
}
