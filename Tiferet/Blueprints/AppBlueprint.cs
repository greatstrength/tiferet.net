using System.Reflection;
using Tiferet.Events;
using Tiferet.Contexts;
using Tiferet.Domain;
using Tiferet.Domain.App;
using Tiferet.Domain.Cli;
using Tiferet.Domain.Feature;
using Tiferet.Events.App;
using Tiferet.Events.DI;
using Tiferet.Events.Error;
using Tiferet.Events.Feature;
using Tiferet.Events.Logging;
using Tiferet.Interfaces;
using Tiferet.Mappers;
using Tiferet.Mappers.App;
using Tiferet.Mappers.Cli;
using Tiferet.Repositories;

namespace Tiferet.Blueprints;

/// <summary>
/// Static blueprint for one-step application bootstrapping.
/// Resolves an <see cref="AppInterfaceConfiguration"/> configuration, wires all framework
/// contexts, and returns a ready-to-use <see cref="AppInterfaceContext"/>.
/// </summary>
public static class AppBlueprint
{
    /// <summary>Default configuration directory (matches Python Tiferet convention).</summary>
    public const string DefaultConfigDir = "app/configs";

    // Default config file names.
    private const string AppConfigFile = "app.yml";
    private const string ContainerConfigFile = "container.yml";
    private const string FeatureConfigFile = "feature.yml";
    private const string ErrorConfigFile = "error.yml";
    private const string LoggingConfigFile = "logging.yml";

    /// <summary>
    /// Build a fully wired <see cref="AppInterfaceContext"/> from YAML configuration.
    /// </summary>
    /// <param name="interfaceId">The interface identifier to load from <c>app.yml</c>.</param>
    /// <param name="configDir">The configuration directory (default: <c>app/configs</c>).</param>
    /// <returns>A ready-to-use <see cref="AppInterfaceContext"/>.</returns>
    public static AppInterfaceContext BuildApp(
        string interfaceId,
        string configDir = DefaultConfigDir)
    {
        // 1. Load the app interface definition from app.yml.
        var appRepo = new AppYamlRepository(Path.Combine(configDir, AppConfigFile));
        var getAppEvent = new GetAppInterface(appRepo);
        var appInterface = getAppEvent.Execute(new GetAppInterfaceParams(interfaceId));

        // 2. Resolve service implementations (overrides from Services, or defaults).
        var featureService = ResolveService<IFeatureService>(appInterface, "feature_service")
            ?? new FeatureYamlRepository(Path.Combine(configDir, FeatureConfigFile));

        var errorService = ResolveService<IErrorService>(appInterface, "error_service")
            ?? new ErrorYamlRepository(Path.Combine(configDir, ErrorConfigFile));

        var diService = ResolveService<IDIService>(appInterface, "di_service")
            ?? new DIYamlRepository(Path.Combine(configDir, ContainerConfigFile));

        var loggingService = ResolveService<ILoggingService>(appInterface, "logging_service")
            ?? new LoggingYamlRepository(Path.Combine(configDir, LoggingConfigFile));

        // 3. Create domain events with injected services.
        var getFeatureEvent = new GetFeature(featureService);
        var getErrorEvent = new GetError(errorService);
        var listAllSettingsEvent = new ListAllSettings(diService);
        var listAllLoggingConfigsEvent = new ListAllLoggingConfigs(loggingService);

        // 4. Create and wire contexts.
        var cache = new CacheContext();
        var errorContext = new ErrorContext(getErrorEvent);

        // Use the configured LoggerId if set, otherwise fall back to the interface ID.
        var loggerId = appInterface.LoggerId == "default"
            ? interfaceId
            : appInterface.LoggerId;
        var loggingContext = new LoggingContext(listAllLoggingConfigsEvent, loggerId);
        var diContext = new DIContext(listAllSettingsEvent, cache);
        var featureContext = new FeatureContext(getFeatureEvent, diContext, cache);

        // 5. Return the assembled application interface context.
        return new AppInterfaceContext(interfaceId, featureContext, errorContext, loggingContext);
    }

    /// <summary>
    /// Attempt to resolve a custom service implementation from the app interface's
    /// <see cref="AppInterfaceConfiguration.Services"/> list. If the interface defines a service
    /// with the given <paramref name="serviceId"/>, the type is imported and
    /// instantiated with any configured parameters.
    /// </summary>
    /// <typeparam name="T">The expected service interface type.</typeparam>
    /// <param name="appInterface">The app interface aggregate.</param>
    /// <param name="serviceId">The service ID to look up.</param>
    /// <returns>The instantiated service, or null if no override is defined.</returns>
    private static T? ResolveService<T>(AppInterfaceAggregate appInterface, string serviceId)
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
