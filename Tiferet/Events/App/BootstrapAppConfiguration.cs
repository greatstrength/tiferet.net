using System.Reflection;
using Tiferet.Assets;
using Tiferet.Domain;
using Tiferet.Domain.App;
using Tiferet.Interfaces;

namespace Tiferet.Events.App;

// *** events

// ** event: bootstrap_app_configuration

/// <summary>
/// Parameters for the <see cref="BootstrapAppConfiguration"/> event.
/// </summary>
/// <param name="InterfaceId">The interface identifier to load from the config file.</param>
/// <param name="ConfigFile">The path to the consolidated configuration file.</param>
public sealed record BootstrapAppConfigurationParams(
    string InterfaceId,
    string ConfigFile);

/// <summary>
/// Result of the <see cref="BootstrapAppConfiguration"/> event.
/// Contains the resolved app interface and all pre-DI framework services.
/// </summary>
/// <param name="AppInterface">The loaded app interface configuration.</param>
/// <param name="FeatureService">The resolved feature service.</param>
/// <param name="ErrorService">The resolved error service.</param>
/// <param name="DIService">The resolved DI service.</param>
/// <param name="LoggingService">The resolved logging service.</param>
public sealed record BootstrapAppConfigurationResult(
    AppInterfaceConfiguration AppInterface,
    IFeatureService FeatureService,
    IErrorService ErrorService,
    IDIService DIService,
    ILoggingService LoggingService);

