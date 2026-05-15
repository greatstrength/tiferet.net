namespace Tiferet.Domain.App;

/// <summary>
/// The base application interface object.
/// </summary>
/// <param name="Id">The unique identifier for the application interface.</param>
/// <param name="Name">The name of the application interface.</param>
/// <param name="AssemblyName">The assembly name for the application context type.</param>
/// <param name="TypeName">The fully-qualified type name for the application context.</param>
/// <param name="Description">The description of the application interface.</param>
/// <param name="LoggerId">The logger ID for the application instance.</param>
/// <param name="Flags">The flags for the application interface.</param>
/// <param name="Services">The application instance service dependencies.</param>
/// <param name="Constants">The application dependency constants.</param>
public sealed record AppInterfaceConfiguration(
    string Id,
    string Name,
    string AssemblyName,
    string TypeName,
    string? Description = null,
    string LoggerId = "default",
    IReadOnlyList<string>? Flags = null,
    IReadOnlyList<AppServiceDependencyConfiguration>? Services = null,
    IReadOnlyDictionary<string, string>? Constants = null) : DomainObject
{
    /// <summary>
    /// Get the service dependency by service ID.
    /// </summary>
    /// <param name="serviceId">The service ID to look up.</param>
    /// <returns>The matching service dependency, or null.</returns>
    public AppServiceDependencyConfiguration? GetService(string serviceId)
    {
        if (Services is null) return null;
        foreach (var dep in Services)
        {
            if (dep.ServiceId == serviceId)
                return dep;
        }
        return null;
    }
}
