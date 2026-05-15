using Tiferet.Assets;
using Tiferet.Events;
using Tiferet.Interfaces;
using Tiferet.Utilities;

namespace Tiferet.Tests.Utilities;

public class SqliteClientTests : IDisposable
{
    private readonly string _tempDir;

    public SqliteClientTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_sqlite_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        using var client = new SqliteClient(":memory:", "rw");
        Assert.Equal(":memory:", client.FilePath);
        Assert.Equal("rw", client.Mode);
    }

    [Fact]
    public void Constructor_ThrowsOnInvalidMode()
    {
        var ex = Assert.Throws<TiferetException>(
            () => new SqliteClient(":memory:", "invalid"));
        Assert.Equal(ErrorCodes.SqliteInvalidMode, ex.ErrorCode);
    }

    [Fact]
    public void Open_CreatesConnection()
    {
        using var client = new SqliteClient();
        client.Open();
        Assert.NotNull(client.Connection);
    }

    [Fact]
    public void Open_ThrowsWhenAlreadyOpen()
    {
        using var client = new SqliteClient();
        client.Open();
        var ex = Assert.Throws<TiferetException>(() => client.Open());
        Assert.Equal(ErrorCodes.SqliteConnAlreadyOpen, ex.ErrorCode);
    }

    [Fact]
    public void Close_DisposesConnection()
    {
        using var client = new SqliteClient();
        client.Open();
        client.Close();
        Assert.Null(client.Connection);
    }

    [Fact]
    public void Execute_BeforeOpen_ThrowsNotInitialized()
    {
        using var client = new SqliteClient();
        var ex = Assert.Throws<TiferetException>(
            () => client.Execute("SELECT 1"));
        Assert.Equal(ErrorCodes.SqliteConnNotInitialized, ex.ErrorCode);
    }

    [Fact]
    public void Execute_CreateAndInsert()
    {
        using var client = new SqliteClient();
        client.Open();
        client.Execute("CREATE TABLE t (id INTEGER, name TEXT)");
        var affected = client.Execute(
            "INSERT INTO t (id, name) VALUES (@p0, @p1)",
            [1, "Alice"]);
        Assert.Equal(1, affected);
    }

    [Fact]
    public void FetchOne_ReturnsRow()
    {
        using var client = new SqliteClient();
        client.Open();
        client.Execute("CREATE TABLE t (id INTEGER, name TEXT)");
        client.Execute("INSERT INTO t VALUES (@p0, @p1)", [1, "Alice"]);
        client.Execute("INSERT INTO t VALUES (@p0, @p1)", [2, "Bob"]);

        var row = client.FetchOne("SELECT * FROM t WHERE id = @p0", [1]);
        Assert.NotNull(row);
        Assert.Equal((long)1, row["id"]);
        Assert.Equal("Alice", row["name"]);
    }

    [Fact]
    public void FetchOne_ReturnsNullWhenNoRows()
    {
        using var client = new SqliteClient();
        client.Open();
        client.Execute("CREATE TABLE t (id INTEGER)");

        var row = client.FetchOne("SELECT * FROM t WHERE id = @p0", [999]);
        Assert.Null(row);
    }

    [Fact]
    public void FetchAll_ReturnsAllRows()
    {
        using var client = new SqliteClient();
        client.Open();
        client.Execute("CREATE TABLE t (id INTEGER, name TEXT)");
        client.Execute("INSERT INTO t VALUES (@p0, @p1)", [1, "Alice"]);
        client.Execute("INSERT INTO t VALUES (@p0, @p1)", [2, "Bob"]);

        var rows = client.FetchAll("SELECT * FROM t ORDER BY id");
        Assert.Equal(2, rows.Count);
        Assert.Equal("Alice", rows[0]["name"]);
        Assert.Equal("Bob", rows[1]["name"]);
    }

    [Fact]
    public void FetchAll_HandlesNullValues()
    {
        using var client = new SqliteClient();
        client.Open();
        client.Execute("CREATE TABLE t (id INTEGER, val TEXT)");
        client.Execute("INSERT INTO t VALUES (@p0, @p1)", [1, null]);

        var rows = client.FetchAll("SELECT * FROM t");
        Assert.Single(rows);
        Assert.Null(rows[0]["val"]);
    }

    [Fact]
    public void ExecuteMany_InsertsMultipleRows()
    {
        using var client = new SqliteClient();
        client.Open();
        client.Execute("CREATE TABLE t (id INTEGER)");
        var affected = client.ExecuteMany(
            "INSERT INTO t VALUES (@p0)",
            [[1], [2], [3]]);
        Assert.Equal(3, affected);

        var rows = client.FetchAll("SELECT * FROM t");
        Assert.Equal(3, rows.Count);
    }

    [Fact]
    public void ExecuteScript_RunsMultipleStatements()
    {
        using var client = new SqliteClient();
        client.Open();
        client.ExecuteScript("""
            CREATE TABLE t1 (id INTEGER);
            CREATE TABLE t2 (id INTEGER);
            INSERT INTO t1 VALUES (1);
            INSERT INTO t2 VALUES (2);
            """);

        Assert.Equal((long)1, client.FetchOne("SELECT * FROM t1")!["id"]);
        Assert.Equal((long)2, client.FetchOne("SELECT * FROM t2")!["id"]);
    }

    [Fact]
    public void Backup_CreatesBackupFile()
    {
        using var client = new SqliteClient();
        client.Open();
        client.Execute("CREATE TABLE t (id INTEGER)");
        client.Execute("INSERT INTO t VALUES (@p0)", [42]);

        var backupPath = Path.Combine(_tempDir, "backup.db");
        client.Backup(backupPath);

        // Verify the backup.
        using var verifier = new SqliteClient(backupPath, "ro");
        verifier.Open();
        var row = verifier.FetchOne("SELECT * FROM t");
        Assert.NotNull(row);
        Assert.Equal((long)42, row["id"]);
    }

    [Fact]
    public void ImplementsISqliteService()
    {
        using var client = new SqliteClient();
        Assert.IsAssignableFrom<ISqliteService>(client);
        Assert.IsAssignableFrom<IDisposable>(client);
    }

    [Fact]
    public void Dispose_ClosesConnection()
    {
        var client = new SqliteClient();
        client.Open();
        client.Dispose();
        Assert.Null(client.Connection);
    }

    [Fact]
    public void FileBased_ReadWriteCreate()
    {
        var dbPath = Path.Combine(_tempDir, "test.db");
        using var client = new SqliteClient(dbPath, "rwc");
        client.Open();
        client.Execute("CREATE TABLE t (v TEXT)");
        client.Execute("INSERT INTO t VALUES (@p0)", ["hello"]);

        var row = client.FetchOne("SELECT * FROM t");
        Assert.Equal("hello", row!["v"]);
    }
}
