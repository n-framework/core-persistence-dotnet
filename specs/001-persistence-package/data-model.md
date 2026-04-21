# Data Model: NFramework.Persistence Package

**Feature**: 001-persistence-package
**Date**: 2026-04-20

## Overview

The persistence package provides core data structures and abstractions that applications use to define and work with entities. The data model follows clean architecture principles with clear separation between abstractions (zero dependencies) and implementation (EF Core-specific).

---

## 1. Core Entity Abstractions

### Three-Level Entity Hierarchy

The persistence package provides a modular three-level hierarchy. Choose the base class that matches your needs:

```text
Entity<TId> (Level 1)
    ↓
AuditableEntity<TId> (Level 2)
    ↓
SoftDeletableEntity<TId> (Level 3)
```

---

### `Entity<TId>` (Level 1)

The simplest base class - just identity and concurrency control.

**Properties:**

```csharp
public abstract class Entity<TId>
{
    public TId Id { get; set; }              // Primary key
    public byte[] RowVersion { get; set; }    // Optimistic concurrency token
}
```

**Use when:** You don't need timestamps - read-only entities, lookup tables, reference data, cached values.

---

### `AuditableEntity<TId>` (Level 2)

Adds timestamp tracking to know when entities are created and modified.

**Inherits:** `Entity<TId>`

**Additional Properties:**

```csharp
public abstract class AuditableEntity<TId> : Entity<TId>
{
    public DateTime CreatedAt { get; set; }   // Auto-set on creation
    public DateTime UpdatedAt { get; set; }   // Auto-updated on save
}
```

**Use when:** You care about audit trails but won't soft delete - system logs, audit records, configuration data (permanent deletion).

---

### `SoftDeletableEntity<TId>` (Level 3)

Adds soft delete with both boolean flag (for fast queries) and timestamp (for audit).

**Inherits:** `AuditableEntity<TId>`

**Additional Properties:**

```csharp
public abstract class SoftDeletableEntity<TId> : AuditableEntity<TId>
{
    public bool IsDeleted { get; set; }         // Query optimization - boolean index
    public DateTime? DeletedAt { get; set; }    // Audit trail - when deleted
}
```

**Why both properties:**

| Property | Purpose | Performance |
| -------- | -------- | --------- |
| `IsDeleted` | Query filtering - `WHERE IsDeleted = false` | **FAST** - boolean index |
| `DeletedAt` | Audit trail - "when was this deleted?" | SLOWER - nullable index |

**Use when:** Business entities where you want to recover deleted data or keep a deletion history - users, orders, products, transactions.

---

### Choosing the Right Base Class

| Base Class | Properties | Soft Delete? | Example Use Cases |
| --------- | --------- | ---------- | ------------ |
| `Entity<TId>` | Id, RowVersion | N/A | Reference data, lookup tables, read-only caches, value objects |
| `AuditableEntity<TId>` | + CreatedAt, UpdatedAt | N/A | System logs, audit records, configuration, immutable entities |
| `SoftDeletableEntity<TId>` | + IsDeleted, DeletedAt | Yes | Users, orders, products - business entities with recovery needs |

**What each level gives you:**

```text
Level 1 - Entity<TId>
├── Identity (Id)
└── Concurrency (RowVersion)

Level 2 - AuditableEntity<TId>
├── All from Level 1
└── Timestamps (CreatedAt, UpdatedAt)

Level 3 - SoftDeletableEntity<TId>
├── All from Level 2
└── Soft Delete (IsDeleted, DeletedAt)
```

---

### State Transitions

**All entities (any level):**

```text
[New] → Id set, RowVersion=null
     ↓
[Saved] → RowVersion set by database
     ↓
[Modified] → RowVersion compared on save
     ↓
[Conflict] → DbUpdateConcurrencyException if RowVersion mismatch
```

**Soft-deletable entities only:**

```text
[Active] → IsDeleted=false, DeletedAt=null
     ↓ (Delete called)
[Soft Deleted] → IsDeleted=true, DeletedAt=now
     ↓
[Excluded from Queries] → Global filter uses IsDeleted
     ↓ (IncludeDeleted())
[Visible Again] → Entity included in query results
```

---

## 2. Repository Interfaces

### `IAsyncRepository<TEntity, TId>`

Primary contract for asynchronous data operations.

**Type Parameters:**

- `TEntity`: Entity type, must inherit from `Entity<TId>`
- `TId`: ID type, must be non-nullable value type

**Core Operations:**

