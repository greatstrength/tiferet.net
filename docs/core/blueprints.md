# Blueprints in Tiferet.NET

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

Blueprints are the primary public entry point for Tiferet.NET applications. They provide a clean, high-level API for bootstrapping — loading services, resolving interfaces, wiring contexts, and preparing the application for feature execution.

## AppBlueprint

`AppBlueprint` is a static class providing two bootstrapping modes:

### Standalone Mode
No external DI container required:

```csharp
using var app = AppBlueprint.BuildApp("my_app", configDir: "app/configs");
var result = app.Run("calc.add", data: new Dictionary<string, object?>
{
    ["A"] = "3", ["B"] = "4"
});
```

The standalone `BuildApp` method:
1. Bootstraps pre-DI services via `BootstrapAppConfiguration` event.
2. Creates domain events with resolved repository services.
3. Wires all contexts (`CacheContext`, `ErrorContext`, `LoggingContext`, `DIContext`, `FeatureContext`).
4. Returns a ready-to-use `AppInterfaceContext`.

### Host-Integrated Mode
Uses `Microsoft.Extensions.DependencyInjection`:

```csharp
// Option 1: Direct registration
AppBlueprint.ConfigureServices(services, new TiferetOptions
{
    InterfaceId = "my_app",
    ConfigDir = "app/configs"
});
var app = AppBlueprint.BuildApp(serviceProvider);

// Option 2: Extension methods
services.AddTiferet(config => { ... });
builder.UseTiferet();
```

`ConfigureServices` registers all framework services into `IServiceCollection`:
- Repository services as singletons.
- Domain events as transient (wired to repositories).
- Contexts with appropriate lifetimes.
- Integrates with host `ILoggerFactory` when available.

### TiferetOptions
Configuration record for host-integrated bootstrapping:
- `InterfaceId` — the interface to load.
- `ConfigDir` — configuration directory (default: `app/assets`).
- `ConfigFile` — configuration file name (default: `config.yml`).

## CliBlueprint

`CliBlueprint` extends `AppBlueprint` with `System.CommandLine`-based CLI support:

```csharp
var cli = CliBlueprint.BuildCli("my_app", configDir: "app/configs");
await cli.InvokeAsync(args);
```

Translates CLI commands defined in YAML (`cli.cmds`) into `System.CommandLine` commands and dispatches feature execution.

## DI Extension Methods

- **`ServiceCollectionExtensions.AddTiferet(services, configure)`** — configures Tiferet via an `Action<TiferetOptions>`.
- **`TiferetHostExtensions.UseTiferet(builder)`** — integrates Tiferet into the generic host pipeline.

## Package Layout

- `AppBlueprint.cs` — standalone and host-integrated bootstrapping.
- `CliBlueprint.cs` — CLI bootstrapping with `System.CommandLine`.
- `TiferetOptions.cs` — bootstrap configuration options.

## Related Documentation

- [docs/core/contexts.md](contexts.md) — contexts wired by blueprints
- [docs/core/events.md](events.md) — domain events used during bootstrapping
- [docs/guides/blueprints.md](../guides/blueprints.md) — strategies and patterns
