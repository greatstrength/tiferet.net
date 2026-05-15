# tiferet.net

> **Beta software.** APIs are stabilizing. Breaking changes are possible before 1.0.

A .NET framework for Domain-Driven Design — configuration-driven features, typed domain events, generic service contracts, and YAML-backed repositories. The C# port of the [Tiferet Python framework](https://github.com/greatstrength/tiferet).

## Installation

```
dotnet add package Tiferet --prerelease
```

`Tiferet` is a single package containing everything: domain objects, events, mappers, service interfaces, YAML repositories, utilities, DI, and the `AppBlueprint`/`CliBlueprint` bootstrappers.

For test helpers (base classes and harness utilities), also add:

```
dotnet add package Tiferet.Testing --prerelease
```

## Quick Start

The following is a condensed version of the [calculator example](https://github.com/greatstrength/tiferet.net/tree/v1.x-proto/examples/Tiferet.Examples.Calculator).

### 1. Define a domain event

```csharp
using Tiferet.Events;

public sealed record AddParams(string A, string B);

public class AddNumber : DomainEvent<AddParams, double>
{
    public override double Execute(AddParams p)
    {
        var a = double.Parse(p.A);
        var b = double.Parse(p.B);
        return a + b;
    }
}
```

### 2. Configure your app

**app/configs/app.yml**
```yaml
interfaces:
  my_app:
    Name: My App
    AssemblyName: MyApp
    TypeName: MyApp.Program
```

**app/configs/container.yml**
```yaml
services:
  add_number_event:
    AssemblyName: MyApp
    TypeName: MyApp.Events.AddNumber
```

**app/configs/feature.yml**
```yaml
features:
  calc:
    add:
      Name: Add Number
      Steps:
        - ServiceId: add_number_event
          Name: Add A and B
```

### 3. Bootstrap and run

`AppInterfaceContext` implements `IDisposable` — use a `using` declaration to ensure logger factories are released.

```csharp
using Tiferet.Blueprints;

using var app = AppBlueprint.BuildApp("my_app", configDir: "app/configs");

var result = app.Run("calc.add", data: new Dictionary<string, object?>
{
    ["A"] = "3",
    ["B"] = "4"
});

Console.WriteLine(result); // 7
```

### 4. Optionally add a CLI

```csharp
var cli = CliBlueprint.BuildCli("my_app", configDir: "app/configs");
await cli.InvokeAsync(args);
```

## Error Handling

Framework errors are raised as `TiferetApiException`. Define error messages in `app/configs/error.yml` and reference them by code in your domain events:

```csharp
Verify(b != 0, "DIVISION_BY_ZERO", "Cannot divide by zero.");
```

## Examples

- [Calculator](https://github.com/greatstrength/tiferet.net/tree/v1.x-proto/examples/Tiferet.Examples.Calculator) — arithmetic operations with demo and CLI modes

## Documentation

- [Tutorial](https://github.com/greatstrength/tiferet.net/blob/v1.x-proto/docs/tutorial.md) — step-by-step walkthrough building the calculator app from scratch
- [Architecture](https://github.com/greatstrength/tiferet.net/blob/v1.x-proto/docs/architecture.md) — layer diagram, responsibilities, and runtime flow

## License

[MIT](https://github.com/greatstrength/tiferet.net/blob/v1.x-proto/LICENSE)
