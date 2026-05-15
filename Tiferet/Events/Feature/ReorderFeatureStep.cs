using Tiferet.Domain;
using Tiferet.Interfaces;

namespace Tiferet.Events.Feature;

public sealed record ReorderFeatureStepParams(string Id, int StartPosition, int EndPosition);

public class ReorderFeatureStep : DomainEvent<ReorderFeatureStepParams, string>
{
    private readonly IFeatureService _featureService;
    public ReorderFeatureStep(IFeatureService featureService) => _featureService = featureService;

    public override string Execute(ReorderFeatureStepParams p)
    {
        var feature = _featureService.Get(p.Id);
        Verify(feature is not null, ErrorCodes.FeatureNotFound, null, ("featureId", p.Id));

        feature!.ReorderStep(p.StartPosition, p.EndPosition);
        _featureService.Save(feature);
        return p.Id;
    }
}
