# Domain Events in Tiferet.NET

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

Domain events are the operational core of the Tiferet.NET framework. Every focused domain action — validation, service interaction, computation, or orchestration — is expressed as a class extending `DomainEvent` or `AsyncDomainEvent` from `Tiferet.Events`.

## The DomainEvent Base Class

`DomainEvent` provides the foundational infrastructure for all domain operations:

```csharp
public abstract class DomainEvent
{
    public void Verify(bool expression, string errorCode, string? message = null,
        params (string Key, object Value)[] context);

    public T VerifyNotNull<T>(T? value, string errorCode, string? message = null,
        params (string Key, object Value)[] context) where T : class;

    public void VerifyNotExists(bool exists, string errorCode, string? message = null,
        params (string Key, object Value)[] context);

    public static void RaiseError(string errorCode, string? message = null,
        params (string Key, object Value)[] context);

    public abstract object? Execute(Dictionary<string, object?> data);
}
```

Key characteristics:
- **`Execute(Dictionary<string, object?> data)`** — runtime entry point used by the feature pipeline.
- **`Verify`** — asserts a boolean expression; raises `TiferetException` on failure.
- **`VerifyNotNull<T>`** — null-check that returns the non-null value, eliminating null-forgiving operators.
- **`VerifyNotExists`** — asserts an entity does not already exist (uniqueness guard).
- **`RaiseError`** — static method for structured error raising with context tuples.

### Generic DomainEvent

`DomainEvent<TParams, TResult>` provides typed execution:

```csharp
public abstract class DomainEvent<TParams, TResult> : DomainEvent
{
    public abstract TResult Execute(TParams parameters);

    public override object? Execute(Dictionary<string, object?> data)
    {
        var parameters = ReflectionActivator.Construct<TParams>(data);
        return Execute(parameters);
    }
}
```

The generic form:
- Defines a typed `Execute(TParams)` for direct invocation.
- Overrides the dictionary-based `Execute` for runtime pipeline compatibility.
- Uses `ReflectionActivator.Construct<TParams>` to build params records from dictionaries.

## AsyncDomainEvent

For events backed by async services (HTTP clients, database operations):

```csharp
public abstract class AsyncDomainEvent<TParams, TResult> : AsyncDomainEvent
{
    public abstract Task<TResult> ExecuteAsync(TParams parameters);
}
```

Key characteristics:
- Provides `ExecuteAsync` as the primary entry point.
- Includes a **sync adapter** via `Task.Run` to avoid `SynchronizationContext` deadlocks — async events work in the existing sync pipeline with no caller changes.
- Native async pipeline available via `AppInterfaceContext.RunAsync` and `FeatureContext.ExecuteFeatureAsync`.

## Params Records

Each domain event co-locates a `sealed record` for its parameters:

```csharp
public sealed record AddErrorParams(
    string Id,
    string Name,
    IReadOnlyList<ErrorMessageConfiguration>? Message = null);

public class AddError : DomainEvent<AddErrorParams, ErrorAggregate>
{
    private readonly IErrorService _errorService;

    public AddError(IErrorService errorService) => _errorService = errorService;

    public override ErrorAggregate Execute(AddErrorParams p)
    {
        // Verify the error doesn't already exist.
        VerifyNotExists(
            _errorService.Exists(p.Id),
            ErrorCodes.ErrorAlreadyExists,
            context: ("id", p.Id));

        // Create and save the error.
        var error = ErrorAggregate.Create(p.Id, p.Name, p.Message);
        _errorService.Save(error);
        return error;
    }
}
```

## Exception Hierarchy

Both live in `Tiferet.Events`:

- **`TiferetException`** — base structured exception with `ErrorCode` and `Context` dictionary.
- **`TiferetApiException`** — API-facing exception with `Name` for response formatting.

## Package Layout

- `DomainEvent.cs` — base class and generic `DomainEvent<TParams, TResult>`.
- `AsyncDomainEvent.cs` — async base with sync adapter.
- `TiferetException.cs` — structured exception.
- `TiferetApiException.cs` — API-facing exception.
- `ParseParameter.cs` — static parameter resolution utility.
- `ImportDependency.cs` — static dependency import utility.
- `App/` — app interface events (`AddAppInterface`, `GetAppInterface`, `BootstrapAppConfiguration`, etc.).
- `Cli/` — CLI events (`ListCliCommands`, `AddCliCommand`, etc.).
- `DI/` — DI events (`ListAllSettings`, `AddServiceConfiguration`, etc.).
- `Error/` — error events (`AddError`, `GetError`, `ListErrors`, etc.).
- `Feature/` — feature events (`AddFeature`, `GetFeature`, `ListFeatures`, etc.).
- `Logging/` — logging events (`ListAllLoggingConfigs`, `AddFormatter`, etc.).

## Related Documentation

- [docs/core/domain.md](domain.md) — domain object design
- [docs/core/interfaces.md](interfaces.md) — service contract patterns
- [docs/core/code_style.md](code_style.md) — artifact comments and formatting
