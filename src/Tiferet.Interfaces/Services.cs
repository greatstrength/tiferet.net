using Tiferet.Domain;
using Tiferet.Mappers;

namespace Tiferet.Interfaces;

/// <summary>Service interface for managing app interface configurations.</summary>
public interface IAppService : IRepository<AppInterfaceAggregate> { }

/// <summary>Service interface for managing feature workflow configurations.</summary>
public interface IFeatureService : IRepository<FeatureAggregate> { }

/// <summary>Service interface for managing error definitions.</summary>
public interface IErrorService : IRepository<ErrorAggregate> { }

/// <summary>Service interface for managing CLI command definitions.</summary>
public interface ICliService : IRepository<CliCommandAggregate>
{
    /// <summary>Get all parent-level CLI arguments.</summary>
    IReadOnlyList<CliArgument> GetParentArguments();
}

/// <summary>Service interface for managing DI service configurations.</summary>
public interface IDIService : IService
{
    /// <summary>Check if a service configuration exists by ID.</summary>
    bool ConfigurationExists(string id);

    /// <summary>Retrieve a service configuration by ID.</summary>
    ServiceConfigurationAggregate? GetConfiguration(string id);

    /// <summary>List all service configurations and constants.</summary>
    (IReadOnlyList<ServiceConfigurationAggregate> Configurations, Dictionary<string, string> Constants) ListAll();

    /// <summary>Save or update a service configuration.</summary>
    void SaveConfiguration(ServiceConfigurationAggregate configuration);

    /// <summary>Delete a service configuration by ID (idempotent).</summary>
    void DeleteConfiguration(string id);

    /// <summary>Save or update constants.</summary>
    void SaveConstants(Dictionary<string, string> constants);
}

/// <summary>Service interface for loading and saving structured configuration data.</summary>
public interface IConfigurationService : IService
{
    /// <summary>Load configuration data.</summary>
    T Load<T>(Func<object, object>? startNode = null, Func<object, T>? dataFactory = null);

    /// <summary>Save configuration data.</summary>
    void Save<T>(T data, string? dataPath = null);
}

/// <summary>Service interface for file stream operations.</summary>
public interface IFileService : IService, IDisposable
{
}

/// <summary>Service interface for SQLite database operations.</summary>
public interface ISqliteService : IService, IDisposable
{
    /// <summary>Execute a SQL command.</summary>
    int Execute(string sql, object?[]? parameters = null);

    /// <summary>Fetch a single row.</summary>
    Dictionary<string, object?>? FetchOne(string sql, object?[]? parameters = null);

    /// <summary>Fetch all rows.</summary>
    IReadOnlyList<Dictionary<string, object?>> FetchAll(string sql, object?[]? parameters = null);
}

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
