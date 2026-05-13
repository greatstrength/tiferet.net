namespace Tiferet.Domain;

/// <summary>
/// A flagged container dependency object.
/// </summary>
/// <param name="Flag">The flag for the container dependency.</param>
/// <param name="AssemblyName">The assembly name containing the dependency type.</param>
/// <param name="TypeName">The fully-qualified type name for the dependency.</param>
/// <param name="Parameters">The container dependency parameters.</param>
public sealed record FlaggedDependency(
    string Flag,
    string AssemblyName,
    string TypeName,
    IReadOnlyDictionary<string, string>? Parameters = null) : DomainObject;

/// <summary>
/// A service configuration that defines dependency injection behavior.
/// </summary>
/// <param name="Id">The unique identifier for the service configuration.</param>
/// <param name="Name">The name of the service configuration.</param>
/// <param name="AssemblyName">The default assembly name for the dependency.</param>
/// <param name="TypeName">The default type name for the dependency.</param>
/// <param name="Parameters">The default configuration parameters.</param>
/// <param name="Dependencies">The flag-specific implementation overrides.</param>
public sealed record ServiceConfiguration(
    string Id,
    string? Name = null,
    string? AssemblyName = null,
    string? TypeName = null,
    IReadOnlyDictionary<string, string>? Parameters = null,
    IReadOnlyList<FlaggedDependency>? Dependencies = null) : DomainObject
{
    /// <summary>
    /// Get the first flagged dependency matching any of the provided flags.
    /// Flags are assumed ordinal in priority.
    /// </summary>
    /// <param name="flags">The flags to match against.</param>
    /// <returns>The first matching dependency, or null.</returns>
    public FlaggedDependency? GetDependency(params string[] flags)
    {
        if (Dependencies is null) return null;

        foreach (var flag in flags)
        {
            foreach (var dep in Dependencies)
            {
                if (dep.Flag == flag)
                    return dep;
            }
        }

        return null;
    }

}
