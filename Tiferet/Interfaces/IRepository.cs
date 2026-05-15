namespace Tiferet.Interfaces;

/// <summary>
/// Generic repository interface providing standard CRUD operations.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type managed by this repository.</typeparam>
public interface IRepository<TAggregate> : IService
{
    /// <summary>Check if an entity exists by ID.</summary>
    bool Exists(string id);

    /// <summary>Retrieve an entity by ID.</summary>
    TAggregate? Get(string id);

    /// <summary>List all entities.</summary>
    IReadOnlyList<TAggregate> List();

    /// <summary>Save or update an entity.</summary>
    void Save(TAggregate entity);

    /// <summary>Delete an entity by ID (idempotent).</summary>
    void Delete(string id);
}
