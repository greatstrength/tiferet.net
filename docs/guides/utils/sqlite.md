# Utilities – SqliteClient

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

`SqliteClient` implements `ISqliteService` for SQLite database operations.

## Usage

```csharp
using var client = new SqliteClient("data.db");
client.Execute("CREATE TABLE IF NOT EXISTS items (id TEXT, name TEXT)");
client.Execute("INSERT INTO items VALUES (@id, @name)",
    new Dictionary<string, object> { ["id"] = "1", ["name"] = "Test" });

var results = client.Query("SELECT * FROM items");
```

## Key Features

- Connection lifecycle management.
- `Execute` — run non-query commands.
- `ExecuteScalar` — run scalar queries.
- `Query` — run queries and return results.
- Implements `IDisposable` for automatic connection cleanup.

## Related Documentation

- [docs/core/utils.md](../../core/utils.md) — utility design overview
- [docs/core/interfaces.md](../../core/interfaces.md) — ISqliteService contract
