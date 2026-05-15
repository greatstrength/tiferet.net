namespace Tiferet.Domain.DI;

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
    IReadOnlyList<FlaggedDependencyConfiguration>? Dependencies = null) : DomainObject
{
    /// <summary>
    /// Get the first flagged dependency matching any of the provided flags.
    /// </summary>
    public FlaggedDependencyConfiguration? GetDependency(params string[] flags)
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
