# Tutorial: Building a Calculator App

This tutorial walks through building a simple calculator application with tiferet.net. By the end you will have a fully working app with five arithmetic operations, structured error handling, and an optional command-line interface — all driven by YAML configuration.

The complete source for this example lives in [`examples/Tiferet.Examples.Calculator`](../examples/Tiferet.Examples.Calculator).

## Prerequisites

- .NET 9 SDK
- A .NET console project

```
dotnet new console -n MyCalculator
cd MyCalculator
dotnet add package Tiferet.Blueprints --prerelease
```

`Tiferet.Blueprints` brings in all other framework packages as transitive dependencies.

## Project Structure

```
MyCalculator/
├── Program.cs
├── Events/
│   └── CalcEvents.cs
└── app/
    └── configs/
        ├── app.yml
        ├── container.yml
        ├── feature.yml
        └── error.yml
```

## Step 1: Define Domain Events

Domain events are the operational core of the application. Each event performs one focused operation. They extend `DomainEvent<TParams, TResult>` where `TParams` is a typed record holding the inputs and `TResult` is the return type.

Create `Events/CalcEvents.cs`:

```csharp
using System.Globalization;
using Tiferet.Events;

namespace MyCalculator.Events;

// Input record shared by all two-operand events.
public sealed record CalcParams(string A, string B);

// Base event providing shared numeric validation.
public abstract class BasicCalcEvent<TResult> : DomainEvent<CalcParams, TResult>
{
    protected static double VerifyNumber(string value)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        // Raise a structured error — matched to an entry in error.yml.
        RaiseError("INVALID_INPUT",
            $"Invalid number: {value}",
            ("value", value));

        return 0; // unreachable
    }
}

public class AddNumber : BasicCalcEvent<double>
{
    public override double Execute(CalcParams p)
        => VerifyNumber(p.A) + VerifyNumber(p.B);
}

public class SubtractNumber : BasicCalcEvent<double>
{
    public override double Execute(CalcParams p)
        => VerifyNumber(p.A) - VerifyNumber(p.B);
}

public class MultiplyNumber : BasicCalcEvent<double>
{
    public override double Execute(CalcParams p)
        => VerifyNumber(p.A) * VerifyNumber(p.B);
}

public class DivideNumber : BasicCalcEvent<double>
{
    public override double Execute(CalcParams p)
    {
        var a = VerifyNumber(p.A);
        var b = VerifyNumber(p.B);
        Verify(b != 0, "DIVISION_BY_ZERO", "Cannot divide by zero.");
        return a / b;
    }
}

public class ExponentiateNumber : BasicCalcEvent<double>
{
    public override double Execute(CalcParams p)
        => Math.Pow(VerifyNumber(p.A), VerifyNumber(p.B));
}
```

### Key patterns

- **`DomainEvent<TParams, TResult>`** — typed base class. `TParams` is constructed automatically from the data dictionary when called through the feature pipeline.
- **`Verify(expression, errorCode, message)`** — asserts a condition; raises `TiferetException` on failure.
- **`RaiseError(errorCode, message, context...)`** — raises a structured error unconditionally.
- Error codes (`"INVALID_INPUT"`, `"DIVISION_BY_ZERO"`) are matched to entries in `error.yml` for user-facing messages.

## Step 2: Configure the Application

All configuration lives in `app/configs/`. The framework reads these files at startup — no code changes are needed to add new features or wire new events.

### app.yml — Interface Definition

Defines the named application interface. `AssemblyName` and `TypeName` identify the entry assembly.

```yaml
interfaces:
  basic_calc:
    Name: Basic Calculator
    AssemblyName: MyCalculator
    TypeName: MyCalculator.Program
    Description: Perform basic calculator operations
```

### container.yml — Service Registration

Maps service IDs to domain event types. The `ServiceId` in `feature.yml` must match a key here.

```yaml
services:
  add_number_event:
    AssemblyName: MyCalculator
    TypeName: MyCalculator.Events.AddNumber
  subtract_number_event:
    AssemblyName: MyCalculator
    TypeName: MyCalculator.Events.SubtractNumber
  multiply_number_event:
    AssemblyName: MyCalculator
    TypeName: MyCalculator.Events.MultiplyNumber
  divide_number_event:
    AssemblyName: MyCalculator
    TypeName: MyCalculator.Events.DivideNumber
  exponentiate_number_event:
    AssemblyName: MyCalculator
    TypeName: MyCalculator.Events.ExponentiateNumber
const: {}
```

### feature.yml — Feature Workflows

Defines features as ordered lists of steps. Each step references a `ServiceId` from `container.yml`. Steps can include fixed `Parameters` to pass values that are not provided by the caller — the `sqrt` feature demonstrates this by fixing `B` to `0.5`.

```yaml
features:
  calc:
    add:
      Name: Add Number
      Description: Adds one number to another
      Steps:
        - ServiceId: add_number_event
          Name: Add A and B
    subtract:
      Name: Subtract Number
      Description: Subtracts one number from another
      Steps:
        - ServiceId: subtract_number_event
          Name: Subtract B from A
    multiply:
      Name: Multiply Number
      Description: Multiplies one number by another
      Steps:
        - ServiceId: multiply_number_event
          Name: Multiply A and B
    divide:
      Name: Divide Number
      Description: Divides one number by another
      Steps:
        - ServiceId: divide_number_event
          Name: Divide A by B
    exp:
      Name: Exponentiate Number
      Description: Raises one number to the power of another
      Steps:
        - ServiceId: exponentiate_number_event
          Name: Raise A to the power of B
    sqrt:
      Name: Square Root
      Description: Calculates the square root of a number
      Steps:
        - ServiceId: exponentiate_number_event
          Name: Calculate square root of A
          Parameters:
            B: '0.5'
```

