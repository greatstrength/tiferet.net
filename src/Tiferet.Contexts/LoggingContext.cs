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
/// </summary>
public class LoggingContext
{
    private readonly ListAllLoggingConfigs _listAllEvent;
    private readonly string _loggerId;
    private readonly ILoggerFactory _loggerFactory;

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
        _loggerFactory = loggerFactory ?? LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });
    }

    /// <summary>
    /// Build a logger for the configured logger ID.
    /// Loads configurations from the event; if none exist, returns a default logger.
    /// </summary>
    /// <returns>The configured logger instance.</returns>
    public ILogger BuildLogger()
    {
        try
        {
            // Load all logging configurations.
            var (formatters, handlers, loggers) =
                _listAllEvent.Execute(new ListAllLoggingConfigsParams());

            // Find the matching logger config for our ID.
            LoggerAggregate? loggerConfig = null;
            foreach (var l in loggers)
            {
                if (l.Domain.Id == _loggerId)
                {
                    loggerConfig = l;
                    break;
                }
            }

            // If a matching config exists, use its level for the factory.
            if (loggerConfig is not null)
            {
                return LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(loggerConfig.Domain.Level);
                }).CreateLogger(_loggerId);
            }
        }
        catch (Exception)
        {
            // Fall through to default logger on any configuration error.
        }

        // Return a default logger.
        return _loggerFactory.CreateLogger(_loggerId);
    }
}
