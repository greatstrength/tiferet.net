using Tiferet.Domain;
using Tiferet.Domain.Feature;
using Tiferet.Interfaces;

namespace Tiferet.Events.Feature;

public sealed record UpdateFeatureParams(string Id, string Attribute, object? Value);

public class UpdateFeature : DomainEvent<UpdateFeatureParams, FeatureConfiguration>
{
    private readonly IFeatureService _featureService;
    public UpdateFeature(IFeatureService featureService) => _featureService = featureService;

    public override FeatureConfiguration Execute(UpdateFeatureParams p)
    {
        Verify(p.Attribute is "Name" or "Description",
            ErrorCodes.InvalidFeatureAttribute,
            $"Invalid feature attribute: {p.Attribute}", ("attribute", p.Attribute));

        var feature = _featureService.Get(p.Id);
        Verify(feature is not null, ErrorCodes.FeatureNotFound, null, ("featureId", p.Id));

        if (p.Attribute == "Name") feature!.Rename((string)p.Value!);
        else if (p.Attribute == "Description") feature!.SetDescription((string?)p.Value);

        _featureService.Save(feature!);
        return feature!.ToDomainObject();
    }
}
