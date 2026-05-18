# Blueprints – Strategies and Patterns

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

Blueprints are the entry point for Tiferet.NET applications. This guide covers the strategies for standalone, host-integrated, and CLI bootstrapping.

## Standalone Bootstrapping

For scripts, console apps, or simple services:

```csharp
using var app = AppBlueprint.BuildApp("my_app", configDir: "app/configs");
var result = app.Run("calc.add", data: new Dictionary<string, object?>
{
    ["A"] = "3", ["B"] = "4"
});
Console.WriteLine(result); // 7
```

`AppInterfaceContext` implements `IDisposable` — use `using` to ensure logger factories are released.

## Host-Integrated Bootstrapping

For ASP.NET Core or generic host applications:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTiferet(options =>
{
    options.InterfaceId = "my_api";
    options.ConfigDir = "app/configs";
});
var app = builder.Build();
app.UseTiferet();
```

Or via direct registration:
```csharp
AppBlueprint.ConfigureServices(services, new TiferetOptions { ... });
var provider = services.BuildServiceProvider();
using var app = AppBlueprint.BuildApp(provider);
```

## CLI Bootstrapping

For command-line applications:

```csharp
var cli = CliBlueprint.BuildCli("my_app", configDir: "app/configs");
await cli.InvokeAsync(args);
```

CLI commands are defined in YAML and translated to `System.CommandLine` commands at build time.

## Configuration

All configuration lives in YAML files under the config directory:

- `config.yml` (consolidated) or individual files (`app.yml`, `container.yml`, `feature.yml`, `error.yml`, `cli.yml`, `logging.yml`).

The `ConfigurationDefaults` class in `Tiferet.Assets` provides default paths and file names.

## Related Documentation

- [docs/core/blueprints.md](../core/blueprints.md) — blueprint design
- [docs/core/contexts.md](../core/contexts.md) — contexts wired by blueprints
- [docs/guides/contexts.md](contexts.md) — context usage patterns
