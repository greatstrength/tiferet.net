# Contexts in Tiferet.NET

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

Contexts are a core component of the Tiferet.NET framework, representing the structural "body" of an application at runtime. They encapsulate user interactions, internal orchestration, and supporting services. Contexts are composed by `AppBlueprint` during bootstrapping and form a graph-like dependency structure.

## Context Types

### AppInterfaceContext
The top-level context that composes feature execution, error handling, and logging into a unified pipeline. Implements `IDisposable`.

- **`Run(featureId, headers, data)`** — full sync pipeline: parse → execute → handle response.
- **`RunAsync(featureId, headers, data)`** — full async pipeline for `AsyncDomainEvent` support.
- **`ParseRequest`** — creates a `RequestContext`.
- **`ExecuteFeature`** — delegates to `FeatureContext`.
- **`HandleError`** — formats errors via `ErrorContext` and throws `TiferetApiException`.
- **`HandleResponse`** — extracts the result from `RequestContext`.

### FeatureContext
Orchestrates feature workflow execution:
- Loads features via `GetFeature` event (with caching via `CacheContext`).
- Resolves event dependencies via `DIContext`.
- Parses parameters (including `$r.` request-backed and `$env.` environment references).
- Evaluates step conditions.
- Executes steps sequentially (sync and async).

### ErrorContext
Formats structured errors by looking up error definitions via `GetError` and producing `ErrorResponse` objects with localized messages.

### DIContext
Manages feature-level dependency injection:
- Loads service configurations via `ListAllSettings`.
- Caches resolved instances via `CacheContext`.
- Resolves dependencies by `serviceId` with flag-based selection via `DynamicServiceResolver`.

### CacheContext
Simple in-memory key-value cache used by `FeatureContext` and `DIContext`.

### LoggingContext
Builds `ILogger` instances from YAML-configured formatters, handlers, and loggers. Implements `IDisposable` to release logger factories.

### RequestContext
Carries request data, headers, and results through the pipeline. Methods include `SetResult`, `HandleResponse`, and `GetHeader`.

## Runtime Flow

1. `AppBlueprint.BuildApp()` assembles all contexts.
2. `AppInterfaceContext.Run()` parses the request and delegates to `FeatureContext`.
3. `FeatureContext.ExecuteFeature()` loads the feature, resolves each step via `DIContext`, and executes sequentially.
4. Results flow back through `RequestContext.HandleResponse()`.

## Creating New Contexts

1. Define the class in `Tiferet/Contexts/`.
2. Accept dependencies via constructor injection.
3. Implement `IDisposable` if the context owns disposable resources.

## Package Layout

- `AppInterfaceContext.cs` — top-level pipeline composition.
- `FeatureContext.cs` — feature workflow execution.
- `ErrorContext.cs` — error formatting.
- `DIContext.cs` — feature-level dependency injection.
- `CacheContext.cs` — in-memory caching.
- `LoggingContext.cs` — logger construction.
- `RequestContext.cs` — request/response carrier.

## Related Documentation

- [docs/core/events.md](events.md) — domain events executed by contexts
- [docs/core/blueprints.md](blueprints.md) — bootstrapping that wires contexts
- [docs/core/code_style.md](code_style.md) — artifact comments and formatting
