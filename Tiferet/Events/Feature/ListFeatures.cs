using Tiferet.Interfaces;
using Tiferet.Mappers.Feature;

namespace Tiferet.Events.Feature;

public sealed record ListFeaturesParams(string? GroupId = null);

public class ListFeatures : DomainEvent<ListFeaturesParams, IReadOnlyList<FeatureAggregate>>
{
    private readonly IFeatureService _featureService;
    public ListFeatures(IFeatureService featureService) => _featureService = featureService;

    public override IReadOnlyList<FeatureAggregate> Execute(ListFeaturesParams p)
    {
        var all = _featureService.List();
        if (p.GroupId is null) return all;
        return all.Where(f => f.Domain.GroupId == p.GroupId).ToList();
    }
}
