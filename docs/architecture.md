# Architecture

tiferet.net is a layered Domain-Driven Design framework. Each layer has a single responsibility, and layers only depend on layers below them. The entry point for most applications is `Tiferet.Blueprints`, which wires everything together automatically.

## Layer Diagram

```
┌─────────────────────────────────────────┐
│           Tiferet.Blueprints            │  ← Your entry point
│     AppBlueprint · CliBlueprint         │
└────────────────────┬────────────────────┘
                     │
┌────────────────────▼────────────────────┐
│           Tiferet.Contexts              │  ← Runtime orchestration
│  AppInterfaceContext · FeatureContext   │
│  DIContext · ErrorContext · Logging     │
└────────────────────┬────────────────────┘
                     │
        ┌────────────┴────────────┐
        │                         │
┌───────▼────────┐   ┌────────────▼────────────┐
│ Tiferet.Events │   │  Tiferet.Repositories   │
│ DomainEvent    │   │  YAML-backed services    │
│ + your events  │   └────────────┬────────────┘
└───────┬────────┘                │
        │              ┌──────────▼──────────┐
        │              │  Tiferet.Utilities  │
        │              │  YAML · JSON · CSV  │
        │              │  SQLite · File I/O  │
        │              └─────────────────────┘
        │
┌───────▼────────────────────────────────┐
│  Tiferet.Interfaces                    │  ← Abstract contracts
│  IService · IRepository               │
└───────┬────────────────────────────────┘
        │
┌───────▼────────────────────────────────┐
│  Tiferet.Mappers                       │  ← YAML ↔ domain mapping
│  Aggregate · TransferObject            │
└───────┬────────────────────────────────┘
        │
┌───────▼────────────────────────────────┐
│  Tiferet.Domain                        │  ← Domain model objects
│  AppInterface · Feature · Error · CLI  │
└───────┬────────────────────────────────┘
        │
┌───────▼────────────────────────────────┐
│  Tiferet.Core                          │  ← Shared infrastructure
│  Exceptions · ErrorCodes · Reflection  │
└────────────────────────────────────────┘
```

## Layer Responsibilities

### Tiferet.Core
Shared building blocks:
- `TiferetException` / `TiferetApiException` — structured exceptions; `Message` is always a human-readable string.
- `ErrorCodes` — framework error code constants.
- `ParseParameter.Parse(string)` — resolves `$env.VAR_NAME` prefixes to environment variable values.
- `ImportDependency.Resolve(assemblyName, className)` — loads a `Type` from an assembly by name.
- `ReflectionActivator.Construct<T>(Dictionary<string, object?>)` — constructs record instances from string-keyed dictionaries; the single source of that logic for both the feature pipeline and the mapper layer.

### Tiferet.Domain
Read-only domain model objects: `AppInterface`, `Feature`, `FeatureStep`, `Error`, `CliCommand`, `CliArgument`, `ServiceConfiguration`, and logging types. These represent the structural shape of every configurable concept in the framework.

### Tiferet.Mappers
Bridges YAML configuration to runtime domain objects. Two base classes:
- **`Aggregate`** — mutable domain objects (add mutation methods like `AddStep`, `Rename`)
- **`TransferObject`** — YAML deserialization layer; maps to aggregates via `Map()`

Each domain concept has a corresponding `*Aggregate` and `*YamlObject` (e.g., `FeatureAggregate`, `FeatureYamlObject`).

`SerializationRoles` provides the well-known role name constants (`ToModel`, `ToDataYaml`) used in `TransferObject` subclass `Roles` dictionaries.

### Tiferet.Interfaces
Abstract service contracts as interfaces: `IFeatureService`, `IErrorService`, `IDIService`, `ILoggingService`, `ICliService`. Domain events and contexts depend only on these interfaces, never on concrete implementations.

### Tiferet.Events
The operational core. `DomainEvent<TParams, TResult>` is the base for all application logic:
- **Typed execution**: `Execute(TParams params)` — called directly with a typed record
- **Dictionary execution**: `Execute(Dictionary<string, object?> data)` — called by the feature pipeline; constructs `TParams` via reflection
- **`Verify`** — assert a condition or raise a structured error
- **`RaiseError`** — raise a structured error unconditionally

