using System.ComponentModel.DataAnnotations;

namespace Tiferet.Domain.DI;

/// <summary>
/// A flagged container dependency object.
/// </summary>
/// <param name="Flag">The flag for the container dependency.</param>
/// <param name="AssemblyName">The assembly name containing the dependency type.</param>
/// <param name="TypeName">The fully-qualified type name for the dependency.</param>
/// <param name="Parameters">The container dependency parameters.</param>
public sealed record FlaggedDependencyConfiguration(
    [Required] string Flag,
    [Required] string AssemblyName,
    [Required] string TypeName,
    IReadOnlyDictionary<string, string>? Parameters = null) : DomainObject;
