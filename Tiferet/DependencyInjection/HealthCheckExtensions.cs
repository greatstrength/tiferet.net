using Microsoft.Extensions.DependencyInjection;
using Tiferet.Assets;
using Tiferet.Blueprints;

namespace Tiferet.DependencyInjection;

/// <summary>
/// Configuration options for Tiferet health checks.
/// </summary>
public class TiferetHealthCheckOptions
{
    /// <summary>
    /// Path to the SQLite database file for the SQLite health check.
    /// When <c>null</c>, the SQLite health check is not registered.
    /// </summary>
    public string? SqliteDatabasePath { get; set; }
}

/// <summary>
/// Extension methods for registering Tiferet health checks.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Add Tiferet health checks to the service collection.
    /// Registers a YAML config file health check by default.
    /// Optionally registers a SQLite health check if configured.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The Tiferet bootstrap options (used to resolve config file path).</param>
    /// <param name="configure">Optional callback to configure additional health checks.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTiferetHealthChecks(
        this IServiceCollection services,
        TiferetOptions options,
        Action<TiferetHealthCheckOptions>? configure = null)
    {
        var healthOptions = new TiferetHealthCheckOptions();
        configure?.Invoke(healthOptions);

        // Resolve the full config file path.
        var configFilePath = Path.Combine(options.ConfigDir, options.ConfigFile);

        // Register health checks.
        var builder = services.AddHealthChecks();

        // YAML config file health check (always registered).
        builder.Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
            "tiferet-config",
            _ => new YamlConfigHealthCheck(configFilePath),
            failureStatus: null,
            tags: ["tiferet", "config"]));

        // SQLite health check (registered when a database path is provided).
        if (healthOptions.SqliteDatabasePath is not null)
        {
            builder.Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
                "tiferet-sqlite",
                _ => new SqliteHealthCheck(healthOptions.SqliteDatabasePath),
                failureStatus: null,
                tags: ["tiferet", "sqlite"]));
        }

        return services;
    }
}
