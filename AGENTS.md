# AGENTS.md — Tiferet.NET (v1.0.0-beta.8)

## Project Overview

**Tiferet.NET** is a .NET 9 framework for Domain-Driven Design (DDD). It provides a layered architecture for building applications with typed domain events (sync and async), configuration-driven feature workflows, service interfaces, dependency injection, YAML-backed repositories, and HTTP-backed repositories with JSON transfer object infrastructure. It is the C# port of the [Tiferet Python framework](https://github.com/greatstrength/tiferet).

- **Repository:** https://github.com/greatstrength/tiferet.net
- **Branch:** `v1.x-proto` (development on `51-beta-8-proto` worktree)
- **.NET:** 9.0
- **Version:** `1.0.0-beta.8`

## Architecture

### Single-Package Layout

All framework code lives in a single `Tiferet` project. Namespaces map to folders. One class per file; supplementary records and enums co-located with their owning class.

```
Tiferet/
├── Assets/               # ConfigurationDefaults (default paths, file names)
├── Blueprints/           # AppBlueprint, CliBlueprint, TiferetOptions (bootstrap configuration)
├── Contexts/             # Runtime orchestration: AppInterfaceContext, FeatureContext, ErrorContext, DIContext, LoggingContext, CacheContext, RequestContext
├── DependencyInjection/  # IServiceResolver, DynamicServiceResolver, ServiceCollectionExtensions, TiferetHostExtensions
├── Domain/               # DomainObject base record + domain subnamespaces
│   ├── DomainObject.cs
│   ├── ErrorCodes.cs         # Framework error code constants
│   ├── DefaultErrors.cs      # Default error definitions
│   ├── TiferetDomainException.cs
│   ├── App/              # AppInterfaceConfiguration, AppServiceDependencyConfiguration
│   ├── Cli/              # CliCommandConfiguration, CliArgumentConfiguration (+ enums)
│   ├── DI/               # ServiceConfiguration, FlaggedDependencyConfiguration
│   ├── Error/            # ErrorConfiguration, ErrorMessageConfiguration, ErrorResponse
│   ├── Feature/          # FeatureConfiguration, FeatureEventConfiguration, FeatureStepConfiguration
│   └── Logging/          # FormatterConfiguration, HandlerConfiguration, LoggerConfiguration
├── Events/               # DomainEvent base + async events + exceptions + static helpers
│   ├── DomainEvent.cs
│   ├── AsyncDomainEvent.cs   # Async domain event base (sync adapter + async pipeline)
│   ├── TiferetException.cs
│   ├── TiferetApiException.cs
│   ├── ParseParameter.cs
│   ├── ImportDependency.cs
│   ├── App/              # AddAppInterface, GetAppInterface, BootstrapAppConfiguration, ...
│   ├── Cli/              # ListCliCommands, GetParentArguments, AddCliCommand, AddCliArgument
│   ├── DI/               # ListAllSettings, AddServiceConfiguration, SetServiceDependency, ...
│   ├── Error/            # AddError, GetError, ListErrors, RenameError, ...
│   ├── Feature/          # AddFeature, GetFeature, ListFeatures, UpdateFeature, AddFeatureStep, ...
│   └── Logging/          # ListAllLoggingConfigs, AddFormatter, AddHandler, AddLogger, ...
├── Interfaces/           # Flat: IService, IRepository<T>, IAuthTokenProvider, IAppService, IFeatureService, IErrorService, ICliService, IDIService, IConfigurationService, IFileService, ISqliteService, ILoggingService
├── Mappers/              # Aggregate, TransferObject, JsonTransferObject base classes + domain subnamespaces
│   ├── Aggregate.cs
│   ├── TransferObject.cs
│   ├── JsonTransferObject.cs # JSON transfer object base with DeserializeAndMap
│   ├── App/              # AppInterfaceAggregate, AppInterfaceYamlObject
│   ├── Cli/              # CliCommandAggregate, CliCommandYamlObject
│   ├── DI/               # DIAggregate, DIYamlObject
│   ├── Error/            # ErrorAggregate, ErrorYamlObject
│   ├── Feature/          # FeatureAggregate, FeatureYamlObject
│   └── Logging/          # LoggingAggregate, LoggingYamlObject
├── Repositories/         # YAML-backed repos + HTTP/SQLite-backed repo bases + generic YamlRepository base
│   ├── HttpRepository.cs    # Abstract HTTP repository with IHttpClientFactory + IAuthTokenProvider
│   └── SqliteRepository.cs  # Abstract SQLite repository with SqliteClient-backed CRUD
└── Utilities/            # FileLoader, YamlLoader, JsonLoader, CsvLoader, CsvDictLoader, CsvParser, SqliteClient, ReflectionActivator
    └── Json/             # NamingConvention, JsonNamingAttribute, ConventionNamingResolver, JsonSerializerHelper
```

