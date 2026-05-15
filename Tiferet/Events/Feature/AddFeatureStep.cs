using Tiferet.Domain;
using Tiferet.Interfaces;

namespace Tiferet.Events.Feature;

public sealed record AddFeatureStepParams(
    string Id, string Name, string ServiceId,
    IReadOnlyDictionary<string, string>? Parameters = null,
    string? DataKey = null, bool PassOnError = false,
    string? Condition = null, int? Position = null);

public class AddFeatureStep : DomainEvent<AddFeatureStepParams, string>
{
    private readonly IFeatureService _featureService;
    public AddFeatureStep(IFeatureService featureService) => _featureService = featureService;

    public override string Execute(AddFeatureStepParams p)
    {
        var feature = _featureService.Get(p.Id);
        Verify(feature is not null, ErrorCodes.FeatureNotFound, null, ("featureId", p.Id));

        feature!.AddStep(p.Name, p.ServiceId, p.Parameters, p.DataKey,
            p.PassOnError, p.Condition, p.Position);
        _featureService.Save(feature);
        return p.Id;
    }
}
