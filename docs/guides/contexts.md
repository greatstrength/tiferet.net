# Contexts – Strategies and Patterns

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

Contexts define the runtime shape of Tiferet.NET applications. This guide covers common patterns for using and extending contexts.

## The Pipeline Pattern

`AppInterfaceContext.Run()` implements a standard pipeline:

1. **Parse** — `ParseRequest()` creates a `RequestContext` with headers and data.
2. **Execute** — `ExecuteFeature()` loads the feature and runs each step sequentially.
3. **Handle** — `HandleResponse()` extracts the result, or `HandleError()` formats errors.

### Sync vs Async
- `Run()` — synchronous pipeline. Async events use `Task.Run` adapter internally.
- `RunAsync()` — native async pipeline. Preferred when features contain `AsyncDomainEvent` steps.

## Feature Execution

`FeatureContext` orchestrates each step:

1. Load feature by ID (cached via `CacheContext`).
2. For each step, resolve the domain event via `DIContext.GetDependency`.
3. Parse step parameters (supporting `$r.` request references and `$env.` environment variables).
4. Evaluate step conditions against request data.
5. Execute the event and store the result in `RequestContext.Data`.

### Parameter Prefixes
- `$r.<key>` — resolves from `RequestContext.Data`.
- `$env.<key>` — resolves from environment variables.
- No prefix — literal value.

### Conditional Steps
Steps can have a `Condition` expression evaluated against request data:
```yaml
Steps:
  - ServiceId: my_event
    Condition: "$r.mode != null"
```

## Error Handling

`ErrorContext` looks up error definitions by code (including framework defaults), formats multilingual messages, and produces `ErrorResponse` objects. `AppInterfaceContext.HandleError` wraps non-Tiferet exceptions and throws `TiferetApiException`.

## Extending Contexts

Create a new context class that accepts dependencies via constructor injection:

```csharp
public class MyApiContext
{
    private readonly FeatureContext _features;
    private readonly ErrorContext _errors;

    public MyApiContext(FeatureContext features, ErrorContext errors)
    {
        _features = features;
        _errors = errors;
    }
}
```

## Related Documentation

- [docs/core/contexts.md](../core/contexts.md) — context base classes
- [docs/core/events.md](../core/events.md) — domain events
- [docs/core/blueprints.md](../core/blueprints.md) — bootstrapping
