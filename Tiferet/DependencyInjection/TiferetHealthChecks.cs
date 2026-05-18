using Microsoft.Extensions.Diagnostics.HealthChecks;
using Tiferet.Utilities;

namespace Tiferet.DependencyInjection;

/// <summary>
/// Health check that verifies the YAML configuration file exists and is readable.
/// </summary>
public class YamlConfigHealthCheck : IHealthCheck
{
    private readonly string _configFilePath;

    /// <summary>
    /// Initializes the YAML config health check.
    /// </summary>
    /// <param name="configFilePath">Absolute or relative path to the YAML config file.</param>
    public YamlConfigHealthCheck(string configFilePath)
    {
        _configFilePath = configFilePath;
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Configuration file not found: {_configFilePath}"));
            }

            // Verify the file is readable by opening it briefly.
            using var stream = File.OpenRead(_configFilePath);
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Configuration file accessible: {_configFilePath}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Configuration file error: {ex.Message}",
                exception: ex));
        }
    }
}

/// <summary>
/// Health check that verifies SQLite database connectivity by executing <c>SELECT 1</c>.
/// </summary>
public class SqliteHealthCheck : IHealthCheck
{
    private readonly string _databasePath;

    /// <summary>
    /// Initializes the SQLite health check.
    /// </summary>
    /// <param name="databasePath">Path to the SQLite database file.</param>
    public SqliteHealthCheck(string databasePath)
    {
        _databasePath = databasePath;
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SqliteClient(_databasePath, "ro");
            client.Open();
            var result = client.FetchOne("SELECT 1 AS ok");

            if (result is not null)
            {
                return Task.FromResult(HealthCheckResult.Healthy(
                    $"SQLite database accessible: {_databasePath}"));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"SQLite query returned no result: {_databasePath}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"SQLite connectivity error: {ex.Message}",
                exception: ex));
        }
    }
}
