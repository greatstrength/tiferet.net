# Contributing to Tiferet.NET

Thank you for your interest in contributing to the Tiferet.NET framework! This document outlines the process and expectations for contributing, whether you're fixing a bug, proposing a feature, or improving documentation.

## Getting Started

1. **Fork** the repository and clone your fork locally.
2. Create a new branch from the appropriate base branch (e.g., `v1.x-proto`).
3. Build and run tests:
   ```bash
   dotnet build Tiferet.sln
   dotnet test tests/Tiferet.Tests/Tiferet.Tests.csproj
   ```

## Contribution Workflow

### 1. Open or Claim an Issue

All contributions should be tied to a GitHub issue. If one doesn't exist for the work you'd like to do, open an issue first to discuss the change with maintainers before starting.

### 2. Write a Technical Requirements Document (TRD)

For non-trivial changes (new features, refactors, architectural updates), a **Technical Requirements Document** is required before implementation begins. TRDs ensure clarity, alignment, and traceability across the project.

Key points:
- Follow the standard structure (Overview, Scope, Components Affected, Detailed Requirements, Acceptance Criteria).
- Include code signatures and behavior descriptions where applicable.
- Reference the structured code style conventions described in `AGENTS.md`.

### 3. Implement

- Follow the **Structured Code Style** — artifact comments (`// ***`, `// **`, `// *`), spacing rules, XML doc comments, and snippet conventions are enforced across the codebase.
- One class per file; supplementary records and enums co-located with their owning class.
- Write tests using `xUnit`.

### 4. Commit Hygiene

- Separate functional code changes from documentation, configuration, and packaging into distinct, atomic commits.
- Title commits by scope (e.g., `Events – AddFeature event`, `Docs/Packaging – update guides`).
- Include `Co-Authored-By: <name> <email>` in commit messages when collaborating with AI agents or other contributors.

### 5. Open a Pull Request

- Target the appropriate base branch.
- Reference the GitHub issue in the PR description.
- Ensure all tests pass and the code follows project conventions.
- Keep PRs focused — one logical change per PR.

### 6. Collaboration Report

Upon completion of a story or issue, a **Collaboration Report** is published as a comment on the originating GitHub issue. This report documents the implementation, deviations from the TRD, git state, and a log of key decisions made during development.

## Prototype Branching Conventions

All development happens on **prototype branches** before merging into `main`. Tags and releases on `main` represent the publishable state of the project.

### Prototype Branches

- **Naming**: `v<major>.<minor>-proto` (e.g., `v1.x-proto`).
- **Purpose**: Long-lived development branches where iterative milestone work accumulates.
- **Merge to main**: Squash-merged into `main` when a publishable milestone is reached. A tag is created on `main` to mark the release.

### Worktree Milestone Branches

For iterative milestone work (e.g., beta cycles), use **worktree branches** to isolate each milestone:

- **Naming**: `beta-<N>-proto` (e.g., `beta-6-proto`, `beta-7-proto`).
- **Creation**: `git worktree add <path> -b beta-<N>-proto <prototype-branch>`
- **Workflow**:
  1. Create a worktree branch from the prototype branch.
  2. Develop the milestone (commits, feature work, etc.).
  3. Squash-merge the worktree branch back into the prototype branch.
  4. Remove the worktree and delete the branch after merge.

This keeps the prototype branch clean with one squash commit per milestone while allowing free-form development on the worktree branch.

## Code Style

Tiferet.NET enforces a structured code style across all modules. The essentials:

- **Artifact comments** (`// ***`, `// **`, `// *`) organize code into predictable, machine-readable sections.
- **XML doc comments** on all public types and methods.
- **Commented code snippets** — each logical step is a separate snippet preceded by a descriptive comment.
- **Consistent spacing** — one empty line between sections, comments, and code blocks.

## Reporting Issues

When reporting bugs or requesting features:
- Use the GitHub issue tracker.
- Include a clear description, steps to reproduce (for bugs), and the expected vs. actual behavior.
- Reference relevant files, configurations, or error messages where applicable.

## License

By contributing to Tiferet.NET, you agree that your contributions will be licensed under the [MIT License](LICENSE) that covers the project.
