namespace Tiferet.Interfaces;

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
