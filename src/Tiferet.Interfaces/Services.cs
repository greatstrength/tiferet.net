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
public interface ICliService : IRepository<CliCommandAggregate> { }

/// <summary>Service interface for managing DI service configurations.</summary>
public interface IDIService : IService
{
    /// <summary>List all service configurations and constants.</summary>
    (IReadOnlyList<ServiceConfigurationAggregate> Configurations, Dictionary<string, string> Constants) ListAll();
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
}
