# Domain – Dependency Injection

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

The DI domain defines service configurations for feature-level dependency injection. These configurations tell the framework how to resolve domain events and their dependencies at runtime.

## Domain Objects

### ServiceConfiguration
- `Id` — service identifier (e.g., `"add_number_event"`).
- `AssemblyName` — .NET assembly containing the service.
- `TypeName` — fully qualified type name.
- `Parameters` — configuration parameters.
- `Dependencies` — list of `FlaggedDependencyConfiguration`.

### FlaggedDependencyConfiguration
- `ServiceId` — the dependency service ID.
- `Flag` — optional flag for conditional resolution.

## Domain Events

- `ListAllSettings` — load all service configurations and constants.
- `AddServiceConfiguration` — create a new service configuration.
- `RemoveServiceConfiguration` — delete a service configuration.
- `SetServiceDependency` — add or update a flagged dependency.
- `RemoveServiceDependency` — remove a flagged dependency.
- `SetDefaultServiceConfiguration` — set the default service for a flag.
- `SetServiceConstants` — set or clear constants.

## YAML Configuration

```yaml
services:
  add_number_event:
    AssemblyName: MyApp
    TypeName: MyApp.Events.AddNumber
  divide_number_event:
    AssemblyName: MyApp
    TypeName: MyApp.Events.DivideNumber
const:
  precision: "2"
```

## Related Documentation

- [docs/core/domain.md](../../core/domain.md) — DomainObject base class
- [docs/core/contexts.md](../../core/contexts.md) — DIContext
