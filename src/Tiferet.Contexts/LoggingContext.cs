using Microsoft.Extensions.Logging;
using Tiferet.Core;
using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Events.Logging;
using Tiferet.Mappers;

namespace Tiferet.Contexts;

/// <summary>
/// Context for building loggers from stored logging configurations.
/// Uses <see cref="ILoggerFactory"/> for idiomatic .NET logging.
/// Implements <see cref="IDisposable"/> to release owned factory resources.
/// </summary>
public class LoggingContext : IDisposable
{
    private readonly ListAllLoggingConfigs _listAllEvent;
    private readonly string _loggerId;

    // Default factory: owned by us when created internally, otherwise caller-owned.
    private readonly ILoggerFactory _defaultFactory;
    private readonly bool _ownsDefaultFactory;

    // Per-config factory: created lazily on first BuildLogger() call when a
    // matching logger config exists. Disposed in Dispose().
    private ILoggerFactory? _configuredFactory;

    private bool _disposed;

    /// <summary>
    /// Initializes the logging context.
    /// </summary>
    /// <param name="listAllLoggingConfigsEvent">The event to list all logging configurations.</param>
    /// <param name="loggerId">The logger category name to create.</param>
    /// <param name="loggerFactory">Optional logger factory (defaults to a console-based factory).</param>
    public LoggingContext(
        ListAllLoggingConfigs listAllLoggingConfigsEvent,
        string loggerId,
        ILoggerFactory? loggerFactory = null)
    {
        _listAllEvent = listAllLoggingConfigsEvent;
        _loggerId = loggerId;
        _ownsDefaultFactory = loggerFactory is null;
        _defaultFactory = loggerFactory ?? LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });
    }

    /// <summary>
    /// Build a logger for the configured logger ID.
    /// The per-config factory is created once on the first call and reused.
    /// Falls back to the default factory if no matching config exists.
    /// </summary>
    /// <returns>The configured logger instance.</returns>
    public ILogger BuildLogger()
    {
        // Return from the already-created configured factory if available.
        if (_configuredFactory is not null)
            return _configuredFactory.CreateLogger(_loggerId);

        try
        {
            // Load all logging configurations.
            var (_, _, loggers) = _listAllEvent.Execute(new ListAllLoggingConfigsParams());

            // Find the matching logger config for our ID.
            var loggerConfig = loggers.FirstOrDefault(l => l.Domain.Id == _loggerId);

            // If a matching config exists, create and cache a factory for its level.
            if (loggerConfig is not null)
            {
                _configuredFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(loggerConfig.Domain.Level);
                });
                return _configuredFactory.CreateLogger(_loggerId);
            }
        }
        catch (Exception)
        {
            // Fall through to default logger on any configuration error.
        }

        // Return a default logger.
        return _defaultFactory.CreateLogger(_loggerId);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        // Dispose the per-config factory if it was created.
        _configuredFactory?.Dispose();

        // Dispose the default factory only if we created it.
        if (_ownsDefaultFactory)
            _defaultFactory.Dispose();

        _disposed = true;
    }
}
