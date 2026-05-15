namespace Tiferet.Domain.App;

/// <summary>
/// An app service dependency that defines a service configuration for an app interface.
/// </summary>
/// <param name="ServiceId">The service ID for the application dependency.</param>
/// <param name="AssemblyName">The assembly name containing the service type.</param>
/// <param name="TypeName">The fully-qualified type name for the service.</param>
/// <param name="Parameters">The parameters for the application dependency.</param>
public sealed record AppServiceDependencyConfiguration(
    string ServiceId,
    string AssemblyName,
    string TypeName,
    IReadOnlyDictionary<string, string>? Parameters = null) : DomainObject;
