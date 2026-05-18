# tiferet.net

> **Beta software.** APIs are stabilizing. Breaking changes are possible before 1.0.

A .NET framework for Domain-Driven Design — configuration-driven features, typed domain events (sync and async), generic service contracts, YAML and HTTP-backed repositories, and JSON transfer object infrastructure. The C# port of the [Tiferet Python framework](https://github.com/greatstrength/tiferet).

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

## Async Domain Events

For events backed by async services (e.g., HTTP clients), extend `AsyncDomainEvent<TParams, TResult>`:

```csharp
using Tiferet.Events;

public class FetchUser : AsyncDomainEvent<FetchUserParams, User>
{
    private readonly IUserApiClient _client;
    public FetchUser(IUserApiClient client) => _client = client;

    public override async Task<User> ExecuteAsync(FetchUserParams p)
        => await _client.GetUserAsync(p.UserId);
}
```

Async events work in the sync pipeline automatically (via a `Task.Run` adapter), or natively via `app.RunAsync()`.

## JSON Transfer Objects

For REST API integrations, use `JsonTransferObject<TAggregate>` with naming convention support:

```csharp
using Tiferet.Mappers;
using Tiferet.Utilities.Json;

[JsonNaming(NamingConvention.SnakeCase)]
public class UserJson : JsonTransferObject<UserAggregate>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public override UserAggregate Map(Dictionary<string, object?>? overrides = null)
        => UserAggregate.Create(FirstName, LastName);
}
```

## HTTP Repositories

Extend `HttpRepository<TAggregate>` for HTTP-backed persistence:

```csharp
using Tiferet.Repositories;

public class UserRepository : HttpRepository<UserAggregate>
{
    public UserRepository(IHttpClientFactory factory, IAuthTokenProvider? auth = null)
        : base(factory, auth) { }

    public Task<UserAggregate> GetUser(string id)
        => GetAsync<UserJson>($"/api/users/{id}");
}
```

## Error Handling

Framework errors are raised as `TiferetApiException`. Define error messages in `app/configs/error.yml` and reference them by code in your domain events:

```csharp
Verify(b != 0, "DIVISION_BY_ZERO", "Cannot divide by zero.");
```

## Testing

`Tiferet.Testing` provides base classes for testing aggregates, transfer objects, and domain events:

- `DomainEventHarness` — sync and async event execution + error assertion
- `AggregateTestBase<TAggregate, TDomain>` — auto-tests for Create factory, delegated properties, ToDomainObject round-trip
- `TransferObjectTestBase<TTransfer, TAggregate>` — auto-tests for Map() verification
- `JsonTransferObjectTestBase<TTransfer, TAggregate>` — adds JSON deserialization round-trip

## Examples

- [Calculator](https://github.com/greatstrength/tiferet.net/tree/v1.x-proto/examples/Tiferet.Examples.Calculator) — arithmetic operations with demo and CLI modes

## Documentation

- [Tutorial](https://github.com/greatstrength/tiferet.net/blob/v1.x-proto/docs/tutorial.md) — step-by-step walkthrough building the calculator app from scratch
- [Architecture](https://github.com/greatstrength/tiferet.net/blob/v1.x-proto/docs/architecture.md) — layer diagram, responsibilities, and runtime flow

### Core (Internal Design)

Component-level design documentation for framework contributors and AI agents.

- [Code Style](docs/core/code_style.md) — artifact comments, naming, spacing, and formatting conventions
- [Domain](docs/core/domain.md) — `DomainObject` base record and domain module design
- [Events](docs/core/events.md) — `DomainEvent`, `AsyncDomainEvent`, and exception hierarchy
- [Interfaces](docs/core/interfaces.md) — `IService`, `IRepository<T>`, and service contract patterns
- [Mappers](docs/core/mappers.md) — `Aggregate`, `TransferObject`, `JsonTransferObject` base classes
- [Contexts](docs/core/contexts.md) — runtime orchestration contexts
- [Repositories](docs/core/repos.md) — `YamlRepository<T>` and `HttpRepository<T>` base classes
- [Utilities](docs/core/utils.md) — infrastructure utilities (`FileLoader`, `YamlLoader`, `JsonLoader`, etc.)
- [Blueprints](docs/core/blueprints.md) — `AppBlueprint` and `CliBlueprint` bootstrapping

### Guides (Strategies & Patterns)

User-facing guides with strategies, patterns, and usage examples.

- [Interfaces](docs/guides/interfaces.md) — CRUD patterns, aggregate return types, service consumption
- [Mappers](docs/guides/mappers.md) — aggregate factories, transfer objects, JSON transfer objects, testing
- [Repositories](docs/guides/repos.md) — YAML and HTTP repository patterns
- [Contexts](docs/guides/contexts.md) — pipeline pattern, feature execution, error handling
- [Blueprints](docs/guides/blueprints.md) — standalone, host-integrated, and CLI bootstrapping
- Domain Guides: [App](docs/guides/domain/app.md) · [CLI](docs/guides/domain/cli.md) · [DI](docs/guides/domain/di.md) · [Error](docs/guides/domain/error.md) · [Feature](docs/guides/domain/feature.md) · [Logging](docs/guides/domain/logging.md)
- Utility Guides: [File](docs/guides/utils/file.md) · [YAML](docs/guides/utils/yaml.md) · [JSON](docs/guides/utils/json.md) · [CSV](docs/guides/utils/csv.md) · [SQLite](docs/guides/utils/sqlite.md)

## License

[MIT](https://github.com/greatstrength/tiferet.net/blob/v1.x-proto/LICENSE)
