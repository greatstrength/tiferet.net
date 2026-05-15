using Tiferet.Domain;
using Tiferet.Domain.Feature;
using Tiferet.Interfaces;

namespace Tiferet.Events.Feature;

public sealed record GetFeatureParams(string Id);

public class GetFeature : DomainEvent<GetFeatureParams, FeatureConfiguration>
{
    private readonly IFeatureService _featureService;
    public GetFeature(IFeatureService featureService) => _featureService = featureService;

    public override FeatureConfiguration Execute(GetFeatureParams p)
    {
        var feature = _featureService.Get(p.Id);
        Verify(feature is not null, ErrorCodes.FeatureNotFound,
            $"FeatureConfiguration not found: {p.Id}", ("featureId", p.Id));
        return feature!.ToDomainObject();
    }
}
