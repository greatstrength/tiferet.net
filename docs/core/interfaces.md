# Interfaces in Tiferet.NET

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

Interfaces define abstract service contracts consumed by domain events, contexts, and repositories. Every service interface extends `IService` from `Tiferet.Interfaces`. The framework provides two levels of abstraction:

- **`IService`** — marker interface for all service contracts.
- **`IRepository<TAggregate>`** — generic repository interface providing standard CRUD operations.

Domain-specific service interfaces (e.g., `IErrorService`, `IFeatureService`) extend `IRepository<TAggregate>` and inherit a complete CRUD contract.

## The IService and IRepository Interfaces

```csharp
// Marker interface
public interface IService { }

// Generic CRUD repository
public interface IRepository<TAggregate> : IService
{
    bool Exists(string id);
    TAggregate? Get(string id);
    IReadOnlyList<TAggregate> List();
    void Save(TAggregate entity);
    void Delete(string id);
}
```

Key characteristics:
- `IRepository<TAggregate>` codifies the standard five-method CRUD pattern used by most domain services.
- `Delete` is always idempotent — deleting a non-existent ID must not raise an error.
- Return types are **Aggregates**, not plain domain objects, so callers can invoke mutation methods directly.

## Domain Service Interfaces

Most domain services simply extend `IRepository` with the appropriate aggregate type:

```csharp
public interface IErrorService : IRepository<ErrorAggregate> { }
public interface IFeatureService : IRepository<FeatureAggregate> { }
public interface IAppService : IRepository<AppInterfaceAggregate> { }
public interface ICliService : IRepository<CliCommandAggregate> { }
```

### Special Interfaces

- **`IAuthTokenProvider`** — provides async bearer token injection for `HttpRepository`.
- **`IConfigurationService`** — abstracts structured configuration loading/saving.
- **`IFileService`** — low-level file stream lifecycle (`Open`, `Close`, `Dispose`).
- **`ISqliteService`** — SQLite database operations.
- **`IDIService`** — manages service configurations and constants (non-standard naming due to dual resources).
- **`ILoggingService`** — manages formatters, handlers, and loggers (split by sub-entity type).

## Creating New Service Interfaces

1. Define the interface in `Tiferet/Interfaces/`.
2. Extend `IRepository<TAggregate>` for standard CRUD, or `IService` for custom contracts.
3. Use XML doc comments for all methods.

```csharp
public interface ICalculatorService : IService
{
    double Compute(string operation, double a, double b);
    IReadOnlyList<string> History();
}
```

## Package Layout

All interfaces are defined in `Tiferet/Interfaces/` (flat namespace):

- `IService.cs` — marker interface.
- `IRepository.cs` — generic CRUD repository.
- `IAppService.cs`, `ICliService.cs`, `IDIService.cs`, `IErrorService.cs`, `IFeatureService.cs`, `ILoggingService.cs` — domain services.
- `IConfigurationService.cs`, `IFileService.cs`, `ISqliteService.cs` — infrastructure services.
- `IAuthTokenProvider.cs` — auth token provider for HTTP repositories.

## Related Documentation

- [docs/core/repos.md](repos.md) — repository implementations
- [docs/core/events.md](events.md) — domain events that consume services
- [docs/guides/interfaces.md](../guides/interfaces.md) — strategies and patterns
