# Utilities – YamlLoader

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

`YamlLoader` extends `FileLoader` for YAML file operations using YamlDotNet.

## Usage

### Loading
```csharp
var loader = YamlLoader.ForReading("app/configs/config.yml");
var data = loader.Load(); // returns Dictionary<object, object>
```

### Saving
```csharp
var loader = YamlLoader.ForWriting("app/configs/config.yml");
loader.Save(data);
```

## Key Features

- `ForReading` / `ForWriting` — static factory methods.
- `Load()` — deserializes YAML content to a raw dictionary.
- `Save(data)` — serializes a dictionary to YAML format.
- Used internally by `YamlRepository` for all configuration file access.

## Related Documentation

- [docs/core/utils.md](../../core/utils.md) — utility design overview
- [docs/core/repos.md](../../core/repos.md) — YAML repositories
