using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Interfaces;
using Tiferet.Mappers;
using Tiferet.Utilities;

namespace Tiferet.Repositories;

/// <summary>
/// Generic SQLite-backed repository implementing <see cref="IRepository{TAggregate}"/>.
/// Wraps <see cref="SqliteClient"/> for connection lifecycle and delegates
/// domain-specific row ↔ aggregate conversion to abstract hooks,
/// mirroring the template-method pattern of <see cref="YamlRepository{TAggregate}"/>
/// and <see cref="HttpRepository{TAggregate}"/>.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type managed by this repository.</typeparam>
public abstract class SqliteRepository<TAggregate> : IRepository<TAggregate>, IDisposable
    where TAggregate : Aggregate
{
    private SqliteClient? _client;
    private bool _initialized;

    /// <summary>The database file path, or <c>":memory:"</c>.</summary>
    protected string DatabasePath { get; }

    /// <summary>The table name for CRUD operations.</summary>
    protected string TableName { get; }

    /// <summary>The SQLite connection mode (default: <c>"rwc"</c>).</summary>
    protected string Mode { get; }

    /// <summary>
    /// The column name used as the primary key for lookups.
    /// Default: <c>"id"</c>. Override for custom primary key columns.
    /// </summary>
    protected virtual string IdColumnName => "id";

    /// <summary>
    /// Initializes the SQLite repository.
    /// </summary>
    /// <param name="path">Database file path or <c>":memory:"</c>.</param>
    /// <param name="tableName">The table name for CRUD operations.</param>
    /// <param name="mode">SQLite connection mode (default: <c>"rwc"</c>).</param>
    protected SqliteRepository(string path, string tableName, string mode = "rwc")
    {
        DatabasePath = path;
        TableName = tableName;
        Mode = mode;
    }

    // --- Extensibility hooks ---

    /// <summary>
    /// Convert a database row (column-name → value dictionary) to an aggregate instance.
    /// Each subclass must provide domain-specific deserialization.
    /// </summary>
    /// <param name="row">The row data as a string-keyed dictionary.</param>
    /// <returns>The hydrated aggregate.</returns>
    protected abstract TAggregate Hydrate(Dictionary<string, object?> row);

    /// <summary>
    /// Convert an aggregate into a column-name → value dictionary for persistence.
    /// Each subclass must provide domain-specific serialization.
    /// </summary>
    /// <param name="entity">The aggregate to dehydrate.</param>
    /// <returns>A dictionary of column names to values.</returns>
    protected abstract Dictionary<string, object?> Dehydrate(TAggregate entity);

    /// <summary>
    /// Extract the entity ID from an aggregate.
    /// </summary>
    /// <param name="entity">The aggregate.</param>
    /// <returns>The entity identifier string.</returns>
    protected abstract string GetEntityId(TAggregate entity);

    /// <summary>
    /// Called once on first connection to allow subclasses to create tables
    /// or perform schema initialization. Default is a no-op.
    /// </summary>
    /// <param name="client">The connected SQLite client.</param>
    protected virtual void EnsureTable(SqliteClient client) { }

    // --- Connection management ---

    /// <summary>
    /// Get the SQLite client, opening the connection and initializing
    /// the table on first access.
    /// </summary>
    /// <returns>The connected SQLite client.</returns>
    protected SqliteClient EnsureConnection()
    {
        if (_client is not null && _initialized)
            return _client;

        _client ??= new SqliteClient(DatabasePath, Mode);

        if (_client.Connection is null)
            _client.Open();

        if (!_initialized)
        {
            EnsureTable(_client);
            _initialized = true;
        }

        return _client;
    }

    // --- IRepository<TAggregate> implementation ---

    /// <inheritdoc/>
    public virtual bool Exists(string id)
    {
        var client = EnsureConnection();
        var row = client.FetchOne(
            $"SELECT 1 FROM {TableName} WHERE {IdColumnName} = @p0 LIMIT 1",
            [id]);
        return row is not null;
    }

    /// <inheritdoc/>
    public virtual TAggregate? Get(string id)
    {
        var client = EnsureConnection();
        var row = client.FetchOne(
            $"SELECT * FROM {TableName} WHERE {IdColumnName} = @p0",
            [id]);
        return row is null ? default : Hydrate(row);
    }

    /// <inheritdoc/>
    public virtual IReadOnlyList<TAggregate> List()
    {
        var client = EnsureConnection();
        var rows = client.FetchAll($"SELECT * FROM {TableName}");
        return rows.Select(Hydrate).ToList();
    }

    /// <inheritdoc/>
    public virtual void Save(TAggregate entity)
    {
        var client = EnsureConnection();
        var columns = Dehydrate(entity);

        // Build INSERT OR REPLACE statement.
        var columnNames = string.Join(", ", columns.Keys);
        var paramPlaceholders = string.Join(", ", columns.Keys.Select((_, i) => $"@p{i}"));
        var values = columns.Values.ToArray();

        client.Execute(
            $"INSERT OR REPLACE INTO {TableName} ({columnNames}) VALUES ({paramPlaceholders})",
            values);
    }

    /// <inheritdoc/>
    public virtual void Delete(string id)
    {
        var client = EnsureConnection();
        client.Execute(
            $"DELETE FROM {TableName} WHERE {IdColumnName} = @p0",
            [id]);
    }

    /// <summary>
    /// Releases the SQLite connection.
    /// </summary>
    public void Dispose()
    {
        _client?.Dispose();
        _client = null;
        _initialized = false;
        GC.SuppressFinalize(this);
    }
}
