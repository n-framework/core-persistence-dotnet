# Implementation Plan: NFramework.Persistence Package

**Branch**: `feature/001-persistence-package` | **Date**: 2026-04-20 | **Spec**: [spec.md](./spec.md)

## Summary

We're building a complete persistence package for the NFramework with two separate NuGet packages. The abstractions package provides zero-dependency interfaces and base classes with a three-level hierarchy for maximum flexibility. The EF Core package brings everything to life with database integration, convention-based configuration, and dynamic querying. Everything follows clean architecture principles and is fully compatible with Native AOT through explicit registration patterns.

## Technical Context

**Language/Version**: C# 14/15 with .NET 11

**Primary Dependencies**:

- Abstractions package: Zero external dependencies (pure interfaces and base classes)
- EF Core package: Microsoft.EntityFrameworkCore 9.0+, System.Linq.Dynamic.Core for dynamic queries, Microsoft.Extensions.DependencyInjection.Abstractions
- Testing: Microsoft.EntityFrameworkCore.InMemory for fast tests, xUnit as the test framework

**Storage**: Any relational database through EF Core (SQL Server, PostgreSQL, SQLite, etc.) - we stay provider-agnostic so applications choose their database

**Testing**: xUnit runs the show, with EF Core's in-memory provider keeping unit tests fast and isolated

**Target Platform**: .NET 11+ across Windows, Linux, and macOS, with full Native AOT support

**Project Type**: Two-package library (NFramework.Persistence.Abstractions, NFramework.Persistence.EfCore)

**Performance Goals**:

- Single-entity operations: under 10ms
- Pagination queries: under 100ms for 1000 records
- Bulk operations: under 5 seconds for 1000 entities
- Unit tests: each runs in under 100ms with in-memory database

**Constraints**:

- Must work with Native AOT (no reflection anywhere)
- Abstractions can't reference any specific database technology
- Explicit registration must be used to ensure trimmability
- Follow clean architecture - abstractions know nothing about implementation

**Scale/Scope**:

- Framework-level package used across many applications
- Two distinct NuGet packages, each with a clear job
- About 20 public interfaces/classes in abstractions
- About 50 public APIs in the EF Core implementation

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Notes |
| ----- | ------ | ---- |
| I. Single-Step Build And Test | ✅ PASS | `dotnet build` and `dotnet test` will work from solution root |
| II. CLI I/O And Exit Codes | ⚪ N/A | Not a CLI tool - library package |
| III. No Suppression | ✅ PASS | Will fail build on compiler warnings, never suppress test failures |
| IV. Deterministic Tests | ✅ PASS | Unit tests use in-memory database; integration tests isolated and labeled |
| V. Documentation Is Part Of Delivery | ✅ PASS | Will include quickstart.md with working examples |

All applicable gates pass. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/001-persistence-package/
├── plan.md              # This file
├── research.md          # Technology research and decisions
├── data-model.md        # Entity and type definitions
├── quickstart.md        # Getting started tutorial
├── contracts/           # API contracts
│   └── repositories.md  # Repository interface contracts
└── tasks.md             # Task breakdown (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── NFramework.Persistence.Abstractions/       # Zero-dependency abstractions
│   ├── Features/
│   │   ├── Repositories/
│   │   │   ├── IAsyncRepository.cs
│   │   │   ├── IRepository.cs
│   │   │   └── IQuery.cs
│   │   ├── Entities/
│   │   │   ├── Entity.cs
│   │   │   ├── AuditableEntity.cs
│   │   │   └── SoftDeletableEntity.cs
│   │   ├── Pagination/
│   │   │   ├── IPaginate.cs
│   │   │   └── Paginate.cs
│   │   └── Dynamic/
│   │       ├── DynamicQuery.cs
│   │       ├── Filter.cs
│   │       └── Sort.cs
│   └── Shared/
│
├── NFramework.Persistence.EfCore/            # EF Core implementation
│   ├── Features/
│   │   ├── Repositories/
│   │   │   ├── EfRepositoryBase.cs
│   │   │   ├── EfRepositoryBase.Create.cs
│   │   │   ├── EfRepositoryBase.Read.cs
│   │   │   ├── EfRepositoryBase.Update.cs
│   │   │   └── EfRepositoryBase.Delete.cs
│   │   ├── Configuration/
│   │   │   └── ModelBuilderExtensions.cs
│   │   ├── Paging/
│   │   │   └── IQueryablePaginateExtensions.cs
│   │   ├── Dynamic/
│   │   │   └── IQueryableDynamicFilterExtensions.cs
│   │   └── Migration/
│   │       └── DatabaseFacadeExtensions.cs
│   └── Shared/
│       ├── Contexts/
│       │   └── BaseDbContext.cs
│       └── Interceptors/
│           └──## Phase 4: Final Polish & Release [x]

1.  **Documentation**:
    *   [x] Authored package-level READMEs.
    *   [x] Updated architectural specs.
2.  **Stability**:
    *   [x] Implemented result set safety guards (`MaxResultSetSize`).
    *   [x] Scaffolded and expanded performance benchmarks.
3.  **Release Preparation**:
    *   [x] Finalized NuGet metadata in projects.
    *   [x] Synchronized task roadmap.
    │
    └── NFramework.Persistence.EfCore.Tests/         # Integration tests
        └── Features/
            ├── Repositories/
            ├── Configuration/
            └── Dynamic/
```

**Structure Decision**: We use feature-based organization with `Features/` folders keeping related functionality together. Each feature has its own folder (Repositories, Configuration, etc.) making it easy to find and navigate code. The `Shared/` folder holds things that don't belong to a specific feature like base contexts and interceptors. This mirrors the clean architecture approach used in the starter project and keeps large codebases organized.

## Complexity Tracking

> No constitution violations to justify - this section intentionally left empty.
