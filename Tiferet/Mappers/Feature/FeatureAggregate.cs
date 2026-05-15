using Tiferet.Domain;
using Tiferet.Domain.Feature;

namespace Tiferet.Mappers.Feature;

/// <summary>Aggregate for mutable feature event operations.</summary>
public record FeatureEventAggregate : Aggregate<FeatureEventConfiguration>
{
    public FeatureEventAggregate(FeatureEventConfiguration state) : base(state) { }

    // Delegated properties.
    public string Name => State.Name;
    public string ServiceId => State.ServiceId;
    public IReadOnlyList<string>? Flags => State.Flags;
    public IReadOnlyDictionary<string, string>? Parameters => State.Parameters;
    public string? DataKey => State.DataKey;
    public bool PassOnError => State.PassOnError;
    public string? Condition => State.Condition;

    public void SetPassOnError(bool value) => Mutate(s => s with { PassOnError = value });

    public void SetParameters(Dictionary<string, string>? parameters)
    {
        if (parameters is null) return;

        var merged = new Dictionary<string, string>(Parameters ?? new Dictionary<string, string>());
        foreach (var (key, value) in parameters)
            merged[key] = value;

        var cleaned = merged.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value);
        Mutate(s => s with { Parameters = cleaned });
    }
}

/// <summary>Aggregate for mutable feature operations.</summary>
public record FeatureAggregate : Aggregate<FeatureConfiguration>
{
    public FeatureAggregate(FeatureConfiguration state) : base(state) { }

    // Delegated properties.
    public string Id => State.Id;
    public string Name => State.Name;
    public string GroupId => State.GroupId;
    public string FeatureKey => State.FeatureKey;
    public IReadOnlyList<string>? Flags => State.Flags;
    public string? Description => State.Description;
    public IReadOnlyList<FeatureEventConfiguration>? Steps => State.Steps;
    public IReadOnlyDictionary<string, string>? LogParams => State.LogParams;

    /// <summary>
    /// Create a new FeatureAggregate with full derivation logic for Id, GroupId, FeatureKey, and Description.
    /// Validates the resulting domain record and throws on failure.
    /// </summary>
    public static FeatureAggregate Create(
        string name,
        string? groupId = null,
        string? featureKey = null,
        string? id = null,
        string? description = null,
        IReadOnlyList<string>? flags = null,
        IReadOnlyList<FeatureEventConfiguration>? steps = null,
        IReadOnlyDictionary<string, string>? logParams = null)
    {
        // Derive groupId and featureKey from id when id is dotted.
        if (id is not null && id.Contains('.') && (groupId is null || featureKey is null))
        {
            var parts = id.Split('.', 2);
            groupId ??= parts[0];
            featureKey ??= parts[1];
        }

        // Derive featureKey from name (snake_case) when missing.
        featureKey ??= name.ToLowerInvariant().Replace(' ', '_');

        // Derive id from groupId and featureKey when missing.
        if (id is null && groupId is not null)
            id = $"{groupId}.{featureKey}";

        // Default description to name when missing.
        description ??= name;

        // Construct the domain record.
        var record = new FeatureConfiguration(
            Id: id!,
            Name: name,
            GroupId: groupId!,
            FeatureKey: featureKey,
            Flags: flags,
            Description: description,
            Steps: steps,
            LogParams: logParams);

        // Validate — throws TiferetDomainException on failure.
        DomainObject.Validate(record);

        // Return the aggregate wrapping the validated record.
        return new FeatureAggregate(record);
    }

    public void Rename(string name) => Mutate(s => s with { Name = name });
    public void SetDescription(string? description) => Mutate(s => s with { Description = description });

    public FeatureEventConfiguration AddStep(
        string name, string serviceId,
        IReadOnlyDictionary<string, string>? parameters = null,
        string? dataKey = null, bool passOnError = false,
        string? condition = null, int? position = null)
    {
        var step = new FeatureEventConfiguration(
            Name: name, ServiceId: serviceId, Parameters: parameters,
            DataKey: dataKey, PassOnError: passOnError, Condition: condition);

        var steps = new List<FeatureEventConfiguration>(Steps ?? []);
        if (position.HasValue) steps.Insert(position.Value, step);
        else steps.Add(step);

        Mutate(s => s with { Steps = steps });
        return step;
    }

    public FeatureEventConfiguration? RemoveStep(int position)
    {
        var steps = new List<FeatureEventConfiguration>(Steps ?? []);
        if (position < 0 || position >= steps.Count) return null;

        var removed = steps[position];
        steps.RemoveAt(position);
        Mutate(s => s with { Steps = steps });
        return removed;
    }

    public FeatureEventConfiguration? ReorderStep(int currentPosition, int newPosition)
    {
        var steps = new List<FeatureEventConfiguration>(Steps ?? []);
        if (currentPosition < 0 || currentPosition >= steps.Count) return null;

        var step = steps[currentPosition];
        steps.RemoveAt(currentPosition);
        newPosition = Math.Clamp(newPosition, 0, steps.Count);
        steps.Insert(newPosition, step);

        Mutate(s => s with { Steps = steps });
        return step;
    }

    /// <summary>
    /// Get the feature step at the given position, or null if out of range.
    /// Delegates to the domain record.
    /// </summary>
    public FeatureEventConfiguration? GetStep(int position)
        => State.GetStep(position);
}
