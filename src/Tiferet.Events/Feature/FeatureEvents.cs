using Tiferet.Core;
using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers;

namespace Tiferet.Events.Feature;

// *** Parameter records

public sealed record AddFeatureParams(
    string Name, string GroupId,
    string? FeatureKey = null, string? Id = null, string? Description = null,
    IReadOnlyList<FeatureEvent>? Steps = null,
    IReadOnlyDictionary<string, string>? LogParams = null);

public sealed record GetFeatureParams(string Id);

public sealed record ListFeaturesParams(string? GroupId = null);

public sealed record RemoveFeatureParams(string Id);

public sealed record UpdateFeatureParams(string Id, string Attribute, object? Value);

public sealed record AddFeatureStepParams(
    string Id, string Name, string ServiceId,
    IReadOnlyDictionary<string, string>? Parameters = null,
    string? DataKey = null, bool PassOnError = false,
    string? Condition = null, int? Position = null);

public sealed record UpdateFeatureStepParams(
    string Id, int Position, string Attribute, object? Value = null);

public sealed record RemoveFeatureStepParams(string Id, int Position);

public sealed record ReorderFeatureStepParams(
    string Id, int StartPosition, int EndPosition);

// *** Events

public class AddFeature : DomainEvent<AddFeatureParams, FeatureAggregate>
{
    private readonly IFeatureService _featureService;
    public AddFeature(IFeatureService featureService) => _featureService = featureService;

    public override FeatureAggregate Execute(AddFeatureParams p)
    {
        var feature = Domain.Feature.Create(
            name: p.Name, groupId: p.GroupId, featureKey: p.FeatureKey,
            id: p.Id, description: p.Description,
            steps: p.Steps, logParams: p.LogParams);

        Verify(!_featureService.Exists(feature.Id),
            ErrorCodes.FeatureAlreadyExists, $"Feature with ID {feature.Id} already exists.",
            ("id", feature.Id));

        var aggregate = new FeatureAggregate(feature);
        _featureService.Save(aggregate);
        return aggregate;
    }
}

public class GetFeature : DomainEvent<GetFeatureParams, FeatureAggregate>
{
    private readonly IFeatureService _featureService;
    public GetFeature(IFeatureService featureService) => _featureService = featureService;

    public override FeatureAggregate Execute(GetFeatureParams p)
    {
        var feature = _featureService.Get(p.Id);
        Verify(feature is not null, ErrorCodes.FeatureNotFound,
            $"Feature not found: {p.Id}", ("featureId", p.Id));
        return feature!;
    }
}

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

public class UpdateFeature : DomainEvent<UpdateFeatureParams, FeatureAggregate>
{
    private readonly IFeatureService _featureService;
    public UpdateFeature(IFeatureService featureService) => _featureService = featureService;

    public override FeatureAggregate Execute(UpdateFeatureParams p)
    {
        Verify(p.Attribute is "Name" or "Description",
            ErrorCodes.InvalidFeatureAttribute,
            $"Invalid feature attribute: {p.Attribute}", ("attribute", p.Attribute));

        var feature = _featureService.Get(p.Id);
        Verify(feature is not null, ErrorCodes.FeatureNotFound, null, ("featureId", p.Id));

        if (p.Attribute == "Name") feature!.Rename((string)p.Value!);
        else if (p.Attribute == "Description") feature!.SetDescription((string?)p.Value);

        _featureService.Save(feature!);
        return feature!;
    }
}

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

public class RemoveFeatureStep : DomainEvent<RemoveFeatureStepParams, string>
{
    private readonly IFeatureService _featureService;
    public RemoveFeatureStep(IFeatureService featureService) => _featureService = featureService;

    public override string Execute(RemoveFeatureStepParams p)
    {
        var feature = _featureService.Get(p.Id);
        Verify(feature is not null, ErrorCodes.FeatureNotFound, null, ("featureId", p.Id));

        feature!.RemoveStep(p.Position);
        _featureService.Save(feature);
        return p.Id;
    }
}

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
