# Mappers – Strategies and Patterns

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

The mappers layer bridges persistent configuration and runtime domain objects. This guide covers the strategies for creating aggregates, transfer objects, and JSON transfer objects.

## Aggregate Pattern

### Create Factory
Every concrete aggregate provides a static `Create()` factory that validates via `DomainObject.Validate`:

```csharp
public static ErrorAggregate Create(string id, string name, ...)
{
    var domain = new ErrorConfiguration(id, name, ...);
    DomainObject.Validate(domain);
    return new ErrorAggregate(domain);
}
```

### Delegated Properties
Aggregates expose domain record properties via delegation:

```csharp
public string Id => State.Id;
public string Name => State.Name;
```

### Mutation Methods
Use `Mutate` for type-safe mutations via `with {}`:

```csharp
public void Rename(string name) => Mutate(s => s with { Name = name });
```

Use `SetAttribute` for dynamic/generic scenarios (validated via reflection).

## TransferObject Pattern

### YAML Transfer Objects
Override `Map()` to construct the target aggregate:

```csharp
public override ErrorAggregate Map(Dictionary<string, object?>? overrides = null)
{
    return ErrorAggregate.Create(Id, Name, Message?.ToList());
}
```

### Role-Based Serialization
Define `Roles` for context-specific serialization:

```csharp
protected override Dictionary<string, RoleConfig> Roles => new()
{
    [SerializationRoles.ToDataYaml] = new RoleConfig { Exclude = { "Id" } }
};
```

## JSON Transfer Object Pattern

For REST API integrations, use `JsonTransferObject<TAggregate>`:

```csharp
[JsonNaming(NamingConvention.SnakeCase)]
public class UserJson : JsonTransferObject<UserAggregate>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public override UserAggregate Map(Dictionary<string, object?>? overrides = null)
        => UserAggregate.Create(FirstName, LastName);
}
```

One-step convenience:
```csharp
var user = JsonTransferObject<UserAggregate>.DeserializeAndMap<UserJson>(jsonString);
```

## Testing

`Tiferet.Testing` provides test harnesses:

- **`AggregateTestBase<TAggregate, TDomain>`** — auto-tests Create factory, delegated properties, `ToDomainObject` round-trip.
- **`TransferObjectTestBase<TTransfer, TAggregate>`** — auto-tests `Map()` verification.
- **`JsonTransferObjectTestBase<TTransfer, TAggregate>`** — adds JSON deserialization round-trip.

## Related Documentation

- [docs/core/mappers.md](../core/mappers.md) — base class design
- [docs/core/domain.md](../core/domain.md) — domain record structure
- [docs/core/repos.md](../core/repos.md) — repositories that consume mappers
