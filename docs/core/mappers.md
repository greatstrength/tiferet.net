# Mappers in Tiferet.NET

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

The mappers layer (`Tiferet.Mappers`) bridges persistent configuration and runtime domain objects. It introduces three base classes:

1. **Aggregate / Aggregate&lt;TDomain&gt;** — mutable wrapper around an immutable domain record. Provides `Mutate`, `SetAttribute`, and `ToDomainObject`.
2. **TransferObject / TransferObject&lt;TAggregate&gt;** — bridges YAML/JSON persistence and runtime aggregates via `Map()` and `ToDictionary(role)`.
3. **JsonTransferObject&lt;TAggregate&gt;** — extends `TransferObject` for JSON API deserialization with convention-aware naming.

## The Aggregate Base

`Aggregate` is a non-generic marker record. `Aggregate<TDomain>` wraps a private mutable `State` instance:

```csharp
public abstract record Aggregate<TDomain> : Aggregate
    where TDomain : DomainObject
{
    protected TDomain State { get; private set; }

    protected Aggregate(TDomain state) => State = state;

    // Type-safe mutation via native with {} expression.
    protected void Mutate(Func<TDomain, TDomain> mutator) => State = mutator(State);

    // Name-based mutation for dynamic scenarios.
    protected void SetAttribute(string attribute, object? value);

    // Returns the internal domain record.
    public TDomain ToDomainObject() => State;
}
```

Key characteristics:
- The aggregate **IS** the domain object at runtime — it holds and exposes the immutable domain record via `State`.
- **`Mutate`** uses native `with {}` expressions for type-safe mutations.
- **`SetAttribute`** clones via the compiler-generated `<Clone>$` method and sets properties via reflection — used for dynamic/generic scenarios.
- **`ToDomainObject`** extracts the underlying record for serialization or persistence.
- Concrete aggregates provide static `Create()` factory methods that call `DomainObject.Validate`.

### Example — ErrorAggregate
```csharp
public record ErrorAggregate : Aggregate<ErrorConfiguration>
{
    public string Id => State.Id;
    public string Name => State.Name;

    private ErrorAggregate(ErrorConfiguration state) : base(state) { }

    public static ErrorAggregate Create(string id, string name, ...)
    {
        var domain = new ErrorConfiguration(id, name, ...);
        DomainObject.Validate(domain);
        return new ErrorAggregate(domain);
    }

    public void Rename(string name) => Mutate(s => s with { Name = name });
}
```

## The TransferObject Base

`TransferObject` provides role-based serialization:

```csharp
public abstract class TransferObject
{
    protected virtual Dictionary<string, RoleConfig> Roles { get; } = new();

    public virtual Dictionary<string, object?> ToDictionary(
        string? role = null,
        Dictionary<string, object?>? overrides = null);
}

public abstract class TransferObject<TAggregate> : TransferObject
    where TAggregate : Aggregate
{
    public abstract TAggregate Map(Dictionary<string, object?>? overrides = null);
}
```

Key characteristics:
- **`ToDictionary(role)`** reflects properties, filtering by role-specific `Include`/`Exclude` sets.
- **`Map()`** constructs the target aggregate from deserialized data.
- `RoleConfig` supports `Exclude`, `Include`, `ByAlias`, and `ExcludeNull`.

### Well-Known Roles
- `SerializationRoles.ToModel` — mapping to an aggregate.
- `SerializationRoles.ToDataYaml` — serialization for YAML persistence.

## The JsonTransferObject Base

For REST API integrations:

```csharp
public abstract class JsonTransferObject<TAggregate> : TransferObject<TAggregate>
    where TAggregate : Aggregate
{
    public static TAggregate DeserializeAndMap<TTransfer>(
        string json,
        Dictionary<string, object?>? overrides = null)
        where TTransfer : JsonTransferObject<TAggregate>;
}
```

Uses `[JsonNaming(NamingConvention.SnakeCase)]` attribute and `JsonSerializerHelper` for convention-aware deserialization.

## Package Layout

- `Aggregate.cs` — `Aggregate` marker and `Aggregate<TDomain>` base.
- `TransferObject.cs` — `TransferObject`, `TransferObject<TAggregate>`, `RoleConfig`, `SerializationRoles`.
- `JsonTransferObject.cs` — JSON transfer object base.
- `App/` — `AppInterfaceAggregate`, `AppInterfaceYamlObject`.
- `Cli/` — `CliCommandAggregate`, `CliCommandYamlObject`.
- `DI/` — `DIAggregate`, `DIYamlObject`.
- `Error/` — `ErrorAggregate`, `ErrorYamlObject`.
- `Feature/` — `FeatureAggregate`, `FeatureYamlObject`.
- `Logging/` — `LoggingAggregate`, `LoggingYamlObject`.

## Related Documentation

- [docs/core/domain.md](domain.md) — domain object base class
- [docs/core/repos.md](repos.md) — repository implementations that use mappers
- [docs/guides/mappers.md](../guides/mappers.md) — strategies and patterns
