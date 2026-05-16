using Tiferet.Domain;
using Tiferet.Interfaces;

namespace Tiferet.Events.Feature;

public sealed record RemoveFeatureStepParams(string Id, int Position);

public class RemoveFeatureStep : DomainEvent<RemoveFeatureStepParams, string>
{
    private readonly IFeatureService _featureService;
    public RemoveFeatureStep(IFeatureService featureService) => _featureService = featureService;

    public override string Execute(RemoveFeatureStepParams p)
    {
        var feature = VerifyNotNull(_featureService.Get(p.Id),
            ErrorCodes.FeatureNotFound, context: ("featureId", p.Id));

        feature.RemoveStep(p.Position);
        _featureService.Save(feature);
        return p.Id;
    }
}
