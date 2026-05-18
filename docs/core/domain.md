# Domain Objects in Tiferet.NET

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

Domain objects are the structural core of the Tiferet.NET framework. Every domain concept — errors, features, app interfaces, CLI commands, DI service configurations, and logging configurations — is expressed as a C# `record` extending `DomainObject` from `Tiferet.Domain`.

All domain objects that map directly to YAML/JSON configuration use the `Configuration` suffix (e.g., `FeatureConfiguration`, `ErrorConfiguration`, `AppInterfaceConfiguration`). `ErrorResponse` is the sole exception — it is a runtime response object.

Domain objects serve a **dual role**:

1. **Runtime Domain Models** — Active participants in application execution, returned by domain events and used by Contexts.
2. **Structural Foundation for the Mappers Layer** — Aggregates wrap domain objects with mutation logic; TransferObjects provide serialization and deserialization.

## The DomainObject Base Record

`DomainObject` is an `abstract record` that provides value equality, immutability, and `with` expression support:

```csharp
// Tiferet/Domain/DomainObject.cs

public abstract record DomainObject
{
    public static void Validate(DomainObject instance)
    {
        var context = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);

        if (results.Count == 0) return;

        var failures = results
            .Select(r => new DomainValidationFailure(
                r.MemberNames.FirstOrDefault() ?? "unknown",
                r.ErrorMessage ?? "Validation failed."))
            .ToList();

        throw new TiferetDomainException(failures);
    }
}
```

Key characteristics:
- **C# records** provide structural equality, immutability, and `with` expression cloning.
- **`DomainObject.Validate`** is a `public static` method using Data Annotations (`[Required]`, `[StringLength]`, etc.) — callable from consumer assemblies in `Aggregate.Create()` factories.
- Domain objects are **read-only**; mutation logic lives on Aggregate subclasses in the mappers layer.

## Error Infrastructure

The domain layer also includes framework error definitions:

- **`ErrorCodes`** — string constants for all framework error codes (e.g., `FeatureNotFound`, `InvalidModelAttribute`).
- **`DefaultErrors`** — default error definitions for framework error codes.
- **`TiferetDomainException`** — thrown by `DomainObject.Validate` for validation failures.

## Creating and Extending Domain Objects

### 1. Define the Domain Record
- Extend `DomainObject`.
- Use `init` properties for immutable fields.
- Use `[Required]` and other Data Annotations for validation.

**Example** — `ErrorConfiguration`:
```csharp
public record ErrorConfiguration(
    string Id,
    string Name,
    IReadOnlyList<ErrorMessageConfiguration>? Message = null) : DomainObject;
```

### 2. Use in Domain Events
Domain objects are returned by domain events and consumed by contexts.

### 3. Extend in Mappers Layer
Domain objects are wrapped by Aggregates (with mutation methods) and mapped from TransferObjects (YAML/JSON deserialization).

### Best Practices
- Use `record` types for value equality and immutability.
- Use the `Configuration` suffix for types that map to YAML/JSON config.
- Keep domain objects focused on **structure and read-only behavior**.
- Place **mutation logic** in Aggregate classes in the mappers layer.
- Use `DomainObject.Validate` in `Aggregate.Create()` factories.

## Package Layout

Domain objects are defined in `Tiferet/Domain/`:

- `DomainObject.cs` — base `abstract record`.
- `ErrorCodes.cs` — framework error code constants.
- `DefaultErrors.cs` — default error definitions.
- `TiferetDomainException.cs` — validation exception.
- `App/` — `AppInterfaceConfiguration`, `AppServiceDependencyConfiguration`.
- `Cli/` — `CliCommandConfiguration`, `CliArgumentConfiguration`.
- `DI/` — `ServiceConfiguration`, `FlaggedDependencyConfiguration`.
- `Error/` — `ErrorConfiguration`, `ErrorMessageConfiguration`, `ErrorResponse`.
- `Feature/` — `FeatureConfiguration`, `FeatureEventConfiguration`, `FeatureStepConfiguration`.
- `Logging/` — `FormatterConfiguration`, `HandlerConfiguration`, `LoggerConfiguration`.

## Related Documentation

- [docs/core/code_style.md](code_style.md) — artifact comments and formatting
- [docs/core/mappers.md](mappers.md) — Aggregate and TransferObject patterns
- [docs/core/events.md](events.md) — domain event design
