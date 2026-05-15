using Tiferet.Domain.Feature;
using Tiferet.Interfaces;

namespace Tiferet.Events.Feature;

public sealed record ListFeaturesParams(string? GroupId = null);

public class ListFeatures : DomainEvent<ListFeaturesParams, IReadOnlyList<FeatureConfiguration>>
{
    private readonly IFeatureService _featureService;
    public ListFeatures(IFeatureService featureService) => _featureService = featureService;

    public override IReadOnlyList<FeatureConfiguration> Execute(ListFeaturesParams p)
    {
        var all = _featureService.List();
        if (p.GroupId is null)
            return all.Select(f => f.ToDomainObject()).ToList();
        return all.Where(f => f.GroupId == p.GroupId).Select(f => f.ToDomainObject()).ToList();
    }
}