| Method | Purpose | Returns |
| ------ | ------ | ----- |
| `AddAsync(TEntity)` | Insert new entity | `Task<TEntity>` |
| `UpdateAsync(TEntity)` | Modify existing entity | `Task` |
| `DeleteAsync(TEntity)` | Remove entity (soft/hard) | `Task` |
| `GetByIdAsync(TId)` | Fetch by ID, includes deleted if specified | `Task<TEntity?>` |
| `GetAllAsync(bool)` | Fetch all or non-deleted, optional includes | `Task<IEnumerable<TEntity>>` |
| `FindAsync(DynamicQuery)` | Query with dynamic filters/sorts/pagination | `Task<IPaginate<TEntity>>` |
| `CountAsync()` | Count entities matching filter | `Task<int>` |
| `BulkAddAsync(IEnumerable)` | Insert multiple entities | `Task<int>` |
| `BulkUpdateAsync(IEnumerable)` | Update multiple entities | `Task<int>` |
| `BulkDeleteAsync(IEnumerable)` | Delete multiple entities | `Task<int>` |

### `IRepository<TEntity, TId>`

Synchronous version of `IAsyncRepository`. Same operations, non-async return types.

### `IQuery<TEntity>`

Read-only query interface. No modification operations.

**Operations:**

- `GetByIdAsync(TId, bool)`
- `GetAllAsync(bool, params Expression<Func<TEntity, object>>[])`
- `FindAsync(DynamicQuery, bool)`
- `CountAsync(Filter?)`
- `RandomAsync(int)`

---

## 3. Pagination Types

### IPaginate<`T`>

Immutable result type representing a page of data with metadata.

**Properties:**

```csharp
public interface IPaginate<out T>
{
    IReadOnlyList<T> Items { get; }        // Current page items
    int Index { get; }                      // Current page index (0-based)
    int Size { get; }                       // Page size
    int Count { get; }                      // Items in current page
    int TotalCount { get; }                 // Total items across all pages
    int TotalPages { get; }                 // Total pages available
    bool HasPrevious { get; }               // Previous page exists
    bool HasNext { get; }                   // Next page exists
}
```

### `Paginate<T>`

Concrete implementation of `IPaginate<T>`.

**Validation:**

- `Index` must be >= 0
- `Size` must be > 0
- `Size` limited to maximum (default: 1000)

---

## 4. Dynamic Query Types

### DynamicQuery

Encapsulates a complete query specification with filters, sorting, and pagination.

**Properties:**

```csharp
public class DynamicQuery
{
    public IList<Filter> Filters { get; set; }      // Filter conditions
    public IList<Sort> Sorts { get; set; }          // Sort specifications
    public int PageIndex { get; set; }              // Page number (0-based)
    public int PageSize { get; set; }               // Items per page
}
```

**Default Values:**

- No filters (returns everything)
- No sorting (database decides order)
- First page (index 0)
- 10 items per page

### Filter

Represents a single filter condition.

**Properties:**

```csharp
public class Filter
{
    public string Field { get; set; }        // Property name on entity
    public string Operator { get; set; }     // Comparison operator
    public object? Value { get; set; }       // Value to compare against
    public bool IsNot { get; set; }          // Negate the condition
}
```

**Valid Operators:**

- Comparison: `eq`, `neq`, `lt`, `lte`, `gt`, `gte`
- Null checks: `is null`, `is not null`
- String operations: `starts with`, `ends with`, `contains`, `does not contain`
- Collections: `in`

### Sort

Represents a sort specification.

**Properties:**

```csharp
public class Sort
{
    public string Field { get; set; }        // Property name to sort by
    public string Direction { get; set; }    // "asc" or "desc"
}
```

---

## 5. EF Core Implementation Types

### EfRepositoryBase<TEntity, TId>

Base repository implementation using EF Core. Partial classes organize operations.

**Dependencies:**

- `DbContext`: Injected via constructor, scoped lifetime
- Convention configuration applied in `OnModelCreating`

**Key Properties:**

- `Table` (or `DbSet<TEntity>`): Direct access to the EF Core DbSet for custom queries

**Virtual Methods** (for extension/override):

- `AddAsync(TEntity, CancellationToken)`
- `UpdateAsync(TEntity, CancellationToken)`
- `DeleteAsync(TEntity, CancellationToken)`
- `CommitAsync(CancellationToken)`

**Using the Table property for custom queries:**

```csharp
public class UserRepository : EfRepositoryBase<User, int>, IUserRepository
{
    // Good: Use Table for custom queries - full EF Core power
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await Table
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    // Good: Complex queries work naturally
    public async Task<IReadOnlyList<User>> GetActiveUsersAsync(DateTime since)
    {
        return await Table
            .Where(u => u.LastLoginAt >= since)
            .OrderByDescending(u => u.LastLoginAt)
            .ToListAsync();
    }
}
```

**Why use Table instead of GetAll():**

