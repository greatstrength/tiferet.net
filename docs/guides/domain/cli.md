# Domain – CLI

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

The CLI domain defines command-line interface structure. CLI commands map to feature workflows and are translated to `System.CommandLine` commands by `CliBlueprint`.

## Domain Objects

### CliCommandConfiguration
- `GroupKey` — command group.
- `Key` — command key within the group.
- `Name` — display name.
- `Description` — command description.
- `Arguments` — list of `CliArgumentConfiguration`.

### CliArgumentConfiguration
- `Name` — argument name.
- `Description` — argument description.
- `IsRequired` — whether the argument is required.

## Domain Events

- `ListCliCommands` — list all CLI commands.
- `GetParentArguments` — get root-level arguments.
- `AddCliCommand` — create a new CLI command.
- `AddCliArgument` — add an argument to a command.

## YAML Configuration

```yaml
cli:
  cmds:
    calc:
      add:
        GroupKey: calc
        Key: add
        Description: Adds two numbers.
        Arguments:
          - Name: a
            Description: First number.
          - Name: b
            Description: Second number.
        Name: Add Number Command
```

## Related Documentation

- [docs/core/domain.md](../../core/domain.md) — DomainObject base class
- [docs/core/blueprints.md](../../core/blueprints.md) — CliBlueprint design