/// <summary>
/// Pre-DI domain event that reflectively resolves all framework services
/// needed to bootstrap an application interface. Takes no injected dependencies —
/// all resolution is performed via <see cref="ImportDependency.Resolve"/> and
/// string constants from <see cref="ConfigurationDefaults"/>.
/// </summary>
public class BootstrapAppConfiguration
    : DomainEvent<BootstrapAppConfigurationParams, BootstrapAppConfigurationResult>
{
    // * method: execute
    /// <summary>
    /// Resolve all pre-DI framework services and load the app interface configuration.
    /// </summary>
    /// <param name="parameters">The bootstrap parameters.</param>
    /// <returns>The resolved services and app interface configuration.</returns>
    public override BootstrapAppConfigurationResult Execute(BootstrapAppConfigurationParams parameters)
    {
        var configFile = parameters.ConfigFile;

        // Resolve the app service and load the interface definition.
        var appService = ResolveDefaultService<IAppService>(
            ConfigurationDefaults.DefaultAppServiceType, configFile);
        var appInterface = appService.Get(parameters.InterfaceId);

        // Verify the app interface was found.
        Verify(appInterface is not null,
            ErrorCodes.AppInterfaceNotFound,
            $"App interface not found: {parameters.InterfaceId}",
            ("interfaceId", parameters.InterfaceId));

        var config = appInterface!.ToDomainObject();

        // Resolve each framework service — app interface overrides take precedence.
        var featureService = ResolveServiceOverride<IFeatureService>(config, "feature_service")
            ?? ResolveDefaultService<IFeatureService>(
                ConfigurationDefaults.DefaultFeatureServiceType, configFile);

        var errorService = ResolveServiceOverride<IErrorService>(config, "error_service")
            ?? ResolveDefaultService<IErrorService>(
                ConfigurationDefaults.DefaultErrorServiceType, configFile);

        var diService = ResolveServiceOverride<IDIService>(config, "di_service")
            ?? ResolveDefaultService<IDIService>(
                ConfigurationDefaults.DefaultDIServiceType, configFile);

        var loggingService = ResolveServiceOverride<ILoggingService>(config, "logging_service")
            ?? ResolveDefaultService<ILoggingService>(
                ConfigurationDefaults.DefaultLoggingServiceType, configFile);

        // Return the resolved result.
        return new BootstrapAppConfigurationResult(
            config, featureService, errorService, diService, loggingService);
    }

    // * method: resolve_default_service
    /// <summary>
    /// Reflectively instantiate a default service implementation using
    /// <see cref="ImportDependency.Resolve"/> and the config file path.
    /// The config file path is passed as the first constructor argument;
    /// remaining parameters are filled with their default values.
    /// </summary>
    /// <typeparam name="T">The expected service interface type.</typeparam>
    /// <param name="typeName">The fully-qualified type name to resolve.</param>
    /// <param name="configFile">The config file path to pass to the constructor.</param>
    /// <returns>The instantiated default service.</returns>
    private static T ResolveDefaultService<T>(string typeName, string configFile)
        where T : class
    {
        var type = ImportDependency.Resolve(ConfigurationDefaults.DefaultAssembly, typeName);

        // Find the best public constructor.
        var ctor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault()
            ?? throw new TiferetException(
                ErrorCodes.AppServiceImportFailed,
                $"No public constructor found for type {type.FullName}.",
                ("type", type.FullName ?? type.Name));

        // First parameter is the config file path; fill remaining with defaults.
        var ctorParams = ctor.GetParameters();
        var args = new object?[ctorParams.Length];
        args[0] = configFile;

        for (int i = 1; i < ctorParams.Length; i++)
        {
            args[i] = ctorParams[i].HasDefaultValue
                ? ctorParams[i].DefaultValue
                : ctorParams[i].ParameterType.IsValueType
                    ? Activator.CreateInstance(ctorParams[i].ParameterType)
                    : null;
        }

        return (T)ctor.Invoke(args);
    }

    // * method: resolve_service_override
    /// <summary>
    /// Attempt to resolve a custom service implementation from the app interface's
    /// <see cref="AppInterfaceConfiguration.Services"/> list. If the interface defines a
    /// service with the given <paramref name="serviceId"/>, the type is imported and
    /// instantiated with any configured parameters.
    /// </summary>
    /// <typeparam name="T">The expected service interface type.</typeparam>
    /// <param name="appInterface">The app interface configuration.</param>
    /// <param name="serviceId">The service ID to look up.</param>
    /// <returns>The instantiated service, or null if no override is defined.</returns>
    private static T? ResolveServiceOverride<T>(AppInterfaceConfiguration appInterface, string serviceId)
        where T : class
    {
        var dep = appInterface.GetService(serviceId);
        if (dep is null)
            return null;

        // Import the service type via assembly reflection.
        var type = ImportDependency.Resolve(dep.AssemblyName, dep.TypeName);

        // Verify the resolved type implements the expected interface.
        if (!typeof(T).IsAssignableFrom(type))
        {
            throw new TiferetException(
                ErrorCodes.AppServiceImportFailed,
                $"Service '{serviceId}' type {type.FullName} does not implement {typeof(T).Name}.",
                ("serviceId", serviceId),
                ("expectedType", typeof(T).Name),
                ("actualType", type.FullName ?? type.Name));
        }

        // Construct the service with parameter injection.
        if (dep.Parameters is not null && dep.Parameters.Count > 0)
            return (T)ConstructWithParameters(type, dep.Parameters);

        return (T)Activator.CreateInstance(type)!;
    }

    // * method: construct_with_parameters
    /// <summary>
    /// Construct a service instance using constructor parameter matching.
    /// Parameters are matched by name (case-insensitive) to constructor arguments,
    /// with environment variable resolution via <see cref="ParseParameter"/>.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="parameters">The parameter dictionary from configuration.</param>
    /// <returns>The constructed instance.</returns>
    private static object ConstructWithParameters(
        Type type, IReadOnlyDictionary<string, string> parameters)
    {
        // Find the best constructor — prefer the one with the most parameters.
        var ctor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (ctor is null)
        {
            throw new TiferetException(
                ErrorCodes.AppServiceImportFailed,
                $"No public constructor found for type {type.FullName}.",
                ("type", type.FullName ?? type.Name));
        }

        var ctorParams = ctor.GetParameters();
        var args = new object?[ctorParams.Length];

        for (int i = 0; i < ctorParams.Length; i++)
        {
            var param = ctorParams[i];
            var paramName = param.Name!;

            // Try exact match, then case-insensitive match against parameter dict.
            string? rawValue = null;
            if (parameters.TryGetValue(paramName, out var exact))
            {
                rawValue = exact;
            }
            else
            {
                var match = parameters.Keys.FirstOrDefault(k =>
                    string.Equals(k, paramName, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                    rawValue = parameters[match];
            }

            if (rawValue is not null)
            {
                // Resolve environment variables via ParseParameter.
                args[i] = ParseParameter.Parse(rawValue);
            }
            else if (param.HasDefaultValue)
            {
                args[i] = param.DefaultValue;
            }
            else
            {
                args[i] = param.ParameterType.IsValueType
                    ? Activator.CreateInstance(param.ParameterType)
                    : null;
            }
        }

        return ctor.Invoke(args);
    }
}
