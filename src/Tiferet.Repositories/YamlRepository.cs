using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers;
using Tiferet.Utilities;

namespace Tiferet.Repositories;

/// <summary>
/// Generic YAML-backed repository implementing <see cref="IRepository{TAggregate}"/>.
/// Handles flat-key CRUD by default. Subclasses override
/// <see cref="GetSectionPath"/> and <see cref="ReconstructId"/> for composite keys.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type (e.g., <see cref="ErrorAggregate"/>).</typeparam>
/// <typeparam name="TDomain">The domain record type (e.g., <see cref="Error"/>).</typeparam>
public abstract class YamlRepository<TAggregate, TDomain> : IRepository<TAggregate>
    where TAggregate : Aggregate<TDomain>
    where TDomain : DomainObject
{
    /// <summary>The path to the YAML configuration file.</summary>
    protected string YamlFile { get; }

    /// <summary>The top-level YAML key containing the entities (e.g., "errors", "interfaces").</summary>
    protected string SectionKey { get; }

    /// <summary>The file encoding.</summary>
    protected string Encoding { get; }

    /// <summary>
    /// Properties to exclude when dehydrating a domain record to YAML.
    /// Default: excludes "Id". Subclasses may override to exclude more.
    /// </summary>
    protected virtual HashSet<string> DehydrateExclude => new() { "Id" };

    /// <summary>
    /// Initializes the YAML repository.
    /// </summary>
    /// <param name="yamlFile">Path to the YAML config file.</param>
    /// <param name="sectionKey">The top-level YAML section key.</param>
    /// <param name="encoding">File encoding (default: utf-8).</param>
    protected YamlRepository(string yamlFile, string sectionKey, string encoding = "utf-8")
    {
        YamlFile = yamlFile;
        SectionKey = sectionKey;
        Encoding = encoding;
    }

    // --- Extensibility hooks ---

    /// <summary>
    /// Get the YAML key path segments for a given entity ID.
    /// Default: <c>[SectionKey, id]</c> (flat key).
    /// Override for composite keys (e.g., <c>[SectionKey, group, key]</c>).
    /// </summary>
    protected virtual string[] GetSectionPath(string id) => [SectionKey, id];

    /// <summary>
    /// Reconstruct a full entity ID from YAML key segments.
    /// Default: returns the last segment (flat key).
    /// Override for composite keys (e.g., <c>"group.key"</c>).
    /// </summary>
    protected virtual string ReconstructId(params string[] segments) => segments[^1];

    /// <summary>
    /// Hydrate an aggregate from a string-keyed data dictionary and entity ID.
    /// Default: injects Id, uses <see cref="DomainObject.FromDictionary{T}"/>, constructs aggregate.
    /// Subclasses override for domain-specific factory methods (e.g., Feature.Create).
    /// </summary>
    protected virtual TAggregate Hydrate(Dictionary<string, object> data, string id)
    {
        data["Id"] = id;
        var domain = DomainObject.FromDictionary<TDomain>(data);
        return (TAggregate)Activator.CreateInstance(typeof(TAggregate), domain)!;
    }

    /// <summary>
    /// Dehydrate an aggregate into a YAML-serializable dictionary.
    /// Default: reflects domain record properties, excludes <see cref="DehydrateExclude"/>.
    /// </summary>
    protected virtual Dictionary<object, object> Dehydrate(TAggregate entity)
    {
        return YamlHelper.DehydrateRecord(entity.Domain, DehydrateExclude);
    }

    // --- YAML I/O helpers ---

    /// <summary>Load the full YAML file as a raw YamlDotNet dictionary.</summary>
    protected Dictionary<object, object> LoadFull()
    {
        var loader = YamlLoader.ForReading(YamlFile);
        var result = loader.Load();
        return result as Dictionary<object, object> ?? new Dictionary<object, object>();
    }

    /// <summary>Write the full dictionary back to the YAML file.</summary>
    protected void SaveFull(Dictionary<object, object> data)
    {
        var loader = YamlLoader.ForWriting(YamlFile);
        loader.Save(data);
    }

    // --- IRepository<TAggregate> implementation ---

    /// <inheritdoc/>
    public virtual bool Exists(string id)
    {
        var path = GetSectionPath(id);
        var full = LoadFull();
        return YamlHelper.GetNestedValue(full, path) is not null;
    }

    /// <inheritdoc/>
    public virtual TAggregate? Get(string id)
    {
        var path = GetSectionPath(id);
        var full = LoadFull();
        var node = YamlHelper.GetNestedValue(full, path);
        if (node is null) return null;

        var data = YamlHelper.ToStringDict(node);
        return Hydrate(data, id);
    }

    /// <inheritdoc/>
    public virtual IReadOnlyList<TAggregate> List()
    {
        var full = LoadFull();
        var section = YamlHelper.GetSection(full, SectionKey);
        return ListFromSection(section);
    }

    /// <summary>
    /// Build aggregates from a section dictionary. Handles both flat and nested structures.
    /// Default: treats each key as a flat entity ID. Subclasses override for nested groups.
    /// </summary>
    protected virtual IReadOnlyList<TAggregate> ListFromSection(Dictionary<string, object> section)
    {
        var results = new List<TAggregate>();
        foreach (var (key, value) in section)
        {
            var data = YamlHelper.ToStringDict(value);
            var id = ReconstructId(key);
            results.Add(Hydrate(data, id));
        }
        return results;
    }

    /// <inheritdoc/>
    public virtual void Save(TAggregate entity)
    {
        var id = GetEntityId(entity);
        var path = GetSectionPath(id);
        var full = LoadFull();
        var dehydrated = Dehydrate(entity);
        YamlHelper.SetNestedValue(full, dehydrated, path);
        SaveFull(full);
    }

    /// <inheritdoc/>
    public virtual void Delete(string id)
    {
        var path = GetSectionPath(id);
        var full = LoadFull();
        YamlHelper.RemoveNestedValue(full, path);
        SaveFull(full);
    }

    /// <summary>
    /// Extract the entity ID from an aggregate. Default reads the "Id" property via reflection.
    /// </summary>
    protected virtual string GetEntityId(TAggregate entity)
    {
        var idProp = typeof(TDomain).GetProperty("Id")
            ?? throw new InvalidOperationException($"{typeof(TDomain).Name} has no Id property.");
        return (string)idProp.GetValue(entity.Domain)!;
    }
}
