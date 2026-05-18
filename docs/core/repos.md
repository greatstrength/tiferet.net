# Repositories in Tiferet.NET

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

Repositories are the concrete data-access layer in the Tiferet.NET framework. The framework provides two repository base classes:

1. **`YamlRepository<TAggregate>`** — generic YAML-backed repository implementing `IRepository<TAggregate>`. Handles flat-key CRUD by default with extensibility hooks for composite keys.
2. **`HttpRepository<TAggregate>`** — abstract HTTP-backed repository for REST API integrations with `IHttpClientFactory` and `IAuthTokenProvider` support.

Repositories are resolved at runtime through the DI container, not by direct import.

## YamlRepository

The generic YAML repository provides a complete CRUD implementation:

```csharp
public abstract class YamlRepository<TAggregate> : IRepository<TAggregate>
    where TAggregate : Aggregate
{
    protected string YamlFile { get; }
    protected string SectionKey { get; }
    protected string Encoding { get; }
}
```

### Extensibility Hooks

- **`GetSectionPath(id)`** — returns YAML key path segments. Override for composite keys (e.g., `[SectionKey, group, key]`).
- **`ReconstructId(segments)`** — reconstructs entity ID from YAML key segments. Override for composite keys.
- **`Hydrate(data, id)`** — deserializes a string-keyed dictionary into an aggregate (typically via transfer object's `FromYaml().Map()` pipeline).
- **`Dehydrate(entity)`** — serializes an aggregate into a YAML-serializable dictionary.
- **`ListFromSection(section)`** — builds aggregates from a section dictionary. Override for nested groups.

### YAML I/O

- `LoadFull()` — loads the entire YAML file as a raw dictionary.
- `SaveFull(data)` — writes the full dictionary back to the file.
- Uses `YamlHelper` for nested value navigation (`GetNestedValue`, `SetNestedValue`, `RemoveNestedValue`).
- Uses `YamlLoader` for file I/O (`ForReading`, `ForWriting`).

## HttpRepository

For REST API-backed persistence:

```csharp
public abstract class HttpRepository<TAggregate>
    where TAggregate : Aggregate
{
    protected async Task<TAggregate> GetAsync<TTransfer>(string url, ...);
    protected async Task<TAggregate> PostAsync<TTransfer>(string url, object body, ...);
    protected async Task<TAggregate> PutAsync<TTransfer>(string url, object body, ...);
    protected async Task DeleteAsync(string url);
}
```

Key characteristics:
- Uses `IHttpClientFactory` for `HttpClient` lifecycle management.
- Injects auth headers via `IAuthTokenProvider` when available.
- Wraps HTTP errors in `TiferetException` with structured error codes.
- Deserializes responses via `JsonSerializerHelper` with convention-aware naming.
- Maps responses to aggregates via `JsonTransferObject<TAggregate>.Map()`.

## Concrete YAML Repositories

- `AppYamlRepository` — app interface configurations.
- `CliYamlRepository` — CLI command configurations.
- `DIYamlRepository` — DI service configurations and constants.
- `ErrorYamlRepository` — error definitions.
- `FeatureYamlRepository` — feature workflow configurations (composite key: `group.feature_key`).
- `LoggingYamlRepository` — logging formatters, handlers, and loggers.

## Package Layout

- `YamlRepository.cs` — generic YAML repository base.
- `YamlHelper.cs` — YAML dictionary navigation utilities.
- `HttpRepository.cs` — abstract HTTP repository base.
- `AppYamlRepository.cs`, `CliYamlRepository.cs`, `DIYamlRepository.cs`, `ErrorYamlRepository.cs`, `FeatureYamlRepository.cs`, `LoggingYamlRepository.cs` — concrete implementations.

## Related Documentation

- [docs/core/interfaces.md](interfaces.md) — service contracts that repositories implement
- [docs/core/mappers.md](mappers.md) — aggregates and transfer objects used by repositories
- [docs/core/utils.md](utils.md) — file utilities used by YAML repositories
- [docs/guides/repos.md](../guides/repos.md) — strategies and patterns
