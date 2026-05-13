namespace Tiferet.Domain;

/// <summary>
/// A base step in a feature workflow.
/// </summary>
/// <param name="Name">The name of the feature step.</param>
public record FeatureStep(string Name) : DomainObject;

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
public sealed record FeatureEvent(
    string Name,
    string ServiceId,
    IReadOnlyList<string>? Flags = null,
    IReadOnlyDictionary<string, string>? Parameters = null,
    string? DataKey = null,
    bool PassOnError = false,
    string? Condition = null) : FeatureStep(Name);

/// <summary>
/// A feature object representing a workflow configuration.
/// </summary>
/// <param name="Id">The unique identifier of the feature (GroupId.FeatureKey).</param>
/// <param name="Name">The name of the feature.</param>
/// <param name="GroupId">The context group identifier.</param>
/// <param name="FeatureKey">The key of the feature within its group.</param>
/// <param name="Flags">List of feature flags that activate this feature.</param>
/// <param name="Description">The description of the feature.</param>
/// <param name="Steps">The step workflow for the feature.</param>
/// <param name="LogParams">The parameters to log for the feature.</param>
public sealed record Feature(
    string Id,
    string Name,
    string GroupId,
    string FeatureKey,
    IReadOnlyList<string>? Flags = null,
    string? Description = null,
    IReadOnlyList<FeatureEvent>? Steps = null,
    IReadOnlyDictionary<string, string>? LogParams = null) : DomainObject
{
    /// <summary>
    /// Create a Feature with derivation logic for Id, GroupId, FeatureKey, and Description.
    /// Mirrors the Python <c>@model_validator(mode='before')</c> derivation.
    /// </summary>
    /// <param name="name">The feature name (required).</param>
    /// <param name="groupId">The group ID (optional if id is provided).</param>
    /// <param name="featureKey">The feature key (optional, derived from name if missing).</param>
    /// <param name="id">The full ID (optional, derived from groupId.featureKey).</param>
    /// <param name="description">The description (optional, defaults to name).</param>
    /// <param name="flags">Feature flags.</param>
    /// <param name="steps">The step workflow.</param>
    /// <param name="logParams">Logging parameters.</param>
    /// <returns>A fully-formed Feature record.</returns>
    public static Feature Create(
        string name,
        string? groupId = null,
        string? featureKey = null,
        string? id = null,
        string? description = null,
        IReadOnlyList<string>? flags = null,
        IReadOnlyList<FeatureEvent>? steps = null,
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

        return new Feature(
            Id: id!,
            Name: name,
            GroupId: groupId!,
            FeatureKey: featureKey,
            Flags: flags,
            Description: description,
            Steps: steps,
            LogParams: logParams);
    }

    /// <summary>
    /// Get the feature step at the given position, or null if out of range.
    /// </summary>
    /// <param name="position">The index of the step to retrieve.</param>
    /// <returns>The FeatureEvent at the position, or null.</returns>
    public FeatureEvent? GetStep(int position)
    {
        if (Steps is null || position < 0 || position >= Steps.Count)
            return null;
        return Steps[position];
    }
}
