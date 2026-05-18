# Utilities in Tiferet.NET

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

Utilities provide concrete infrastructure implementations that satisfy Service contracts from `Tiferet.Interfaces`. They form the infrastructure layer bridging abstract service contracts with underlying processes — file I/O, database access, JSON serialization, and type activation.

## Current Utility Classes

### FileLoader
Base utility implementing `IFileService`. Manages file stream lifecycle with validation.

- `ForReading(path)` / `ForWriting(path)` — static factory methods.
- `Open()` / `Close()` — stream lifecycle with double-open guard.
- `VerifyFile(path, fileMode)` — static existence check adapted to read/write mode.
- Implements `IDisposable`.

### YamlLoader
Extends `FileLoader` for YAML file operations using YamlDotNet.

- `ForReading(path)` / `ForWriting(path)` — static factories.
- `Load()` — deserialize YAML to dictionary.
- `Save(data)` — serialize dictionary to YAML.

### JsonLoader
Extends `FileLoader` for JSON operations using `System.Text.Json`.

- `Load<T>()` — deserialize JSON to typed object.
- `Save(data)` — serialize to JSON.

### CsvLoader / CsvDictLoader
CSV file operations for list-based and dictionary-based rows.

- `CsvLoader` — list-based rows with `CsvParser`.
- `CsvDictLoader` — dictionary-based rows with header support.

### CsvParser
Low-level CSV parsing utilities with RFC 4180 compliance.

### SqliteClient
SQLite database operations implementing `ISqliteService`.

- Connection lifecycle management.
- `Execute`, `ExecuteScalar`, `Query` methods.

### ReflectionActivator
Type activation utility for constructing instances from dictionaries:

- `Construct<T>(Dictionary<string, object?> data)` — builds typed instances via reflection, used by `DomainEvent<TParams, TResult>` to construct params records from pipeline data.

### JSON Utilities (`Utilities/Json/`)
Convention-aware JSON serialization infrastructure:

- **`NamingConvention`** — enum: `PascalCase`, `CamelCase`, `SnakeCase`.
- **`JsonNamingAttribute`** — class-level attribute specifying the naming convention.
- **`ConventionNamingResolver`** — `System.Text.Json` naming policy resolver.
- **`JsonSerializerHelper`** — static helper for convention-aware serialization/deserialization.

## Package Layout

```
Tiferet/Utilities/
├── FileLoader.cs           — base file I/O (IFileService)
├── YamlLoader.cs           — YAML read/write (YamlDotNet)
├── JsonLoader.cs           — JSON read/write (System.Text.Json)
├── CsvLoader.cs            — list-based CSV
├── CsvDictLoader.cs        — dict-based CSV
├── CsvParser.cs            — low-level CSV parsing
├── SqliteClient.cs         — SQLite operations (ISqliteService)
├── ReflectionActivator.cs  — dictionary-to-type construction
└── Json/
    ├── NamingConvention.cs
    ├── JsonNamingAttribute.cs
    ├── ConventionNamingResolver.cs
    └── JsonSerializerHelper.cs
```

## Related Documentation

- [docs/core/interfaces.md](interfaces.md) — service contracts that utilities implement
- [docs/core/repos.md](repos.md) — repositories that consume utilities
- [docs/core/code_style.md](code_style.md) — artifact comments and formatting
