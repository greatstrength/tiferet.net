using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tiferet.Assets;
using Tiferet.Contexts;
using Tiferet.Events.App;
using Tiferet.Events.DI;
using Tiferet.Events.Error;
using Tiferet.Events.Feature;
using Tiferet.Events.Logging;
using Tiferet.Interfaces;

namespace Tiferet.Blueprints;

/// <summary>
/// Static blueprint for one-step application bootstrapping.
/// Delegates pre-DI service resolution to <see cref="BootstrapAppConfiguration"/>,
/// then wires domain events and contexts into a ready-to-use
/// <see cref="AppInterfaceContext"/>.
/// Acts as the mediator between <see cref="ConfigurationDefaults"/> (Assets)
/// and the Microsoft DI container — the service provider never interacts
/// with Assets directly.
/// </summary>
public static class AppBlueprint
{
    // *** methods

    // ** method: build_app (standalone)
    /// <summary>
    /// Build a fully wired <see cref="AppInterfaceContext"/> from YAML configuration.
    /// Standalone mode — no external DI container required.
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

    // ** method: build_app (from service provider)
    /// <summary>
    /// Extract a fully wired <see cref="AppInterfaceContext"/> from an existing
    /// <see cref="IServiceProvider"/> that was configured via
    /// <see cref="ConfigureServices"/>.
    /// </summary>
    /// <param name="provider">The service provider built from a configured <see cref="IServiceCollection"/>.</param>
    /// <returns>The resolved <see cref="AppInterfaceContext"/>.</returns>
    public static AppInterfaceContext BuildApp(IServiceProvider provider)
    {
        return provider.GetRequiredService<AppInterfaceContext>();
    }

    // ** method: configure_services
    /// <summary>
    /// Register all Tiferet framework services into an <see cref="IServiceCollection"/>.
    /// The blueprint reads <see cref="ConfigurationDefaults"/> from Assets and translates
    /// that knowledge into DI registrations — the service provider never touches Assets directly.
    /// </summary>
    /// <param name="services">The service collection to populate.</param>
    /// <param name="options">The bootstrap configuration options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureServices(
        IServiceCollection services,
        TiferetOptions options)
    {
        // 1. Bootstrap pre-DI services via the BootstrapAppConfiguration event.
        var configFile = Path.Combine(options.ConfigDir, options.ConfigFile);
        var bootstrap = new BootstrapAppConfiguration();
        var result = bootstrap.Execute(
            new BootstrapAppConfigurationParams(options.InterfaceId, configFile));

        // 2. Register the resolved repository services as singletons.
        services.AddSingleton<IFeatureService>(result.FeatureService);
        services.AddSingleton<IErrorService>(result.ErrorService);
        services.AddSingleton<IDIService>(result.DIService);
        services.AddSingleton<ILoggingService>(result.LoggingService);

        // 3. Register domain events as transient (wired to their repository dependencies).
        services.AddTransient(sp => new GetFeature(sp.GetRequiredService<IFeatureService>()));
        services.AddTransient(sp => new GetError(sp.GetRequiredService<IErrorService>()));
        services.AddTransient(sp => new ListAllSettings(sp.GetRequiredService<IDIService>()));
        services.AddTransient(sp => new ListAllLoggingConfigs(sp.GetRequiredService<ILoggingService>()));

        // 4. Register contexts with appropriate lifetimes.
        services.AddSingleton<CacheContext>();

        services.AddSingleton(sp => new ErrorContext(
            sp.GetRequiredService<GetError>()));

        // Resolve the logger ID from the app interface configuration.
        var loggerId = result.AppInterface.LoggerId == "default"
            ? options.InterfaceId
            : result.AppInterface.LoggerId;

        services.AddSingleton(sp =>
        {
            // Use the host's ILoggerFactory if registered, otherwise let LoggingContext create its own.
            var loggerFactory = sp.GetService<ILoggerFactory>();
            return new LoggingContext(
                sp.GetRequiredService<ListAllLoggingConfigs>(),
                loggerId,
                loggerFactory);
        });

        services.AddSingleton(sp => new DIContext(
            sp.GetRequiredService<ListAllSettings>(),
            sp.GetRequiredService<CacheContext>()));

        services.AddSingleton(sp => new FeatureContext(
            sp.GetRequiredService<GetFeature>(),
            sp.GetRequiredService<DIContext>(),
            sp.GetRequiredService<CacheContext>()));

        // 5. Register the top-level AppInterfaceContext.
        services.AddSingleton(sp => new AppInterfaceContext(
            options.InterfaceId,
            sp.GetRequiredService<FeatureContext>(),
            sp.GetRequiredService<ErrorContext>(),
            sp.GetRequiredService<LoggingContext>()));

        return services;
    }
}
