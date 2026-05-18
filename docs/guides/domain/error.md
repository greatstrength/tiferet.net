# Domain – Error

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

The Error domain defines structured error handling with multilingual message support. Error definitions are loaded from YAML and formatted at runtime by `ErrorContext`.

## Domain Objects

### ErrorConfiguration
- `Id` — error identifier (e.g., `"invalid_input"`).
- `Name` — human-readable error name.
- `Message` — list of `ErrorMessageConfiguration` for multilingual support.

### ErrorMessageConfiguration
- `Lang` — language code (e.g., `"en_US"`, `"es_ES"`).
- `Text` — message template with `{}` placeholders.

### ErrorResponse
Runtime response object (not a configuration type):
- `ErrorCode` — the error identifier.
- `Name` — display name.
- `Message` — formatted message.

## Domain Events

- `GetError` — retrieve an error definition by code (includes framework defaults).
- `AddError` — create a new error definition.
- `ListErrors` — list all error definitions.
- `RenameError` — rename an error.
- `SetErrorMessage` — add or update a message for a language.
- `RemoveErrorMessage` — remove a message for a language.
- `RemoveError` — delete an error definition.

## YAML Configuration

```yaml
errors:
  invalid_input:
    Name: Invalid Input
    Message:
      - Lang: en_US
        Text: "Value {} must be a number"
      - Lang: es_ES
        Text: "El valor {} debe ser un número"
```

## Related Documentation

- [docs/core/domain.md](../../core/domain.md) — DomainObject base class
- [docs/core/events.md](../../core/events.md) — domain event patterns
