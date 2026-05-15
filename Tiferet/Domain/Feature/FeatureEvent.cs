namespace Tiferet.Domain.Feature;

/// <summary>
/// A feature event step that executes a domain event from the container.
/// </summary>
/// <param name="Name">The name of the feature event.</param>
/// <param name="ServiceId">The service configuration ID for the feature event.</param>
/// <param name="Flags">List of feature flags that activate this event.</param>
/// <param name="Parameters">The custom parameters for the feature event.</param>
/// <param name="DataKey">The data key to store the result in, or null.</param>
/// <param name="PassOnError">Whether to pass on the error if the event fails.</param>
/// <param name="Condition">Optional boolean expression for conditional execution.</param>
public sealed record FeatureEventConfiguration(
    string Name,
    string ServiceId,
    IReadOnlyList<string>? Flags = null,
    IReadOnlyDictionary<string, string>? Parameters = null,
    string? DataKey = null,
    bool PassOnError = false,
    string? Condition = null) : FeatureStepConfiguration(Name);