### Companion Projects

- `Tiferet.Testing` — Test harness, base classes (`DomainEventHarness`, `AggregateTestBase`, `TransferObjectTestBase`, `JsonTransferObjectTestBase`) for consumer test projects.
- `tests/Tiferet.Tests` — Framework unit tests (references both `Tiferet` and `Tiferet.Testing`).
- `tests/Tiferet.Tests.Integration` — Integration tests.
- `examples/Tiferet.Examples.Calculator` — Calculator example app.
- `examples/Tiferet.Examples.WebApi` — Task list REST API example (WebBlueprint + health checks).

## Key Concepts

### Domain Naming Convention

All domain objects that map directly to YAML/JSON configuration use the `Configuration` suffix:

- `FeatureConfiguration` — a workflow definition loaded from `feature.yml`
- `FeatureEventConfiguration` — a step within a feature workflow
- `ErrorConfiguration` — an error definition loaded from `error.yml`
- `AppInterfaceConfiguration` — an app interface definition from `app.yml`
- `CliCommandConfiguration`, `CliArgumentConfiguration` — CLI definitions from `cli.yml`
- `ServiceConfiguration`, `FlaggedDependencyConfiguration` — DI definitions from `container.yml`
- `FormatterConfiguration`, `HandlerConfiguration`, `LoggerConfiguration` — logging definitions

`ErrorResponse` is NOT a configuration type — it is a runtime response object and does not carry the suffix.

### DomainEvent

`DomainEvent<TParams, TResult>` is the base for all synchronous domain operations:

- Typed entry point: `TResult Execute(TParams parameters)`
- Runtime entry point: `object? Execute(Dictionary<string, object?> data)` (used by the feature pipeline)
- `Verify(bool expression, string errorCode, ...)` — domain rule enforcement
- `RaiseError(string errorCode, ...)` — direct structured error raising
- Params records are co-located in the same `.cs` file as the event class

### AsyncDomainEvent

`AsyncDomainEvent<TParams, TResult>` is the base for asynchronous domain operations:

- Typed entry point: `Task<TResult> ExecuteAsync(TParams parameters)`
- Runtime entry point: `Task<object?> ExecuteAsync(Dictionary<string, object?> data)`
- Sync adapter: overrides `Execute(Dictionary<string, object?> data)` via `Task.Run` to avoid `SynchronizationContext` deadlocks — async events work in the existing sync pipeline with no caller changes
- Inherits `Verify`, `VerifyNotNull`, `VerifyNotExists`, and `RaiseError` from `DomainEvent`
- `FeatureContext.ExecuteFeatureAsync` and `AppInterfaceContext.RunAsync` provide the native async pipeline

### Aggregate and TransferObject

- `Aggregate` — abstract record extending `DomainObject`. The aggregate IS the domain object (no wrapper). Exposes `SetAttribute` for validated in-place mutation via reflection. Concrete aggregates are positional records (e.g., `record ErrorAggregate(string Id, string Name, ...) : Aggregate`).
- `DomainObject.Validate` — `public static` method for Data Annotations validation. Callable from consumer assemblies in `Aggregate.Create()` factories.
- `TransferObject` / `TransferObject<TAggregate>` — bridges YAML/JSON persistence and runtime aggregates via `Map()` and `ToDictionary(role)`. Single type parameter constrained to `Aggregate`.
- `JsonTransferObject<TAggregate>` — extends `TransferObject<TAggregate>` for JSON API deserialization with convention-aware naming. Provides `DeserializeAndMap<TTransfer>()` convenience.
- `HttpRepository<TAggregate>` — abstract HTTP-backed repository with `IHttpClientFactory` integration, `IAuthTokenProvider` auth injection, and template methods (`GetAsync`, `PostAsync`, `PutAsync`, `DeleteAsync`) with automatic transfer object `.Map()` calls.

### Runtime Flow

Tiferet supports two bootstrapping modes:

**Standalone** (no external DI):
1. `AppBlueprint.BuildApp(interfaceId, configDir)` — loads config, wires all contexts and repositories, returns `AppInterfaceContext`

**Host-integrated** (Microsoft.Extensions.DependencyInjection):
1. `AppBlueprint.ConfigureServices(services, options)` — registers all Tiferet services into `IServiceCollection`
2. `AppBlueprint.BuildApp(provider)` — resolves `AppInterfaceContext` from the service provider
3. Or use `services.AddTiferet(config)` / `builder.UseTiferet()` extension methods