| Aspect | `Table` | `GetAll()` |
| ----- | ----- | ----- |
| Type | `DbSet<TEntity>` (EF Core) | `IQueryable<TEntity>` (wrapper) |
| Performance | Direct EF Core, no overhead | Slight wrapper overhead |
| Flexibility | Full EF Core query capabilities | Limited to basic queries |
| Use case | Custom repository methods | General-purpose queries from interface methods |

---

## 6. Configuration Extensions

### ModelBuilderExtensions

Extension methods that apply standard EF Core conventions to your entities.

**What they do:**

| Method | What it configures |
| ----- | --------------- |
| `ApplyTimestampsConfiguration()` | `CreatedAt`, `UpdatedAt` columns with defaults |
| `ApplyOptimisticConcurrencyConfiguration()` | `RowVersion` as a concurrency token |
| `ApplySoftDeleteConfiguration()` | Global query filters for `IsDeleted` |
| `ApplyAllConventions()` | All of the above at once |
| `ApplyConfigurationsFromAssembly()` | Scans for `IEntityTypeConfiguration<T>` classes |

**Convention rules:**

1. `AuditableEntity<TId>` and descendants → Configure `CreatedAt`, `UpdatedAt`
2. `SoftDeletableEntity<TId>` → Configure `IsDeleted`, `DeletedAt`, and global filter
3. All entities → Configure `RowVersion` as concurrency token
4. All `IEntityTypeConfiguration<T>` with parameterless constructors → Applied automatically

---

## 7. Migration Support

### MigrationExtensions

An extension method that keeps your database schema up to date automatically.

```csharp
public static async Task EnsureDatabaseCreated<TContext>(
    this IServiceProvider services,
    CancellationToken cancellationToken = default)
    where TContext : DbContext
```

**How it behaves:**

- In-memory database → Creates the schema
- Relational database → Applies pending migrations
- Checks connectivity first, fails quickly if database is unavailable

---

## 8. Source Generator

### PersistenceGenerator

A Roslyn source generator that finds your repository interfaces and generates dependency injection registration code.

**What it looks for:**

```csharp
// Finds interfaces like this:
public interface IUserRepository : IAsyncRepository<User, int> { }
```

**What it generates:**

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        // ... one for each repository
        return services;
    }
}
```

**Diagnostic messages:**

| Code | When you see it | What it means |
| ---- | --------------- | ----------- |
| PG001 | No implementation found | You have an interface but no class implementing it |
| PG002 | No DbContext constructor | Your implementation needs a constructor that accepts DbContext |
| PG003 | Generic constraints not met | Your type arguments don't satisfy the interface requirements |

---

## Type Constraints Summary

| Type | Constraint | Why |
| ---- | --------- | --- |
| `Entity<TId>` | `TId : struct` | IDs must be value types for performance |
| `IAsyncRepository<TEntity, TId>` | `TEntity : Entity<TId>` | Entities have the base properties |
| `EfRepositoryBase<TEntity, TId>` | `TEntity : class, Entity<TId>` | Entities are reference types with base properties |

---

## Data Validation

### Entity-Level Validation

1. **ID required**: Entities must have a non-default ID before being saved
2. **Timestamps managed**: You don't set `CreatedAt` or `UpdatedAt` - the repository does it
3. **Concurrency checked**: `RowVersion` must match what's in the database on updates
4. **Soft delete maintained**: `IsDeleted` and `DeletedAt` stay synchronized

### Query-Level Validation

1. **Page size**: Must be greater than 0, limited to a maximum
2. **Page index**: Must be 0 or higher
3. **Field names**: Must exist on the entity type
4. **Operators**: Must be from the supported set
5. **Sort direction**: Must be "asc" or "desc"

### Bulk Operation Validation

1. **No nulls allowed**: Collections can't contain null entities
2. **Entities must exist**: Updates fail if the entity isn't found or is already deleted
3. **Fail fast**: Validation happens before any database work begins

---

## Query Execution Flow

```text
You create a DynamicQuery
        ↓
Parse and validate filters
Parse and validate sorts
Validate pagination values
        ↓
Build the IQueryable
Apply global filters (IsDeleted)
Apply dynamic filters
Apply sorting
Apply pagination
        ↓
Generate SQL
Parameterize values (safe from SQL injection)
Execute against database
        ↓
Return IPaginate<T>
With metadata about pages
```

---

## Summary

The data model provides:

1. **Three-level hierarchy** - Choose only what you need (Entity, AuditableEntity, SoftDeletableEntity)
2. **Clear repository contracts** with both async and sync variants
3. **Type-safe dynamic querying** with filter and sort abstractions
4. **Immutable pagination results** with navigation metadata
5. **Convention-based configuration** that eliminates boilerplate
6. **AOT-compatible patterns** throughout (no reflection anywhere)
7. **Performance-optimized** soft delete with boolean `IsDeleted` for fast queries
