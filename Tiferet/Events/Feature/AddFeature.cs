using Tiferet.Assets;
using Tiferet.Domain.Feature;
using Tiferet.Interfaces;
using Tiferet.Mappers.Feature;

namespace Tiferet.Events.Feature;

public sealed record AddFeatureParams(
    string Name, string GroupId,
    string? FeatureKey = null, string? Id = null, string? Description = null,
    IReadOnlyList<FeatureEventConfiguration>? Steps = null,
    IReadOnlyDictionary<string, string>? LogParams = null);

public class AddFeature : DomainEvent<AddFeatureParams, FeatureAggregate>
{
    private readonly IFeatureService _featureService;
    public AddFeature(IFeatureService featureService) => _featureService = featureService;

    public override FeatureAggregate Execute(AddFeatureParams p)
    {
        var feature = FeatureConfiguration.Create(
            name: p.Name, groupId: p.GroupId, featureKey: p.FeatureKey,
            id: p.Id, description: p.Description,
            steps: p.Steps, logParams: p.LogParams);

        Verify(!_featureService.Exists(feature.Id),
            ErrorCodes.FeatureAlreadyExists, $"FeatureConfiguration with ID {feature.Id} already exists.",
            ("id", feature.Id));

        var aggregate = new FeatureAggregate(feature);
        _featureService.Save(aggregate);
        return aggregate;
    }
}
