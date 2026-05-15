namespace Tiferet.Domain.Feature;

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
public sealed record FeatureConfiguration(
    string Id,
    string Name,
    string GroupId,
    string FeatureKey,
    IReadOnlyList<string>? Flags = null,
    string? Description = null,
    IReadOnlyList<FeatureEventConfiguration>? Steps = null,
    IReadOnlyDictionary<string, string>? LogParams = null) : DomainObject
{
    /// <summary>
    /// Create a FeatureConfiguration with derivation logic for Id, GroupId, FeatureKey, and Description.
    /// </summary>
    public static FeatureConfiguration Create(
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

        return new FeatureConfiguration(
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
    public FeatureEventConfiguration? GetStep(int position)
    {
        if (Steps is null || position < 0 || position >= Steps.Count)
            return null;
        return Steps[position];
    }
}
