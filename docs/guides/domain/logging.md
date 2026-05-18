# Domain – Logging

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

The Logging domain defines structured logging configuration. Formatters, handlers, and loggers are loaded from YAML and used by `LoggingContext` to build `ILogger` instances via `Microsoft.Extensions.Logging`.

## Domain Objects

### FormatterConfiguration
- `Id` — formatter identifier.
- `Format` — the format string pattern.

### HandlerConfiguration
- `Id` — handler identifier.
- `Type` — handler type (e.g., `"Console"`, `"File"`).
- `FormatterId` — references a `FormatterConfiguration`.
- `Level` — minimum log level.
- `Parameters` — handler-specific parameters.

### LoggerConfiguration
- `Id` — logger identifier.
- `Handlers` — list of handler IDs.
- `Level` — minimum log level.

## Domain Events

- `ListAllLoggingConfigs` — load all formatters, handlers, and loggers.
- `AddFormatter` — create a new formatter.
- `AddHandler` — create a new handler.
- `AddLogger` — create a new logger.
- `RemoveFormatter` / `RemoveHandler` / `RemoveLogger` — delete configurations.

## YAML Configuration

```yaml
logging:
  formatters:
    default:
      Format: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}"
  handlers:
    console:
      Type: Console
      FormatterId: default
      Level: Information
  loggers:
    default:
      Handlers: [console]
      Level: Debug
```

## Related Documentation

- [docs/core/domain.md](../../core/domain.md) — DomainObject base class
- [docs/core/contexts.md](../../core/contexts.md) — LoggingContext
