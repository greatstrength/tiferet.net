namespace Tiferet.Interfaces;

/// <summary>Service interface for managing app interface configurations.</summary>
public interface IAppService : IRepository<object> { }

/// <summary>Service interface for managing feature workflow configurations.</summary>
public interface IFeatureService : IRepository<object> { }

/// <summary>Service interface for managing error definitions.</summary>
public interface IErrorService : IRepository<object> { }

/// <summary>Service interface for managing CLI command definitions.</summary>
public interface ICliService : IRepository<object> { }

/// <summary>Service interface for managing DI service configurations.</summary>
public interface IDIService : IService
{
    /// <summary>List all service configurations and constants.</summary>
    (IReadOnlyList<object> Configurations, Dictionary<string, object> Constants) ListAll();
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
    /// <summary>Open the file stream.</summary>
    void Open();

    /// <summary>Close the file stream.</summary>
    void Close();
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
    (IReadOnlyList<object> Formatters, IReadOnlyList<object> Handlers, IReadOnlyList<object> Loggers) ListAll();
}
