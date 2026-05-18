# Interfaces – Strategies and Patterns

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

The interfaces layer defines abstract service contracts consumed by domain events, contexts, and repositories. This guide covers cross-cutting strategies and design decisions that apply across all interface modules.

## The Standard CRUD Pattern

Most domain services extend `IRepository<TAggregate>`, inheriting a five-method CRUD contract:

- `Exists(id)` — check presence before get or save.
- `Get(id)` — retrieve a single aggregate by ID.
- `List()` — retrieve all aggregates.
- `Save(entity)` — persist a new or updated aggregate.
- `Delete(id)` — remove by ID (idempotent).

`Exists` is used by domain events to enforce uniqueness before creation. `Delete` is always idempotent.

## Domain-Specific Variations

### IDIService — Non-Standard Naming
Manages two distinct resources (service configurations and constants), so methods use domain-specific names: `ConfigurationExists`, `GetConfiguration`, `ListAll`, `SaveConfiguration`, `DeleteConfiguration`, `SaveConstants`.

### ILoggingService — Split by Sub-Entity Type
Manages formatters, handlers, and loggers with a `ListAll` bulk-read and entity-specific write methods (`SaveFormatter`, `DeleteFormatter`, etc.).

### ICliService — Extra Query Method
Adds `GetParentArguments()` beyond the standard CRUD for top-level CLI arguments.

## Aggregate Return Types

Domain service interfaces declare return types as **Aggregates**, not plain domain records. This allows callers to invoke mutation methods directly:

```csharp
var feature = _featureService.Get(id);    // returns FeatureAggregate
feature.Rename("New Name");                // mutation method on aggregate
_featureService.Save(feature);
```

Infrastructure services (`IFileService`, `ISqliteService`) are the exception — they deal with raw I/O types.

## How Services Are Consumed

Services are injected via constructor:

```csharp
public class GetFeature : DomainEvent<GetFeatureParams, FeatureConfiguration>
{
    private readonly IFeatureService _featureService;

    public GetFeature(IFeatureService featureService)
        => _featureService = featureService;

    public override FeatureConfiguration Execute(GetFeatureParams p)
    {
        var feature = VerifyNotNull(
            _featureService.Get(p.Id),
            ErrorCodes.FeatureNotFound,
            context: ("featureId", p.Id));

        return feature.ToDomainObject();
    }
}
```

## Adding a New Service Interface

1. Define the interface in `Tiferet/Interfaces/`.
2. Extend `IRepository<TAggregate>` for standard CRUD, or `IService` for custom contracts.
3. Add XML doc comments for all methods.

## Related Documentation

- [docs/core/interfaces.md](../core/interfaces.md) — interface design and base classes
- [docs/core/repos.md](../core/repos.md) — repository implementations
- [docs/core/events.md](../core/events.md) — domain events that consume services
