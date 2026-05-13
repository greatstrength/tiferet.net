using Microsoft.Data.Sqlite;

using Tiferet.Core;
using Tiferet.Interfaces;

namespace Tiferet.Utilities;

/// <summary>
/// SQLite database client with connection management and structured error handling.
/// Implements <see cref="ISqliteService"/> as a standalone utility (no FileLoader inheritance).
/// </summary>
public class SqliteClient : ISqliteService
{
    private SqliteConnection? _connection;

    /// <summary>The database file path, or <c>":memory:"</c>.</summary>
    public string FilePath { get; }

    /// <summary>The SQLite open mode (<c>"ro"</c>, <c>"rw"</c>, or <c>"rwc"</c>).</summary>
    public string Mode { get; }

    /// <summary>The underlying connection, or <c>null</c> if not open.</summary>
    public SqliteConnection? Connection => _connection;

    /// <summary>
    /// Initializes a new <see cref="SqliteClient"/>.
    /// </summary>
    /// <param name="path">Database path or <c>":memory:"</c>.</param>
    /// <param name="mode">SQLite connection mode: <c>"ro"</c>, <c>"rw"</c>, or <c>"rwc"</c>.</param>
    public SqliteClient(string path = ":memory:", string mode = "rw")
    {
        FilePath = path ?? throw new ArgumentNullException(nameof(path));
        Mode = mode;
        VerifyMode(mode);
    }

    /// <summary>
    /// Validate the SQLite mode string.
    /// </summary>
    private static void VerifyMode(string mode)
    {
        if (mode is not ("ro" or "rw" or "rwc"))
            throw new TiferetException(ErrorCodes.SqliteInvalidMode, null, ("mode", mode));
    }

    /// <summary>
    /// Build the connection string for the configured path and mode.
    /// </summary>
    private string BuildConnectionString()
    {
        if (FilePath == ":memory:")
            return "Data Source=:memory:";

        var sqliteMode = Mode switch
        {
            "ro" => SqliteOpenMode.ReadOnly,
            "rw" => SqliteOpenMode.ReadWrite,
            "rwc" => SqliteOpenMode.ReadWriteCreate,
            _ => throw new TiferetException(ErrorCodes.SqliteInvalidMode, null, ("mode", Mode)),
        };

        return new SqliteConnectionStringBuilder
        {
            DataSource = FilePath,
            Mode = sqliteMode,
        }.ConnectionString;
    }

    /// <summary>
    /// Open the database connection.
    /// </summary>
    public void Open()
    {
        if (_connection != null)
            throw new TiferetException(ErrorCodes.SqliteConnAlreadyOpen, null, ("path", FilePath));

        try
        {
            _connection = new SqliteConnection(BuildConnectionString());
            _connection.Open();
        }
        catch (SqliteException ex)
        {
            _connection?.Dispose();
            _connection = null;
            throw new TiferetException(
                ErrorCodes.SqliteConnFailed, null,
                ("originalError", ex.Message), ("path", FilePath));
        }
    }

    /// <summary>
    /// Close the database connection.
    /// </summary>
    public void Close()
    {
        _connection?.Dispose();
        _connection = null;
    }

    /// <summary>
    /// Ensure the connection is open, throwing if not initialized.
    /// </summary>
    private SqliteConnection EnsureConnection()
    {
        return _connection
            ?? throw new TiferetException(ErrorCodes.SqliteConnNotInitialized);
    }

    /// <summary>
    /// Create a command with optional positional parameters.
    /// </summary>
    private SqliteCommand CreateCommand(string sql, object?[]? parameters)
    {
        var conn = EnsureConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        if (parameters != null)
        {
            for (var i = 0; i < parameters.Length; i++)
                cmd.Parameters.AddWithValue($"@p{i}", parameters[i] ?? DBNull.Value);
        }

        return cmd;
    }

    /// <inheritdoc/>
    public int Execute(string sql, object?[]? parameters = null)
    {
        using var cmd = CreateCommand(sql, parameters);
        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Execute a SQL command with an iterable of parameter arrays.
    /// </summary>
    /// <param name="sql">The SQL statement.</param>
    /// <param name="parameterSets">Sequence of parameter arrays.</param>
    /// <returns>Total rows affected.</returns>
    public int ExecuteMany(string sql, IEnumerable<object?[]> parameterSets)
    {
        var total = 0;
        foreach (var parameters in parameterSets)
            total += Execute(sql, parameters);
        return total;
    }

    /// <summary>
    /// Execute multiple SQL statements from a script string.
    /// </summary>
    /// <param name="script">The SQL script.</param>
    public void ExecuteScript(string script)
    {
        var conn = EnsureConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = script;
        cmd.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public Dictionary<string, object?>? FetchOne(string sql, object?[]? parameters = null)
    {
        using var cmd = CreateCommand(sql, parameters);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        var row = new Dictionary<string, object?>();
        for (var i = 0; i < reader.FieldCount; i++)
            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        return row;
    }

    /// <inheritdoc/>
    public IReadOnlyList<Dictionary<string, object?>> FetchAll(string sql, object?[]? parameters = null)
    {
        using var cmd = CreateCommand(sql, parameters);
        using var reader = cmd.ExecuteReader();
        var rows = new List<Dictionary<string, object?>>();
        while (reader.Read())
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }
        return rows;
    }

    /// <summary>Commit the current transaction.</summary>
    public void Commit()
    {
        var conn = EnsureConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "COMMIT";
        try { cmd.ExecuteNonQuery(); } catch (SqliteException) { /* no active transaction is fine */ }
    }

    /// <summary>Roll back the current transaction.</summary>
    public void Rollback()
    {
        var conn = EnsureConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "ROLLBACK";
        try { cmd.ExecuteNonQuery(); } catch (SqliteException) { /* no active transaction is fine */ }
    }

    /// <summary>
    /// Backup the database to a target file path.
    /// </summary>
    /// <param name="targetPath">The backup destination path.</param>
    public void Backup(string targetPath)
    {
        var source = EnsureConnection();
        try
        {
            using var target = new SqliteConnection($"Data Source={targetPath}");
            target.Open();
            source.BackupDatabase(target);
        }
        catch (SqliteException ex)
        {
            throw new TiferetException(
                ErrorCodes.SqliteBackupFailed, null,
                ("originalError", ex.Message), ("targetPath", targetPath));
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }
}