Both paths converge on the same execution flow:
1. `AppInterfaceContext.Run(featureId, data)` — parses request, executes feature pipeline, handles response (sync)
2. `AppInterfaceContext.RunAsync(featureId, data)` — async equivalent for pipelines with `AsyncDomainEvent` steps
3. `FeatureContext.ExecuteFeature` / `ExecuteFeatureAsync` — loads feature config, resolves event dependencies via `DIContext`, executes each step sequentially
4. Each step is a `DomainEvent` or `AsyncDomainEvent` subclass resolved by `DynamicServiceResolver`

### Exception Hierarchy

Both live in `Tiferet.Events`:

- `TiferetException` — base structured exception with `ErrorCode` and `Context`
- `TiferetApiException` — API-facing exception with `Name` for response formatting

## Configuration Files

Applications configure behavior via YAML. As of beta 5, a single consolidated `config.yml` is supported (individual files also work):

| Section | Key | Purpose |
|---|---|---|
| interfaces | `interfaces` | App interface definitions (`AppInterfaceConfiguration`) |
| services | `services` / `const` | DI service configurations (`ServiceConfiguration`) |
| features | `features` | Feature workflow definitions (`FeatureConfiguration`) |
| errors | `errors` | Error definitions with multilingual messages (`ErrorConfiguration`) |
| cli | `cli.cmds` | CLI command definitions (`CliCommandConfiguration`) |
| logging | `logging` | Logging formatters, handlers, and loggers |

## Building and Testing

```bash
# Activate virtual environment if needed
source .venv/bin/activate

# Build entire solution
dotnet build Tiferet.sln

# Run unit tests
dotnet test tests/Tiferet.Tests/Tiferet.Tests.csproj

# Run integration tests
dotnet test tests/Tiferet.Tests.Integration/Tiferet.Tests.Integration.csproj
```

## Structured Code Style

All code follows a strict artifact comment hierarchy:

- `// *** <section>` — Top-level: `imports`, `events`, `contexts`, `interfaces`, `mappers`, `repos`, `constants`, `utils`
- `// ** <category>: <name>` — Mid-level: `core`, `infra`, `app` (imports); `event: <name>`, `context: <name>`, etc.
- `// * <component>` — Low-level: `attribute: <name>`, `init`, `method: <name>`, `method: <name> (static)`

One empty line between `// ***` and first `// **`; one empty line between each `// *` section; one empty line after docstrings and between code snippets.

## Version Roadmap

- **Beta 1** (`1.0.0-beta.1`): Single `Tiferet` package; YAML configuration baked in.
- **Beta 2** (`1.0.0-beta.2`): `Create` factories on aggregates, `DomainObject.Validate`, domain records purely structural.
- **Beta 3** (`1.0.0-beta.3`): Aggregate evolution — `Aggregate` is now an `abstract record` extending `DomainObject` directly (no generic wrapper). All concrete aggregates are positional records. `.Domain` property removed; consumers access fields directly on aggregates. `TransferObject<TAggregate>` uses single type parameter.
- **Beta 4** (`1.0.0-beta.4`): Domain Layer Alignment — `ErrorCodes` and `DefaultErrors` moved from `Assets` to `Domain`. `TiferetDomainException` added.
- **Beta 5** (`1.0.0-beta.5`): Assets namespace with `ConfigurationDefaults`, `BootstrapAppConfiguration` event, consolidated `config.yml` support.
- **Beta 6** (`1.0.0-beta.6`): Microsoft.Extensions.DependencyInjection integration — `TiferetOptions`, `AppBlueprint.ConfigureServices`, `AddTiferet` / `UseTiferet` extensions, `TiferetHostExtensions`.
- **Beta 7** (`1.0.0-beta.7`): Consumer wishlist — `DomainObject.Validate` public accessibility, `AsyncDomainEvent` with sync adapter and async pipeline, JSON transfer object infrastructure (`JsonNamingAttribute`, `ConventionNamingResolver`, `JsonSerializerHelper`, `JsonTransferObject<T>`), `HttpRepository<T>` with `IAuthTokenProvider`, `Tiferet.Testing` harnesses (`AggregateTestBase`, `TransferObjectTestBase`, `JsonTransferObjectTestBase`).
- **Beta 8** (`1.0.0-beta.8`): Prototype wishlist — `SqliteRepository<T>` generic base class, `WebBlueprint` for ASP.NET Minimal API endpoint generation from feature config, `IHealthCheck` implementations (`YamlConfigHealthCheck`, `SqliteHealthCheck`) with `AddTiferetHealthChecks` extension, Task List Web API example.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the full workflow, including prototype branching conventions (`beta-<N>-proto` worktree branches).
