using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>
/// Aggregate for mutable feature event operations.
/// </summary>
public class FeatureEventAggregate : Aggregate<FeatureEvent>
{
    public FeatureEventAggregate(FeatureEvent domain) : base(domain) { }

    /// <summary>Set the pass-on-error flag.</summary>
    public void SetPassOnError(bool value) => SetAttribute(nameof(FeatureEvent.PassOnError), value);

    /// <summary>
    /// Merge new parameters into the existing parameters.
    /// Keys with null values are removed.
    /// </summary>
    public void SetParameters(Dictionary<string, string>? parameters)
    {
        if (parameters is null) return;

        var merged = new Dictionary<string, string>(
            Domain.Parameters ?? new Dictionary<string, string>());
        foreach (var (key, value) in parameters)
            merged[key] = value;

        // Remove keys with null-equivalent values.
        var cleaned = merged
            .Where(kv => kv.Value is not null)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        SetAttribute(nameof(FeatureEvent.Parameters),
            (IReadOnlyDictionary<string, string>)cleaned);
    }
}

/// <summary>
/// Aggregate for mutable feature operations.
/// </summary>
public class FeatureAggregate : Aggregate<Feature>
{
    public FeatureAggregate(Feature domain) : base(domain) { }

    /// <summary>Update the display name of the feature.</summary>
    public void Rename(string name) => SetAttribute(nameof(Feature.Name), name);

    /// <summary>Update the feature description.</summary>
    public void SetDescription(string? description) =>
        SetAttribute(nameof(Feature.Description), description);

    /// <summary>
    /// Add a feature event step.
    /// </summary>
    public FeatureEvent AddStep(
        string name,
        string serviceId,
        IReadOnlyDictionary<string, string>? parameters = null,
        string? dataKey = null,
        bool passOnError = false,
        string? condition = null,
        int? position = null)
    {
        var step = new FeatureEvent(
            Name: name,
            ServiceId: serviceId,
            Parameters: parameters,
            DataKey: dataKey,
            PassOnError: passOnError,
            Condition: condition);

        var steps = new List<FeatureEvent>(Domain.Steps ?? []);
        if (position.HasValue)
            steps.Insert(position.Value, step);
        else
            steps.Add(step);

        SetAttribute(nameof(Feature.Steps), (IReadOnlyList<FeatureEvent>)steps);
        return step;
    }

    /// <summary>
    /// Remove and return the feature step at the given position.
    /// </summary>
    public FeatureEvent? RemoveStep(int position)
    {
        var steps = new List<FeatureEvent>(Domain.Steps ?? []);
        if (position < 0 || position >= steps.Count) return null;

        var removed = steps[position];
        steps.RemoveAt(position);
        SetAttribute(nameof(Feature.Steps), (IReadOnlyList<FeatureEvent>)steps);
        return removed;
    }

    /// <summary>
    /// Move a step from its current position to a new position.
    /// </summary>
    public FeatureEvent? ReorderStep(int currentPosition, int newPosition)
    {
        var steps = new List<FeatureEvent>(Domain.Steps ?? []);
        if (currentPosition < 0 || currentPosition >= steps.Count) return null;

        var step = steps[currentPosition];
        steps.RemoveAt(currentPosition);

        // Clamp new position.
        newPosition = Math.Clamp(newPosition, 0, steps.Count);
        steps.Insert(newPosition, step);

        SetAttribute(nameof(Feature.Steps), (IReadOnlyList<FeatureEvent>)steps);
        return step;
    }
}
