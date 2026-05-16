using Tiferet.Assets;
using Tiferet.Contexts;
using Tiferet.Events.App;
using Tiferet.Events.DI;
using Tiferet.Events.Error;
using Tiferet.Events.Feature;
using Tiferet.Events.Logging;

namespace Tiferet.Blueprints;

/// <summary>
/// Static blueprint for one-step application bootstrapping.
/// Delegates pre-DI service resolution to <see cref="BootstrapAppConfiguration"/>,
/// then wires domain events and contexts into a ready-to-use
/// <see cref="AppInterfaceContext"/>.
/// </summary>
public static class AppBlueprint
{
    /// <summary>
    /// Build a fully wired <see cref="AppInterfaceContext"/> from YAML configuration.
    /// </summary>
    /// <param name="interfaceId">The interface identifier to load from the config file.</param>
    /// <param name="configDir">The configuration directory (default: <c>app/assets</c>).</param>
    /// <returns>A ready-to-use <see cref="AppInterfaceContext"/>.</returns>
    public static AppInterfaceContext BuildApp(
        string interfaceId,
        string configDir = ConfigurationDefaults.DefaultConfigDir)
    {
        // 1. Bootstrap pre-DI services via the BootstrapAppConfiguration event.
        var configFile = Path.Combine(configDir, ConfigurationDefaults.ConfigFile);
        var bootstrap = new BootstrapAppConfiguration();
        var result = bootstrap.Execute(
            new BootstrapAppConfigurationParams(interfaceId, configFile));

        // 2. Create domain events with the resolved services.
        var getFeatureEvent = new GetFeature(result.FeatureService);
        var getErrorEvent = new GetError(result.ErrorService);
        var listAllSettingsEvent = new ListAllSettings(result.DIService);
        var listAllLoggingConfigsEvent = new ListAllLoggingConfigs(result.LoggingService);

        // 3. Create and wire contexts.
        var cache = new CacheContext();
        var errorContext = new ErrorContext(getErrorEvent);

        // Use the configured LoggerId if set, otherwise fall back to the interface ID.
        var loggerId = result.AppInterface.LoggerId == "default"
            ? interfaceId
            : result.AppInterface.LoggerId;
        var loggingContext = new LoggingContext(listAllLoggingConfigsEvent, loggerId);
        var diContext = new DIContext(listAllSettingsEvent, cache);
        var featureContext = new FeatureContext(getFeatureEvent, diContext, cache);

        // 4. Return the assembled application interface context.
        return new AppInterfaceContext(interfaceId, featureContext, errorContext, loggingContext);
    }
}
