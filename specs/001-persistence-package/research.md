# Research: NFramework.Persistence Package

**Feature**: 001-persistence-package
**Date**: 2026-04-20
**Status**: Complete

## Overview

This document captures research findings for implementing the NFramework.Persistence package, a three-package solution providing repository abstractions, Entity Framework Core implementation, and Roslyn source generator for automatic DI registration - all with Native AOT compatibility.

---

## 1. Entity Framework Core Version and API Surface

### Decision (1)

Use **Entity Framework Core 9.0+** as the ORM foundation.

### Rationale (1)

- EF Core 9 is the latest LTS release with mature tooling
- Native AOT support significantly improved in EF Core 9
- Includes all required features: lazy loading, eager loading, change tracking, migrations
- Strong community support and Microsoft backing
- Compatible with .NET 11 target framework

### Alternatives Considered (1)

- **Dapper**: Too lightweight, would require building our own change tracking and migration system
- **NHibernate**: Heavy, complex, less modern API surface
- **Marten (PostgreSQL)**: Database-specific, contradicts provider-agnostic requirement

### Key APIs to Use

- `DbContext` with `Set<TEntity>()` for repository base
- `IQueryable` for dynamic querying
- `ChangeTracker` for detecting concurrent modifications
- `ModelBuilder` for convention-based configuration
- `IMigrator` for programmatic migration application
- `SaveChangesInterceptor` for cross-cutting concerns

---

## 2. Native AOT Compatibility Strategy

### Decision (2)

Design **zero-reflection architecture** with trimmable code paths.

### Rationale (2)

- Native AOT fails with reflection-heavy patterns
- Reflection prevents IL trimming and increases binary size
- Compile-time patterns are more performant and type-safe
- Aligns with framework's AOT-first philosophy

### Implementation Patterns (2)

1. **No reflection-based activation**: Use `new()` constraint or factory delegates
2. **Avoid `GetType()` and `typeof()` runtime lookups**: Use generic type parameters
3. **No dynamic**: All queries compile with `System.Linq.Dynamic.Core` (compiles expressions, not reflection)
4. **Source generator for DI**: Emit concrete registration code instead of scanning assemblies
5. **Trim-friendly attributes**: Use `DynamicallyAccessedMembers` where runtime access is unavoidable

### Alternatives Considered (2)

- **Runtime assembly scanning**: Rejected - incompatible with AOT trimming
- `Activator.CreateInstance`: Rejected - uses reflection
- **Reflection emit**: Rejected - not AOT-compatible

---

## 3. Repository Pattern Implementation Approach

### Decision (3)

Use **generic repository base class** with partial classes for operation organization.

### Rationale (3)

- Partial classes keep file sizes manageable while maintaining single class semantics
- Generic constraints ensure type safety at compile time
- Virtual methods enable extension via decoration/inheritance
- Scoped lifetime aligns with DbContext lifetime

### Structure (3)

```csharp
public abstract partial class EfRepositoryBase<TEntity, TId>
    : IAsyncRepository<TEntity, TId>, IRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    // EfRepositoryBase.Create.cs
    // EfRepositoryBase.Read.cs
    // EfRepositoryBase.Update.cs
    // EfRepositoryBase.Delete.cs
}
```

### Alternatives Considered (3)

- **Interface-only pattern**: Would require every entity to have custom implementation - too much boilerplate
- **Specification pattern**: Adds complexity for limited benefit - dynamic queries cover most use cases
- **Unit of Work pattern**: DbContext already implements this - redundant abstraction

---

## 4. Optimistic Concurrency Strategy

### Decision (4)

Support **both RowVersion (timestamp) and UpdatedAt** concurrency tokens.

### Rationale (4)

- `RowVersion` (rowversion/timestamp in SQL) is database-generated and foolproof
- `UpdatedAt` provides application-level concurrency and user-readable timestamps
- Supporting both accommodates different database capabilities
- Dual-token approach provides defense in depth

### Implementation (4)

- Mark properties as `[Timestamp]` or configure as `IsRowVersion()`
- Use `DbUpdateConcurrencyException` to detect conflicts
- Include current and database values in error details for conflict resolution

---

## 5. Soft Delete Cascade Implementation

### Decision (5)

Use **global query filters** combined with **navigation property traversal**.

### Rationale (5)

- Global filters automatically exclude soft-deleted entities from all queries
- Cascade logic runs on save to mark related entities
- Explicit `IncludeDeleted()` bypass when needed

### Implementation Pattern (5)

```csharp
// Global filter in model configuration
modelBuilder.Entity<TEntity>()
    .HasQueryFilter(e => !e.IsDeleted || _includeDeleted);

// Cascade on SaveChanges via interceptor
foreach (var entry in ChangeTracker.Entries())
{
    if (entry.State == EntityState.Deleted && entry.Entity is SoftDeletableEntity<TId>)
    {
        // Cascade to related entities
    }
}
```

---

## 6. Dynamic Querying Approach

### Decision (6)

Use **System.Linq.Dynamic.Core** library for expression building.

### Rationale (6)

- Mature, battle-tested library
- Compiles string-based queries to LINQ expressions at runtime
- Supports all comparison, logical, and string operators
- Parameterized queries prevent SQL injection
- Compatible with EF Core's `IQueryable`

### Alternatives Considered (6)

- **Expression trees manually built**: Too verbose for dynamic scenarios
- **Raw SQL**: Loses provider abstraction, harder to compose
- **Specification pattern**: Too rigid for truly dynamic queries

### Supported Operators (6)

