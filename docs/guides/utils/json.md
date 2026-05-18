# Utilities – JSON

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

The JSON utility layer provides convention-aware serialization infrastructure for REST API integrations.

## JsonLoader
Extends `FileLoader` for JSON file operations using `System.Text.Json`.

```csharp
var loader = JsonLoader.ForReading("data.json");
var obj = loader.Load<MyType>();
```

## JsonSerializerHelper
Static helper for convention-aware JSON serialization/deserialization:

```csharp
var obj = JsonSerializerHelper.Deserialize<UserJson>(jsonString);
var json = JsonSerializerHelper.Serialize(obj);
```

Respects `[JsonNaming]` attributes on the target type.

## JsonNamingAttribute
Class-level attribute specifying the naming convention:

```csharp
[JsonNaming(NamingConvention.SnakeCase)]
public class UserJson : JsonTransferObject<UserAggregate> { ... }
```

## NamingConvention Enum
- `PascalCase` — default .NET naming.
- `CamelCase` — JavaScript/JSON convention.
- `SnakeCase` — Python/REST API convention.

## ConventionNamingResolver
`System.Text.Json` naming policy that resolves the convention from the `[JsonNaming]` attribute.

## Related Documentation

- [docs/core/utils.md](../../core/utils.md) — utility design overview
- [docs/core/mappers.md](../../core/mappers.md) — JsonTransferObject base
