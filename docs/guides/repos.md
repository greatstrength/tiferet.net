# Repositories – Strategies and Patterns

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

Repositories are the concrete data-access layer. This guide covers the strategies for implementing YAML-backed and HTTP-backed repositories.

## YamlRepository Pattern

### Extending for Flat Keys (Default)
Override `Hydrate` to deserialize via transfer objects:

```csharp
public class ErrorYamlRepository : YamlRepository<ErrorAggregate>, IErrorService
{
    public ErrorYamlRepository(string yamlFile)
        : base(yamlFile, "errors") { }

    protected override ErrorAggregate Hydrate(Dictionary<string, object> data, string id)
    {
        var transfer = ErrorYamlObject.FromYaml(data, id);
        return transfer.Map();
    }
}
```

### Extending for Composite Keys
Override `GetSectionPath` and `ReconstructId`:

```csharp
// FeatureYamlRepository uses group.feature_key composite IDs
protected override string[] GetSectionPath(string id)
{
    var parts = id.Split('.');
    return [SectionKey, parts[0], parts[1]];
}

protected override string ReconstructId(params string[] segments)
    => $"{segments[0]}.{segments[1]}";
```

### Idempotent Deletes
`YamlRepository.Delete` is idempotent by default — deleting a non-existent ID is a no-op.

## HttpRepository Pattern

### Implementing an HTTP Repository

```csharp
public class UserRepository : HttpRepository<UserAggregate>
{
    public UserRepository(IHttpClientFactory factory, IAuthTokenProvider? auth = null)
        : base(factory, auth) { }

    public Task<UserAggregate> GetUser(string id)
        => GetAsync<UserJson>($"/api/users/{id}");

    public Task<UserAggregate> CreateUser(object body)
        => PostAsync<UserJson>("/api/users", body);
}
```

### Auth Token Injection
Implement `IAuthTokenProvider` and pass it to the constructor. The base class injects the Bearer token automatically.

### Error Handling
HTTP errors are wrapped in `TiferetException` with `ErrorCodes.HttpRequestFailed`, including method, URL, status code, and response body in the context.

## Related Documentation

- [docs/core/repos.md](../core/repos.md) — repository base class design
- [docs/core/mappers.md](../core/mappers.md) — aggregates and transfer objects
- [docs/core/utils.md](../core/utils.md) — file utilities