Feature IDs are constructed as `<group>.<key>` — e.g., `calc.add`, `calc.sqrt`.

### error.yml — Error Messages

Maps error codes to display names and localized message templates. Placeholder tokens like `{value}` are filled in from the context passed to `RaiseError`.

```yaml
errors:
  INVALID_INPUT:
    Name: Invalid Numeric Input
    Messages:
      - Lang: en_US
        Text: 'Value {value} must be a number'
      - Lang: es_ES
        Text: 'El valor {value} debe ser un numero'
  DIVISION_BY_ZERO:
    Name: Division By Zero
    Messages:
      - Lang: en_US
        Text: 'Cannot divide by zero'
      - Lang: es_ES
        Text: 'No se puede dividir por cero'
```

## Step 3: Bootstrap and Run

`AppBlueprint.BuildApp` reads your configuration, resolves services, wires all contexts, and returns an `AppInterfaceContext` ready for use.

Update `Program.cs`:

```csharp
using Tiferet.Blueprints;
using Tiferet.Core;

var configDir = Path.Combine(AppContext.BaseDirectory, "app", "configs");
var app = AppBlueprint.BuildApp("basic_calc", configDir);

var cases = new[]
{
    ("calc.add",      new[] { ("A", "1"),  ("B", "2")  }, "1 + 2 = {0}"),
    ("calc.subtract", new[] { ("A", "5"),  ("B", "3")  }, "5 - 3 = {0}"),
    ("calc.multiply", new[] { ("A", "4"),  ("B", "3")  }, "4 * 3 = {0}"),
    ("calc.divide",   new[] { ("A", "8"),  ("B", "2")  }, "8 / 2 = {0}"),
    ("calc.divide",   new[] { ("A", "8"),  ("B", "0")  }, "8 / 0 = {0}"),  // error
    ("calc.exp",      new[] { ("A", "2"),  ("B", "3")  }, "2 ** 3 = {0}"),
    ("calc.sqrt",     new[] { ("A", "16")              }, "√16 = {0}"),
};

foreach (var (featureId, pairs, fmt) in cases)
{
    try
    {
        var data = pairs.ToDictionary(p => p.Item1, p => (object?)p.Item2);
        var result = app.Run(featureId, data: data);
        Console.WriteLine(string.Format(fmt, result));
    }
    catch (TiferetApiException ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
```

**Expected output:**

```
1 + 2 = 3
5 - 3 = 2
4 * 3 = 12
8 / 2 = 4
Error: Cannot divide by zero
2 ** 3 = 8
√16 = 4
```

### Mark config files for output

The YAML files must be present alongside the compiled binary. Add this to your `.csproj`:

```xml
<ItemGroup>
  <None Update="app/configs/**">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

## Step 4: Add a CLI (Optional)

`CliBlueprint` maps a `cli.yml` command definition to a `System.CommandLine` `RootCommand`. Each command is wired to dispatch the corresponding feature, collecting positional arguments or named options as the data dictionary.

### cli.yml — CLI Command Definitions

```yaml
cli:
  cmds:
    calc:
      add:
        Name: Add Number Command
        Description: Adds two numbers.
        Arguments:
          - NameOrFlags:
              - a
            Description: The first number.
          - NameOrFlags:
              - b
            Description: The second number.
      subtract:
        Name: Subtract Number Command
        Description: Subtracts one number from another.
        Arguments:
          - NameOrFlags:
              - a
            Description: The number to subtract from.
          - NameOrFlags:
              - b
            Description: The number to subtract.
      divide:
        Name: Divide Number Command
        Description: Divides one number by another.
        Arguments:
          - NameOrFlags:
              - a
            Description: The numerator.
          - NameOrFlags:
              - b
            Description: The denominator.
      sqrt:
        Name: Square Root Command
        Description: Calculates the square root of a number.
        Arguments:
          - NameOrFlags:
              - a
            Description: The number to square root.
```

Commands are grouped by the YAML key hierarchy: the outer key (`calc`) becomes the group command and the inner keys (`add`, `subtract`, etc.) become sub-commands. The feature ID is derived as `<group>.<key>`.

Named flags (prefixed with `-` or `--`) are mapped to `Option<string>`; unprefixed names are positional `Argument<string>`.

### Update Program.cs for CLI

```csharp
using Tiferet.Blueprints;

var configDir = Path.Combine(AppContext.BaseDirectory, "app", "configs");

var cli = CliBlueprint.BuildCli("basic_calc", configDir: configDir, description: "Calculator CLI");
await cli.InvokeAsync(args);
```

**Usage:**

```
dotnet run -- calc add 3 4
# Output: 7

dotnet run -- calc sqrt 16
# Output: 4

dotnet run -- calc divide 8 0
# Error: Cannot divide by zero
```

## What's Next

- **Multi-step features** — add more than one `ServiceId` under `Steps` to chain events; each step receives the result of the previous step as additional data
- **Fixed parameters** — use `Parameters` in a feature step to inject constant values (as seen in `calc.sqrt` with `B: '0.5'`)
- **Custom service overrides** — specify `Services` in `app.yml` to replace default YAML repositories with your own implementations
- **Environment variable parameters** — use `$ENV_VAR_NAME` syntax in `container.yml` parameter values; `ParseParameter` resolves them at startup

See [`docs/architecture.md`](architecture.md) for a full explanation of the layer stack and runtime flow.
