# Repository Interface Contracts

**Feature**: 001-persistence-package
**Package**: NFramework.Persistence.Abstractions
**Version**: 1.0.0

## Purpose

This document describes the public API contracts for repository interfaces. Your applications will depend on these interfaces, so any breaking changes here require a major version bump. These contracts are the foundation - the EF Core package provides the implementations.

---

## `IAsyncRepository<TEntity, TId>`

Contract for asynchronous repository operations.

### Type Parameters

| Parameter | Constraint | Description |
| ---------- | ---------- | ----------- |
| `TEntity` | `Entity<TId>` | Entity type managed by repository |
| `TId` | `struct` | Primary key type (int, long, Guid, etc.) |

### Methods

#### AddAsync

```csharp
Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
```

**Behavior:**

- Sets `entity.CreatedAt` to `DateTime.UtcNow` if not already set
- Sets `entity.UpdatedAt` to `DateTime.UtcNow`
- Adds entity to underlying data store
- Returns the entity with any database-generated values populated

**Throws:**

- `ArgumentNullException`: If `entity` is null
- `InvalidOperationException`: If entity with same ID already exists

**Contract Guarantee:** After successful call, entity is persisted and retrievable via `GetByIdAsync`.

---

#### UpdateAsync

```csharp
Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
```

**Behavior:**

- Updates `entity.UpdatedAt` to `DateTime.UtcNow`
- Compares `entity.RowVersion` with stored value for optimistic concurrency
- Persists changes to underlying data store

**Throws:**

- `ArgumentNullException`: If `entity` is null
- `DbUpdateConcurrencyException`: If `RowVersion` mismatch (concurrent modification detected)
- `InvalidOperationException`: If entity does not exist or is soft-deleted

**Contract Guarantee:** After successful call, stored entity matches the provided entity's state.

---

#### DeleteAsync

```csharp
Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
```

**Behavior:**

- If entity is of type `SoftDeletableEntity<TId>`, performs soft delete:
  - Sets `IsDeleted = true`
  - Sets `DeletedAt = DateTime.UtcNow`
  - Cascades soft delete to related entities
- Otherwise, performs hard delete (permanent removal)
- Persists changes to underlying data store

**Throws:**

- `ArgumentNullException`: If `entity` is null
- `DbUpdateConcurrencyException`: If concurrent modification detected

**Contract Guarantee:** After successful call, entity is not returned in normal queries.

---

#### GetByIdAsync

```csharp
Task<TEntity?> GetByIdAsync(TId id, bool includeDeleted = false, CancellationToken cancellationToken = default);
```

**Behavior:**

- Fetches entity with matching ID
- If `includeDeleted` is false and entity is soft-deleted, returns null
- If `includeDeleted` is true, returns entity regardless of deletion status

**Returns:** Entity if found, null otherwise

**Throws:**

- `ArgumentNullException`: If `id` is default value (e.g., 0, empty Guid)

**Contract Guarantee:** If non-null returned, entity represents current persisted state.

---

#### GetAllAsync

```csharp
Task<IReadOnlyList<TEntity>> GetAllAsync(
    bool includeDeleted = false,
    params Expression<Func<TEntity, object>>[] includes);
```

**Behavior:**

- Fetches all entities matching deletion status
- Eagerly loads related entities specified in `includes`
- Does not apply pagination

**Returns:** Read-only list of entities (empty if none)

**Contract Guarantee:** Returns all non-deleted entities (or all if `includeDeleted` true).

---

#### FindAsync

```csharp
Task<IPaginate<TEntity>> FindAsync(
    DynamicQuery query,
    bool includeDeleted = false,
    CancellationToken cancellationToken = default);
```

**Behavior:**

- Applies filters from `query.Filters`
- Applies sorts from `query.Sorts`
- Paginates according to `query.PageIndex` and `query.PageSize`
- Returns paginated result with metadata

**Returns:** `IPaginate<TEntity>` with current page and navigation metadata

**Throws:**

- `ArgumentNullException`: If `query` is null
- `ArgumentException`: If field names don't exist on entity type
- `ArgumentException`: If operators are invalid
- `ArgumentException`: If pagination values are invalid

**Contract Guarantee:** Returned page contains only entities matching all filters, sorted correctly.

---

#### CountAsync

```csharp
Task<int> CountAsync(Filter? filter = null, CancellationToken cancellationToken = default);
```

**Behavior:**

- Counts entities matching optional filter
- Respects soft delete (excludes deleted unless filter explicitly includes them)

**Returns:** Count of matching entities

