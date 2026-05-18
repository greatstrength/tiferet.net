# Utilities – CSV

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

CSV utilities provide list-based and dictionary-based CSV file operations.

## CsvLoader
List-based CSV rows using `CsvParser`:

```csharp
var loader = new CsvLoader("data.csv");
var rows = loader.ReadAll(); // List<List<string>>
```

## CsvDictLoader
Dictionary-based CSV rows with header support:

```csharp
var loader = new CsvDictLoader("data.csv", fieldNames: ["Name", "Age"]);
var rows = loader.ReadAll(); // List<Dictionary<string, string>>
```

## CsvParser
Low-level RFC 4180-compliant CSV parsing. Handles quoted fields, embedded commas, and newlines.

## Related Documentation

- [docs/core/utils.md](../../core/utils.md) — utility design overview
