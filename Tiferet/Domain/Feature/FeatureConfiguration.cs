using System.ComponentModel.DataAnnotations;

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
    [Required] string Id,
    [Required] string Name,
    [Required] string GroupId,
    [Required] string FeatureKey,
    IReadOnlyList<string>? Flags = null,
    string? Description = null,
    IReadOnlyList<FeatureEventConfiguration>? Steps = null,
    IReadOnlyDictionary<string, string>? LogParams = null) : DomainObject
{
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
