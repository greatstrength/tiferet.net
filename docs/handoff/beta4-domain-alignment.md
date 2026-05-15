# Beta 4 Handoff — Domain Layer Alignment

**Project:** Tiferet.NET
**Version context:** v1.0.0-beta.4
**Prerequisite:** v1.0.0-beta.3 (aggregate adapter pattern)

---

## Goal

With `Aggregate<TDomain>` established in beta.3, beta.4 aligns the rest of the stack so each layer uses the correct type:

| Layer | Works with | Rationale |
|---|---|---|
| **Domain objects** | `*Configuration` records | Pure structure, read-only, value equality |
| **Interfaces (Services)** | Aggregates | Services manage persistence — they need mutable entities |
| **Repositories / Utilities** | Aggregates + TransferObjects | Infrastructure layer — hydration, dehydration, serialization |
| **Domain Events** | Return `*Configuration` (domain objects) | Events are the boundary — callers get immutable domain records |
| **Contexts** | Receive domain objects from events | Orchestration layer consumes immutable results |

---

## Key Changes

### 1. Interfaces return/accept Aggregates

`IRepository<TAggregate>` already does this. Domain-specific service interfaces (`IErrorService`, `IFeatureService`, `IAppService`, `ICliService`, `IDIService`, `ILoggingService`) should consistently use aggregate types in their signatures:

- `Get(id)` → returns `TAggregate?`
- `Save(TAggregate)` → accepts aggregate
- `List()` → returns `IReadOnlyList<TAggregate>`
- `Delete(id)` → unchanged (string)

This is already largely the case via `IRepository<TAggregate>`. Verify any domain-specific service methods follow the same pattern.

### 2. Domain Events return domain objects

Domain events perform operations and return results to callers (contexts, application code). The return type should be the **domain object** (`*Configuration`), not the aggregate:

**Before (beta.3):**
```csharp
public class GetError : DomainEvent<GetErrorParams, ErrorAggregate>
{
    public override ErrorAggregate Execute(GetErrorParams p)
    {
        var aggregate = _errorService.Get(p.Id);
        // ...
        return aggregate;  // returns mutable aggregate
    }
}
```

**After (beta.4):**
```csharp
public class GetError : DomainEvent<GetErrorParams, ErrorConfiguration>
{
    public override ErrorConfiguration Execute(GetErrorParams p)
    {
        var aggregate = _errorService.Get(p.Id);
        // ...
        return aggregate.ToDomainObject();  // returns immutable domain record
    }
}
```

This applies to all domain events that return domain entities:
- `GetError` → `ErrorConfiguration`
- `ListErrors` → `IReadOnlyList<ErrorConfiguration>`
- `GetFeature` → `FeatureConfiguration`
- `ListFeatures` → `IReadOnlyList<FeatureConfiguration>`
- `GetAppInterface` → `AppInterfaceConfiguration`
- `AddError`, `AddFeature`, `AddAppInterface`, etc. → return the domain object after save
- DI, CLI, and Logging events follow the same pattern

Events that perform mutations internally still work with aggregates — they just call `ToDomainObject()` before returning.

### 3. Utilities use Aggregates / TransferObjects

Utilities (repositories, loaders) are infrastructure. They work with aggregates for persistence and transfer objects for serialization. They never accept or return raw domain objects directly:

- `YamlRepository<TAggregate>.Save(TAggregate)` — already correct
- `YamlRepository<TAggregate>.Get(id)` → returns `TAggregate?` — already correct
- `Dehydrate(TAggregate)` uses `FromAggregate()` → `ToYamlDict()` — already correct
- `Hydrate(data, id)` uses `FromYaml()` → `Map()` → returns `TAggregate` — already correct

No changes expected here — just verify consistency.

### 4. Contexts operate exclusively in the Domain layer

Contexts understand Domain — they do **not** understand Mappers. This means:

- Contexts never import or reference aggregates, transfer objects, or anything from `Tiferet.Mappers`.
- Contexts receive domain objects (`*Configuration` records) from events.
- If a context needs to mutate a domain object, either:
  - The mutation is a **domain-specific method on the domain record itself** (e.g., `FormatMessage`, `FormatResponse`, `GetStep`) — behavior that belongs on the domain object because it's a read/query concern.
  - The context **delegates to a domain event** to perform the mutation — the event handles aggregate retrieval, mutation, persistence, and returns the updated domain object.
- Contexts never construct or interact with aggregates directly.

This enforces a clean dependency direction:

```
Contexts → Domain + Events + Interfaces
Contexts ✗ Mappers
```

---

## Migration Pattern

For each domain event that currently returns an aggregate:

1. Change `DomainEvent<TParams, TAggregate>` → `DomainEvent<TParams, TConfiguration>`
2. Internal logic continues to use aggregates (from services)
3. Add `return aggregate.ToDomainObject();` (or `.Select(a => a.ToDomainObject())` for lists) at the return boundary
4. Update any context or consumer that was depending on aggregate-specific methods from the event result

---

## Impact Summary

| Layer | Change | Scope |
|---|---|---|
| `Tiferet/Events/**/*.cs` | Change return types from aggregate to domain object, add `ToDomainObject()` | ~15-20 events |
| `Tiferet/Interfaces/*.cs` | Verify aggregate types in signatures (likely minimal) | ~6 interfaces |
| `Tiferet/Contexts/*.cs` | Update to consume domain objects from events | ~4-5 contexts |
| `tests/` | Update event test assertions for domain object returns | ~10-15 test files |
| `Tiferet/Repositories/`, `Tiferet/Utilities/` | No changes expected — already use aggregates | 0 files |

---

## Design Principle

The aggregate adapter pattern creates a clean type boundary:

```
[Repos/Utils] ←— Aggregate —→ [Domain Event] ←— DomainObject —→ [Context/App]
               (mutable)                         (immutable)
```

- **Left of the event boundary**: mutable aggregates for infrastructure operations
- **Right of the event boundary**: immutable domain records for application consumption
- `ToDomainObject()` is the explicit crossing point

### Layer visibility rules

```
Domain:       knows nothing else
Interfaces:   knows Domain
Events:       knows Domain, Interfaces, Mappers (aggregates)
Contexts:     knows Domain, Interfaces, Events — NOT Mappers
Repos/Utils:  knows Domain, Interfaces, Mappers
```

Contexts are the orchestration layer. They speak the domain language exclusively — if they need something mutated or persisted, they ask an event to do it.
