# Structured Code Style in Tiferet.NET

**Project:** Tiferet.NET Framework
**Repository:** https://github.com/greatstrength/tiferet.net

## Overview

The Tiferet.NET framework enforces a structured code style to ensure consistency, readability, extensibility, and AI-parsability across all components. This style relies on **artifact comments** for hierarchical organization and strict formatting conventions for XML doc comments, parameters, snippets, and spacing.

This document defines the required code style for all classes in the `Tiferet` package and serves as a guide for application-level code.

## Artifact Comments: Hierarchy and Purpose

Artifact comments provide a predictable, machine-readable structure that organizes code into clear sections and sub-sections. In C#, artifact comments use `//` with the same `***`, `**`, `*` hierarchy as the Python framework.

### Top-Level (`// ***`)
Denotes major module sections:
- `// *** imports` — using directives.
- `// *** events`, `// *** contexts`, `// *** interfaces`, `// *** mappers`, `// *** repos`, `// *** utils` — component groups.

**Spacing**: One empty line between top-level comment and first mid-level comment.

### Mid-Level (`// **`)
Specifies categories or individual components:
- For imports: `// ** core`, `// ** infra`, `// ** app`.
- For components: `// ** event: <name>`, `// ** context: <name>`, etc.

**Spacing**: One empty line between mid-level comments.

### Low-Level (`// *`)
Defines subcomponents within a class:
- `// * attribute: <name>` — instance fields/properties.
- `// * init` — constructor.
- `// * method: <name>` — instance methods.
- `// * method: <name> (static)` — static methods.

**Spacing**: One empty line between low-level comments and code blocks.

## Code Formatting Conventions

### XML Documentation Comments
- Use standard `<summary>`, `<param>`, `<typeparam>`, `<returns>` tags.
- All public types and members must have XML doc comments.
- One empty line after the closing `</summary>` tag before the first code line.

### Naming
- Classes and methods: `PascalCase`.
- Parameters: `camelCase`.
- Constants: `PascalCase`.
- Private fields: `_camelCase` prefix.

### Code Snippets
- Each logical step is a separate snippet.
- Precede with a 1–2 line comment describing intent.
- One empty line between snippets.

**Example of code snippets within a method** (`FeatureContext.LoadFeature`):
```csharp
// * method: load_feature
public FeatureConfiguration LoadFeature(string featureId)
{
    // Try cache first.
    var cached = _cache.Get<FeatureConfiguration>(featureId);
    if (cached is not null)
        return cached;

    // Load via the GetFeature event.
    var feature = _getFeatureEvent.Execute(new GetFeatureParams(featureId));

    // Cache and return.
    _cache.Set(featureId, feature);
    return feature;
}
```

### Spacing Rules
- One empty line between:
  - Top-level sections and first mid-level.
  - Mid-level comments.
  - Low-level comments and code.
  - Code snippets within a method.
  - Methods/properties within a class.

## One Class Per File

All types in Tiferet.NET follow the **one class per file** rule. Supplementary records (e.g., params records) and enums may be co-located with their owning class when they are tightly coupled and not reused elsewhere.

## Record vs. Class Usage

- **Domain objects** — `abstract record DomainObject` and all domain configuration types are C# records, providing value equality and `with` expression support.
- **Aggregates** — `abstract record Aggregate` and `abstract record Aggregate<TDomain>`, wrapping domain records with mutation logic.
- **Events** — `abstract class DomainEvent` and `abstract class AsyncDomainEvent` are classes, as they carry mutable service dependencies and behavior.
- **Contexts** — Classes with constructor-injected dependencies.
- **Utilities** — Classes implementing `IDisposable` where appropriate.
- **Transfer objects** — Classes with mutable properties for deserialization.

## Namespace Organization

Namespaces map directly to folder structure:
- `Tiferet.Domain` — domain records.
- `Tiferet.Domain.App`, `Tiferet.Domain.Error`, etc. — domain subnamespaces.
- `Tiferet.Events` — domain event base classes.
- `Tiferet.Events.App`, `Tiferet.Events.Error`, etc. — domain event modules.
- `Tiferet.Interfaces` — service contracts (flat namespace).
- `Tiferet.Mappers` — aggregate and transfer object base classes.
- `Tiferet.Mappers.App`, `Tiferet.Mappers.Error`, etc. — domain mapper modules.
- `Tiferet.Contexts` — runtime orchestration.
- `Tiferet.Repositories` — YAML and HTTP repository implementations.
- `Tiferet.Utilities` — infrastructure utilities.
- `Tiferet.Blueprints` — application bootstrapping.

## Best Practices Summary

- Use artifact comments consistently.
- Explicitly mark static methods with `(static)` in artifact comments.
- Write XML doc comments for all public members.
- Break methods into commented snippets.
- Maintain consistent spacing.
- Follow the one-class-per-file convention.

These practices ensure Tiferet.NET code remains consistent, maintainable, and AI-friendly.
