# AGENTS.md — Tiferet.NET (v1.0.0-beta.1)

## Project Overview

**Tiferet.NET** is a .NET 9 framework for Domain-Driven Design (DDD). It provides a layered architecture for building applications with typed domain events, configuration-driven feature workflows, service interfaces, dependency injection, and YAML-backed repositories. It is the C# port of the [Tiferet Python framework](https://github.com/greatstrength/tiferet).

- **Repository:** https://github.com/greatstrength/tiferet.net
- **Branch:** `v1.x-proto`
- **.NET:** 9.0
- **Version:** `1.0.0-beta.1`

## Architecture

### Single-Package Layout

As of `1.0.0-beta.1`, all framework code lives in a single `Tiferet` project. Namespaces map to folders. One class per file; supplementary records and enums co-located with their owning class.

```
Tiferet/
├── Assets/               # ErrorCodes, DefaultErrors (downward-importing constants)
├── Blueprints/           # AppBlueprint, CliBlueprint (one-step bootstrappers)
├── Contexts/             # Runtime orchestration: AppInterfaceContext, FeatureContext, ErrorContext, DIContext, LoggingContext, CacheContext, RequestContext
├── DependencyInjection/  # IServiceResolver, DynamicServiceResolver, ServiceCollectionExtensions
├── Domain/               # DomainObject base record + domain subnamespaces
│   ├── DomainObject.cs
│   ├── App/              # AppInterfaceConfiguration, AppServiceDependencyConfiguration
│   ├── Cli/              # CliCommandConfiguration, CliArgumentConfiguration (+ enums)
│   ├── DI/               # ServiceConfiguration, FlaggedDependencyConfiguration
│   ├── Error/            # ErrorConfiguration, ErrorMessageConfiguration, ErrorResponse
│   ├── Feature/          # FeatureConfiguration, FeatureEventConfiguration, FeatureStepConfiguration
│   └── Logging/          # FormatterConfiguration, HandlerConfiguration, LoggerConfiguration
├── Events/               # DomainEvent base + exceptions + static helpers
│   ├── DomainEvent.cs
│   ├── TiferetException.cs
│   ├── TiferetApiException.cs
│   ├── ParseParameter.cs
│   ├── ImportDependency.cs
│   ├── App/              # AddAppInterface, GetAppInterface, UpdateAppInterface, ...
│   ├── Cli/              # ListCliCommands, GetParentArguments, AddCliCommand, AddCliArgument
│   ├── DI/               # ListAllSettings, AddServiceConfiguration, SetServiceDependency, ...
│   ├── Error/            # AddError, GetError, ListErrors, RenameError, ...
│   ├── Feature/          # AddFeature, GetFeature, ListFeatures, UpdateFeature, AddFeatureStep, ...
│   └── Logging/          # ListAllLoggingConfigs, AddFormatter, AddHandler, AddLogger, ...
├── Interfaces/           # Flat: IService, IRepository<T>, IAppService, IFeatureService, IErrorService, ICliService, IDIService, IConfigurationService, IFileService, ISqliteService, ILoggingService
├── Mappers/              # Aggregate<T>, TransferObject base classes + domain subnamespaces
│   ├── Aggregate.cs
│   ├── TransferObject.cs
│   ├── App/              # AppInterfaceAggregate, AppInterfaceYamlObject
│   ├── Cli/              # CliCommandAggregate, CliCommandYamlObject
│   ├── DI/               # ServiceConfigurationAggregate, FlaggedDependencyAggregate, DIYamlObject
│   ├── Error/            # ErrorAggregate, ErrorYamlObject
│   ├── Feature/          # FeatureAggregate, FeatureEventAggregate, FeatureYamlObject
│   └── Logging/          # FormatterAggregate, HandlerAggregate, LoggerAggregate, LoggingYamlObject
├── Repositories/         # Flat YAML-backed repos: AppYamlRepository, FeatureYamlRepository, ErrorYamlRepository, ...
└── Utilities/            # Flat: FileLoader, YamlLoader, JsonLoader, CsvLoader, SqliteClient, ReflectionActivator
```

### Companion Projects

- `Tiferet.Testing` — Test harness, base classes, and `DomainEventHarness` helpers for consumer test projects.
- `tests/Tiferet.Tests` — Framework unit tests (references both `Tiferet` and `Tiferet.Testing`).
- `tests/Tiferet.Tests.Integration` — Integration tests.
- `examples/Tiferet.Examples.Calculator` — Calculator example app.

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

`DomainEvent<TParams, TResult>` is the base for all domain operations:

- Typed entry point: `TResult Execute(TParams parameters)`
- Runtime entry point: `object? Execute(Dictionary<string, object?> data)` (used by the feature pipeline)
- `Verify(bool expression, string errorCode, ...)` — domain rule enforcement
- `RaiseError(string errorCode, ...)` — direct structured error raising
- Params records are co-located in the same `.cs` file as the event class

### Aggregate and TransferObject

- `Aggregate<TDomain>` — wraps an immutable domain record, exposes `SetAttribute` for validated mutations via record cloning
- `TransferObject` / `TransferObject<TDomain, TAggregate>` — bridges YAML/JSON persistence and runtime aggregates via `Map()` and `ToDictionary(role)`

### Runtime Flow

1. `AppBlueprint.BuildApp(interfaceId, configDir)` — loads `app.yml`, wires all contexts and repositories, returns `AppInterfaceContext`
2. `AppInterfaceContext.Run(featureId, data)` — parses request, executes feature pipeline, handles response
3. `FeatureContext.ExecuteFeature` — loads feature config, resolves event dependencies via `DIContext`, executes each step sequentially
4. Each step is a `DomainEvent` subclass resolved by `DynamicServiceResolver`

### Exception Hierarchy

Both live in `Tiferet.Events`:

- `TiferetException` — base structured exception with `ErrorCode` and `Context`
- `TiferetApiException` — API-facing exception with `Name` for response formatting

## Configuration Files

Applications configure behavior via YAML in `app/configs/`:

| File | Key | Purpose |
|---|---|---|
| `app.yml` | `interfaces` | App interface definitions (`AppInterfaceConfiguration`) |
| `container.yml` | `services` / `const` | DI service configurations (`ServiceConfiguration`) |
| `feature.yml` | `features` | Feature workflow definitions (`FeatureConfiguration`) |
| `error.yml` | `errors` | Error definitions with multilingual messages (`ErrorConfiguration`) |
| `cli.yml` | `cli.cmds` | CLI command definitions (`CliCommandConfiguration`) |
| `logging.yml` | `logging` | Logging formatters, handlers, and loggers |

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

## Package Naming Roadmap

- **Beta 1** (`1.0.0-beta.1`): Single `Tiferet` package; YAML configuration baked in.
- **Beta 2** (planned): `Tiferet` core + native .NET `IConfiguration` integration; YAML components split into optional `Tiferet.Yaml` package for Python-interop deployments.

## Contributing

1. Tie work to a GitHub issue.
2. Write a TRD for non-trivial changes.
3. Implement following structured code style and namespace conventions above.
4. Separate functional changes from docs/config in distinct commits.
5. Include `Co-Authored-By:` lines when collaborating with AI agents.
6. Publish a Collaboration Report on the issue upon completion.
