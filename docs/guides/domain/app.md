# Domain – App (Bootstrap & Assembly)

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

The App domain defines **how the application is assembled**. `AppInterfaceConfiguration` is the blueprint for a running application instance, declaring which services to wire and how to configure them. `AppServiceDependencyConfiguration` represents a single service binding in that blueprint.

## Domain Objects

### AppInterfaceConfiguration
Top-level configuration for an application interface, loaded from `app.yml`:
- `Id` — unique interface identifier.
- `Name` — display name.
- `Description` — optional description.
- `AssemblyName` — .NET assembly for the interface context.
- `TypeName` — fully qualified type name for the context class.
- `LoggerId` — logger configuration reference (default: `"default"`).
- `Flags` — flags for dependency resolution.
- `Services` — list of `AppServiceDependencyConfiguration`.
- `Constants` — key-value constants.

### AppServiceDependencyConfiguration
A single injectable service dependency binding:
- `ServiceId` — canonical service identifier.
- `AssemblyName` — .NET assembly containing the service class.
- `TypeName` — fully qualified class name.
- `Parameters` — configuration parameters.

## Domain Events

- `GetAppInterface` — retrieve by ID (used during bootstrapping).
- `BootstrapAppConfiguration` — resolve all pre-DI services from config.
- `AddAppInterface` — create a new interface configuration.
- `UpdateAppInterface` — update scalar attributes.
- `SetAppConstants` — set or clear constants.
- `SetServiceDependency` — add or update a dependency.
- `RemoveServiceDependency` — remove a dependency.
- `RemoveAppInterface` — delete an interface.
- `ListAppInterfaces` — list all interfaces.

## YAML Configuration

```yaml
interfaces:
  my_app:
    Name: My Application
    AssemblyName: MyApp
    TypeName: MyApp.Contexts.AppContext
    Services:
      - ServiceId: my_repo
        AssemblyName: MyApp
        TypeName: MyApp.Repos.MyRepository
```

## Related Documentation

- [docs/core/domain.md](../../core/domain.md) — DomainObject base class
- [docs/core/blueprints.md](../../core/blueprints.md) — bootstrapping that consumes app configuration
- [docs/core/contexts.md](../../core/contexts.md) — contexts wired from app configuration
