using Tiferet.Domain;
using Tiferet.Mappers;
using Tiferet.Repositories;
using Tiferet.Utilities;

namespace Tiferet.Tests.Repositories;

// *** Test domain types

public sealed record SqliteTestDomain(string Id, string Name, int Score) : DomainObject;

public record SqliteTestAggregate : Aggregate<SqliteTestDomain>
{
    public SqliteTestAggregate(SqliteTestDomain state) : base(state) { }
    public string Id => State.Id;
    public string Name => State.Name;
    public int Score => State.Score;
}

// *** Concrete test repository

public class TestSqliteRepository : SqliteRepository<SqliteTestAggregate>
{
    public TestSqliteRepository(string path = ":memory:")
        : base(path, "items") { }

    protected override SqliteTestAggregate Hydrate(Dictionary<string, object?> row)
    {
        return new SqliteTestAggregate(new SqliteTestDomain(
            Id: row["id"]?.ToString() ?? "",
            Name: row["name"]?.ToString() ?? "",
            Score: Convert.ToInt32(row["score"] ?? 0)));
    }

    protected override Dictionary<string, object?> Dehydrate(SqliteTestAggregate entity)
    {
        return new Dictionary<string, object?>
        {
            ["id"] = entity.Id,
            ["name"] = entity.Name,
            ["score"] = entity.Score
        };
    }

    protected override string GetEntityId(SqliteTestAggregate entity) => entity.Id;

    protected override void EnsureTable(SqliteClient client)
    {
        client.Execute("""
            CREATE TABLE IF NOT EXISTS items (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                score INTEGER NOT NULL DEFAULT 0
            )
            """);
    }
}

// *** Tests

public class SqliteRepositoryTests : IDisposable
{
    private readonly TestSqliteRepository _repo;

    public SqliteRepositoryTests()
    {
        _repo = new TestSqliteRepository();
    }

    public void Dispose() => _repo.Dispose();

    [Fact]
    public void Exists_ReturnsFalse_WhenEmpty()
    {
        Assert.False(_repo.Exists("missing"));
    }

    [Fact]
    public void Save_And_Exists_ReturnsTrue()
    {
        var entity = new SqliteTestAggregate(new SqliteTestDomain("1", "Alice", 100));
        _repo.Save(entity);

        Assert.True(_repo.Exists("1"));
    }

    [Fact]
    public void Save_And_Get_ReturnsEntity()
    {
        var entity = new SqliteTestAggregate(new SqliteTestDomain("1", "Alice", 100));
        _repo.Save(entity);

        var result = _repo.Get("1");
        Assert.NotNull(result);
        Assert.Equal("1", result!.Id);
        Assert.Equal("Alice", result.Name);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Get_ReturnsNull_WhenNotFound()
    {
        var result = _repo.Get("missing");
        Assert.Null(result);
    }

    [Fact]
    public void Save_Upserts_ExistingEntity()
    {
        var original = new SqliteTestAggregate(new SqliteTestDomain("1", "Alice", 100));
        _repo.Save(original);

        var updated = new SqliteTestAggregate(new SqliteTestDomain("1", "Alice Updated", 200));
        _repo.Save(updated);

        var result = _repo.Get("1");
        Assert.NotNull(result);
        Assert.Equal("Alice Updated", result!.Name);
        Assert.Equal(200, result.Score);
    }

    [Fact]
    public void List_ReturnsAllEntities()
    {
        _repo.Save(new SqliteTestAggregate(new SqliteTestDomain("1", "Alice", 100)));
        _repo.Save(new SqliteTestAggregate(new SqliteTestDomain("2", "Bob", 200)));
        _repo.Save(new SqliteTestAggregate(new SqliteTestDomain("3", "Charlie", 300)));

        var results = _repo.List();
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void List_ReturnsEmpty_WhenNoEntities()
    {
        var results = _repo.List();
        Assert.Empty(results);
    }

    [Fact]
    public void Delete_RemovesEntity()
    {
        _repo.Save(new SqliteTestAggregate(new SqliteTestDomain("1", "Alice", 100)));
        Assert.True(_repo.Exists("1"));

        _repo.Delete("1");
        Assert.False(_repo.Exists("1"));
    }

    [Fact]
    public void Delete_IsIdempotent_WhenNotFound()
    {
        // Should not throw.
        _repo.Delete("missing");
    }

    [Fact]
    public void MultipleOperations_MaintainConsistency()
    {
        // Save three items.
        _repo.Save(new SqliteTestAggregate(new SqliteTestDomain("1", "Alice", 100)));
        _repo.Save(new SqliteTestAggregate(new SqliteTestDomain("2", "Bob", 200)));
        _repo.Save(new SqliteTestAggregate(new SqliteTestDomain("3", "Charlie", 300)));
        Assert.Equal(3, _repo.List().Count);

        // Delete one.
        _repo.Delete("2");
        Assert.Equal(2, _repo.List().Count);
        Assert.Null(_repo.Get("2"));

        // Update one.
        _repo.Save(new SqliteTestAggregate(new SqliteTestDomain("1", "Alice V2", 999)));
        var alice = _repo.Get("1");
        Assert.Equal("Alice V2", alice!.Name);
        Assert.Equal(999, alice.Score);
    }
}
