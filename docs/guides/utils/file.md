# Utilities – FileLoader

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

`FileLoader` is the base utility for file stream operations. It implements `IFileService` and provides path management, mode validation, and lifecycle management.

## Usage

### Reading
```csharp
var loader = FileLoader.ForReading("path/to/file.txt");
var stream = loader.Open();
// ... use stream ...
loader.Close();
```

### Writing
```csharp
var loader = FileLoader.ForWriting("path/to/output.txt");
var stream = loader.Open();
// ... write to stream ...
loader.Close();
```

### Static Verification
```csharp
FileLoader.VerifyFile("path/to/file.txt", FileMode.Open);
```

## Key Features

- `ForReading` / `ForWriting` — static factory methods.
- `Open()` — opens the file stream with double-open guard.
- `Close()` — closes and disposes the stream.
- `VerifyFile` — static existence check adapted to read/write mode.
- Implements `IDisposable` for automatic cleanup.

## Error Handling

- `ErrorCodes.FileNotFound` — file or parent directory does not exist.
- `ErrorCodes.FileAlreadyOpen` — attempt to open an already-open stream.

## Related Documentation

- [docs/core/utils.md](../../core/utils.md) — utility design overview