Framework events (in `Tiferet.Events`) handle internal operations like loading features, resolving DI services, and fetching error definitions. Your application events extend `DomainEvent<TParams, TResult>` and implement `Execute(TParams)`.

### Tiferet.Repositories
Concrete `IService` implementations backed by YAML files: `FeatureYamlRepository`, `ErrorYamlRepository`, `DIYamlRepository`, `AppYamlRepository`, `CliYamlRepository`, `LoggingYamlRepository`. Used by `AppBlueprint` as defaults; override via `app.yml` services if needed.

### Tiferet.Utilities
Infrastructure utilities: `YamlLoader`, `JsonLoader`, `CsvLoader`, `CsvDictLoader`, `SqliteClient`, `FileLoader`. Used internally by repositories; also available for use in your own domain events and services.

### Tiferet.Contexts
Runtime orchestration:
- **`AppInterfaceContext : IDisposable`** — top-level entry point; exposes `Run(featureId, data)`. Implements `IDisposable` as the composition root — use a `using` declaration to release owned logger factories.
- **`FeatureContext`** — loads feature definitions and executes each step (domain event) in sequence
- **`DIContext`** — resolves domain event instances from `container.yml` service configurations
- **`ErrorContext`** — formats `TiferetException` into `TiferetApiException` with localized messages
- **`CacheContext`** — in-memory cache shared between DI and feature resolution
- **`LoggingContext : IDisposable`** — configures `Microsoft.Extensions.Logging` from `logging.yml`; lazily builds and caches the per-config `ILoggerFactory`, disposing it (and any internally-created default factory) when the context is disposed

### Tiferet.Blueprints
One-step bootstrappers:
- **`AppBlueprint.BuildApp(interfaceId, configDir)`** — loads app.yml, wires all contexts and services, returns `AppInterfaceContext`
- **`CliBlueprint.BuildCli(interfaceId, configDir)`** — calls `BuildApp` then maps `cli.yml` command definitions to a `System.CommandLine` `RootCommand`

## Runtime Flow

When `app.Run("calc.add", data: new { A = "3", B = "4" })` is called:

```
AppInterfaceContext.Run("calc.add", data)
  └─ FeatureContext.Execute("calc.add", data)
       └─ FeatureYamlRepository.Get("calc.add")     → loads feature.yml
            → Feature has one Step: ServiceId = "add_number_event"
       └─ DIContext.GetDependency("add_number_event")
            → DIYamlRepository.Get("add_number_event") → loads container.yml
            → ReflectionActivator instantiates AddNumber
       └─ AddNumber.Execute(data)
            → constructs CalcParams(A="3", B="4") via reflection
            → Execute(CalcParams) returns 7.0
       └─ result = 7.0
  └─ ErrorContext wraps any TiferetException → TiferetApiException
  └─ returns 7.0
```

## Configuration Files

All configuration lives in a single directory (`app/configs/` by default):

| File | Purpose |
|---|---|
| `app.yml` | Defines named interfaces; references the assembly that owns the application |
| `container.yml` | Maps service IDs to domain event types (assembly + class name) |
| `feature.yml` | Defines feature workflows as ordered steps with service IDs and optional fixed parameters |
| `error.yml` | Error codes with names and localized message templates |
| `cli.yml` | CLI command structure with argument/option definitions |
| `logging.yml` | `Microsoft.Extensions.Logging` formatters, handlers, and loggers |

## Error Handling

Errors flow through two exception types:

- **`TiferetException`** — internal, raised by `Verify`/`RaiseError` in domain events. Contains an `ErrorCode` and optional context key-value pairs.
- **`TiferetApiException`** — public-facing, raised by `ErrorContext` when formatting a `TiferetException`. Contains a `Message` string resolved from `error.yml` with context values substituted in.

Your application code catches `TiferetApiException`. The `ErrorCode` from `error.yml` is matched case-insensitively, and `{placeholder}` tokens in the message template are replaced with matching context values.