- Comparison: `eq`, `neq`, `lt`, `lte`, `gt`, `gte`
- Null checks: `isnull`, `isnotnull`
- String: `startswith`, `endswith`, `contains`, `doesnotcontain`
- Collections: `in`
- Logical: `and`, `or`
- Sorting: `asc`, `desc`

---

## 7. Bulk Operations Strategy

### Decision (7)

Implement **in-memory batching** using standard EF Core APIs.

### Rationale (7)

- Simplicity: Uses existing `AddRange`, `UpdateRange`, `RemoveRange`
- Sufficient performance: EF Core 9 optimizes batch operations
- No additional dependencies
- Predictable behavior with change tracking

### Alternatives Considered (7)

- **EFCore.BulkExtensions**: Additional dependency, AOT compatibility unclear
- **Raw SQL bulk insert**: Loses change tracking, harder to maintain

### Implementation (7)

- Process entities in configurable batch sizes (default: 1000)
- Use `SaveChanges()` after each batch
- Validate all entities before processing (fail-fast)

---

## 8. Source Generator Implementation

### Decision (8)

Use **Roslyn IncrementalGenerator** API with syntax receiver pattern.

### Rationale (8)

- Incremental generators cache results and only re-run on changes
- Syntax receiver efficiently finds repository interface declarations
- Generates concrete DI registration code at compile time
- Zero runtime overhead
- Fully AOT-compatible

### Detection Pattern (8)

```csharp
// Find interfaces inheriting from IAsyncRepository<,>
interfaceDeclaration.Syntax.Kind()
    .IsInterfaceDeclaration()
    && HasBaseType("IAsyncRepository")
```

### Generation Pattern (8)

```csharp
// Emit registration extension method
public static void AddPersistenceRepositories(
    this IServiceCollection services)
{
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IOrderRepository, OrderRepository>();
    // ...
}
```

### Diagnostics (8)

- `PG001`: Repository interface not found
- `PG002`: Implementation class missing
- `PG003`: Generic constraints not satisfied

---

## 9. Testing Strategy

### Decision (9)

**Two-tier testing**: Unit tests with fakes, integration tests with in-memory EF Core.

### Rationale (9)

- Unit tests verify abstractions without dependencies (fastest)
- In-memory EF Core tests verify repository logic (fast, no external DB)
- Consider SQLite integration tests for complete EF Core provider coverage

### Test Organization (9)

```text
NFramework.Persistence.Tests/
├── Unit tests for interfaces, pagination, dynamic types

NFramework.Persistence.EfCore.Tests/
├── Repository CRUD with InMemoryContext
├── Dynamic queries with InMemoryContext
├── Concurrency with mocked DbUpdateConcurrencyException
├── Bulk operations with InMemoryContext
```

### Golden File Testing for Generator

- Capture expected generated output as `.cs` files
- Compare actual generator output against golden files
- Run in CI to detect breaking changes

---

## 10. Configuration Extensions Design

### Decision (10)

Provide **model builder extension methods** for conventions.

### Rationale (10)

- Convention-based configuration reduces boilerplate
- Extensions are opt-in via `ApplyConventions(this ModelBuilder)`
- Assembly scanning auto-discovers `IEntityTypeConfiguration<T>` classes

### Conventions to Apply (10)

1. `Entity<TId>` → configure `Id` as primary key
2. `CreatedAt`, `UpdatedAt` → configure as timestamp columns with defaults
3. `RowVersion` → configure as concurrency token
4. `IsDeleted` → configure global query filter
5. Scan assembly for `IEntityTypeConfiguration<T>` with parameterless constructors

---

## 11. Migration Applier Design

### Decision (11)

**Extension method** that checks provider and applies migrations or creates schema.

### Rationale (11)

- Single entry point for application startup
- Handles both in-memory (schema creation) and relational (migrations)
- Gracefully handles already-migrated databases

### Implementation (11)

```csharp
public static async Task EnsureDatabaseCreated(this IHost host)
{
    var context = host.Services.GetRequiredService<MyDbContext>();
    if (context.Database.IsInMemory())
    {
        await context.Database.EnsureCreatedAsync();
    }
    else
    {
        await context.Database.MigrateAsync();
    }
}
```

---

## 12. Observability Extension Points

### Decision (12)

**Virtual methods and interceptor hooks** for application-level observability.

### Rationale (12)

- Applications have diverse logging/monitoring needs
- Built-in instrumentation creates dependency lock-in
- Virtual methods enable decoration without source changes
- EF Core interceptors provide hooks for all operations

### Extension Points (12)

1. Virtual repository methods → can override for logging
2. `SaveChangesInterceptor` → applications can register custom interceptors
3. Diagnostic events for critical errors only (concurrency, connection failures)

---

## Summary of Key Decisions

| Area | Decision | Primary Benefit |
| ---- | -------- | ------------- |
| ORM | EF Core 9.0+ | Mature, AOT-compatible, full-featured |
| AOT | Zero-reflection architecture | Trimmable, fast startup, small binary |
| Repository | Generic base with partial classes | Type-safe, organized, testable |
| Concurrency | RowVersion + UpdatedAt | Database and application-level tokens |
| Soft Delete | Global query filters | Automatic exclusion, easy bypass |
| Dynamic Queries | System.Linq.Dynamic.Core | Runtime flexibility, parameterized |
| Bulk Ops | In-memory batching | Simple, predictable, no new deps |
| DI Registration | Roslyn source generator | Zero runtime overhead, AOT-safe |
| Testing | Unit + in-memory EF Core | Fast, isolated, reliable |

All decisions align with the framework's clean architecture principles and Native AOT requirements.