**Contract Guarantee:** Count represents current persisted state.

---

#### BulkAddAsync

```csharp
Task<int> BulkAddAsync(
    IEnumerable<TEntity> entities,
    CancellationToken cancellationToken = default);
```

**Behavior:**

- Validates all entities are non-null before processing
- Sets `CreatedAt` and `UpdatedAt` for all entities
- Processes in batches (default: 1000)
- Commits each batch to database

**Returns:** Number of entities successfully added

**Throws:**

- `ArgumentNullException`: If `entities` is null
- `ArgumentException`: If collection contains null entities
- `InvalidOperationException`: If any entity already exists

**Contract Guarantee:** All entities in collection are persisted on successful return.

---

#### BulkUpdateAsync

```csharp
Task<int> BulkUpdateAsync(
    IEnumerable<TEntity> entities,
    CancellationToken cancellationToken = default);
```

**Behavior:**

- Validates all entities are non-null before processing
- Updates `UpdatedAt` for all entities
- Checks `RowVersion` for each entity
- Processes in batches

**Returns:** Number of entities successfully updated

**Throws:**

- `ArgumentNullException`: If `entities` is null
- `ArgumentException`: If collection contains null entities
- `DbUpdateConcurrencyException`: If any entity has `RowVersion` mismatch

**Contract Guarantee:** All entities in collection are updated on successful return.

---

#### BulkDeleteAsync

```csharp
Task<int> BulkDeleteAsync(
    IEnumerable<TEntity> entities,
    CancellationToken cancellationToken = default);
```

**Behavior:**

- Validates all entities are non-null before processing
- Performs soft or hard delete based on entity type
- Cascades soft deletes to related entities
- Processes in batches

**Returns:** Number of entities successfully deleted

**Throws:**

- `ArgumentNullException`: If `entities` is null
- `ArgumentException`: If collection contains null entities

**Contract Guarantee:** All entities in collection are deleted on successful return.

---

## `IRepository<TEntity, TId>`

Synchronous version of `IAsyncRepository`. Same method signatures without `Task` wrapper and `CancellationToken`.

**Additional Contract Guarantee:** All operations complete synchronously before returning. May block calling thread.

---

## `IQuery<TEntity>`

Read-only query contract. No modification methods.

### API Methods

#### GetByIdAsync (IQuery)

Same as `IAsyncRepository.GetByIdAsync`.

#### GetAllAsync (IQuery)

Same as `IAsyncRepository.GetAllAsync`.

#### FindAsync (IQuery)

Same as `IAsyncRepository.FindAsync`.

#### CountAsync (IQuery)

Same as `IAsyncRepository.CountAsync`.

#### RandomAsync

```csharp
Task<IReadOnlyList<TEntity>> RandomAsync(
    int count,
    CancellationToken cancellationToken = default);
```

**Behavior:**

- Randomly selects `count` entities from data store
- Uses database-provided random selection if available
- Falls back to client-side random if necessary

**Returns:** List of randomly selected entities (size ≤ `count`)

**Throws:**

- `ArgumentException`: If `count` < 1

**Contract Guarantee:** Returned entities are randomly selected, not ordered.

---

## `IPaginate<T>`

Immutable pagination result contract.

### Properties

| Property | Type | Description |
| ---------- | ---------- | ----------- |
| `Items` | `IReadOnlyList<T>` | Items in current page |
| `Index` | `int` | Zero-based page index |
| `Size` | `int` | Page size (items per page) |
| `Count` | `int` | Number of items in current page |
| `TotalCount` | `int` | Total items across all pages |
| `TotalPages` | `int` | Total number of pages |
| `HasPrevious` | `bool` | True if a previous page exists |
| `HasNext` | `bool` | True if a next page exists |

**Contract Guarantee:** All properties are immutable. `Items.Count` equals `Count`. `TotalPages` equals `(int)Math.Ceiling((double)TotalCount / Size)`.

---

## Versioning Policy

- **Major** (X.0.0): Breaking changes to any interface signature or behavior contract
- **Minor** (1.X.0): New methods added to existing interfaces (backward compatible)
- **Patch** (1.0.X): Bug fixes, internal implementation changes

---

## Implementation Requirements

Any implementation of these interfaces MUST:

1. **Throw documented exceptions** for invalid inputs
2. **Respect cancellation tokens** for async operations
3. **Handle concurrency conflicts** with proper exceptions
4. **Maintain transaction consistency** across bulk operations
5. **Validate before processing** (fail-fast on invalid input)
