# Domain – Feature

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

The Feature domain defines workflow configurations. A feature is a named sequence of steps (domain events) that are executed sequentially by `FeatureContext`.

## Domain Objects

### FeatureConfiguration
- `Id` — composite identifier (`group.feature_key`).
- `Name` — display name.
- `Description` — optional description.
- `Steps` — list of `FeatureStepConfiguration`.
- `Flags` — optional flags for dependency resolution.

### FeatureStepConfiguration
- `ServiceId` — references a service in `container.yml`.
- `Name` — step display name.
- `Parameters` — static parameters merged with request data.
- `DataKey` — optional key to store step result in request data.
- `Condition` — optional boolean expression for conditional execution.
- `PassOnError` — if true, swallow errors and set result to null.
- `Flags` — step-level dependency resolution flags.

### FeatureEventConfiguration
Extended step configuration with event-specific metadata.

## Domain Events

- `GetFeature` — retrieve a feature by ID.
- `AddFeature` — create a new feature workflow.
- `UpdateFeature` — update feature metadata.
- `AddFeatureStep` — add a step to a feature.
- `RemoveFeatureStep` — remove a step.
- `ReorderFeatureStep` — change step order.
- `ListFeatures` — list all features.
- `RemoveFeature` — delete a feature.

## YAML Configuration

```yaml
features:
  calc:
    add:
      Name: Add Number
      Steps:
        - ServiceId: add_number_event
          Name: Add A and B
    sqrt:
      Name: Square Root
      Steps:
        - ServiceId: exponentiate_event
          Name: Calculate square root
          Parameters:
            B: "0.5"
```

## Related Documentation

- [docs/core/domain.md](../../core/domain.md) — DomainObject base class
- [docs/guides/contexts.md](../contexts.md) — feature execution pipeline
