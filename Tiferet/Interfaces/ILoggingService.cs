using Tiferet.Mappers.Logging;

namespace Tiferet.Interfaces;

/// <summary>Service interface for logging configuration.</summary>
public interface ILoggingService : IService
{
    /// <summary>List all logging configurations.</summary>
    (IReadOnlyList<FormatterAggregate> Formatters, IReadOnlyList<HandlerAggregate> Handlers, IReadOnlyList<LoggerAggregate> Loggers) ListAll();

    /// <summary>Save a formatter configuration.</summary>
    void SaveFormatter(FormatterAggregate formatter);

    /// <summary>Save a handler configuration.</summary>
    void SaveHandler(HandlerAggregate handler);

    /// <summary>Save a logger configuration.</summary>
    void SaveLogger(LoggerAggregate logger);

    /// <summary>Delete a formatter by ID (idempotent).</summary>
    void DeleteFormatter(string id);

    /// <summary>Delete a handler by ID (idempotent).</summary>
    void DeleteHandler(string id);

    /// <summary>Delete a logger by ID (idempotent).</summary>
    void DeleteLogger(string id);
}
