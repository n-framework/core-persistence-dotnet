# Feature Specification: NFramework.Persistence Package

## User Scenarios & Testing

### User Story 1 - Create and Register Repositories (Priority: P1)

As an application developer, I want to define repository interfaces and register them explicitly in my dependency injection configuration so that I can perform database operations with full control over my service graph.

**Why this priority**: Repositories are the primary entry point for data access. Explicit registration ensures total transparency and avoids "magic" behavior that can complicate debugging and Native AOT compilation.

**Independent Test**: Create a repository interface and implementation, register it in the DI container, and verify it can be resolved and used for data access.

**Acceptance Scenarios**:

1. **Given** a repository interface inheriting from `IAsyncRepository<TEntity, TId>`, **When** registered in the DI container, **Then** it can be injected into high-level services.
2. **Given** a repository implementation, **When** registered as Scoped, **Then** it correctly shares the DbContext lifetime within the same scope.
3. **Given** a Native AOT build, **When** the application is compiled, **Then** the explicit registrations work without runtime reflection.

---

### User Story 2 - Query Entities with Pagination (Priority: P1)

As an application developer, I want to query entities with pagination so that I can display large datasets in pages without loading everything into memory at once.

**Why this priority**: Displaying lists of data is one of the most common application patterns. Without pagination, applications become slow and consume excessive memory.

**Independent Test**: Query a dataset with thousands of records and verify that requesting a specific page returns only that page's worth of data with accurate metadata about total items and page count.

**Acceptance Scenarios**:

1. **Given** a repository with 1000 entities, **When** requesting page 2 with 50 items per page, **Then** only entities 51-100 are returned.
2. **Given** a paginated query with ordering, **When** results come back, **Then** the items on each page are in the correct order.
3. **Given** a paginated query with filtering, **When** results come back, **Then** only matching entities are included in the page count and results.

---

### User Story 3 - Filter Data Dynamically (Priority: P2)

As an application developer, I want to build queries at runtime based on what users are searching for so that I can support complex search screens without hard-coding every possible query combination.

**Why this priority**: Dynamic filtering is essential for good search experiences, but applications can start with simpler static queries and add dynamic filtering later.

**Independent Test**: Build various filter combinations from user input and verify the correct results are returned.

**Acceptance Scenarios**:

1. **Given** a user searches for entities where a field equals a specific value, **When** the query runs, **Then** only matching entities are returned.
2. **Given** a user searches for entities containing a text string, **When** the query runs, **Then** entities with that text anywhere in the field are returned (case-insensitive by default).
3. **Given** a user combines multiple search conditions with AND/OR logic, **When** the query runs, **Then** results match all the combined criteria.
4. **Given** a user wants results sorted by specific columns, **When** the query runs, **Then** results come back in the requested order.

---

### User Story 4 - Process Multiple Records at Once (Priority: P2)

As an application developer, I want to insert, update, or delete many entities in a single operation so that I can efficiently handle batch processing scenarios like importing data or bulk updates.

**Why this priority**: Batch operations improve performance significantly for large datasets, but single-entity operations work fine for smaller volumes.

**Independent Test**: Perform bulk operations on collections of entities and verify all are processed correctly with better performance than individual operations.

**Acceptance Scenarios**:

1. **Given** a collection of 100 new entities to import, **When** bulk add is called, **Then** all entities are saved efficiently.
2. **Given** a collection of entities that need updating, **When** bulk update is called, **Then** all changes are persisted.
3. **Given** a collection of entities to delete, **When** bulk delete is called, **Then** all entities are marked as deleted (soft delete) or removed (permanent delete).
4. **Given** a very large collection that exceeds the default batch size, **When** the operation runs, **Then** it's automatically split into manageable chunks.

---

### User Story 5 - Keep Deleted Data for Recovery (Priority: P2)

As an application developer, I want to mark entities as deleted instead of actually removing them so that I can recover accidentally deleted data and maintain a history of what was in the system.

**Why this priority**: Data retention and audit trails are important for many applications, but hard delete works fine for scenarios where permanent removal is acceptable.

**Independent Test**: Delete entities and verify they still exist in the database with deletion timestamps and are hidden from normal queries.

**Acceptance Scenarios**:

1. **Given** an entity that needs to be deleted, **When** soft delete is used, **Then** the entity stays in the database with a deletion timestamp.
2. **Given** soft-deleted entities exist, **When** normal queries run, **Then** deleted entities don't appear in results.
3. **Given** a need to see deleted entities, **When** querying with a special flag, **Then** deleted entities are included in results.
4. **Given** an entity with related records, **When** it's soft deleted, **Then** the deletion cascades to related entities automatically.

---

### User Story 6 - Prevent Lost Updates from Conflicting Edits (Priority: P1)

As an application developer, I want to detect when two people try to update the same record at the same time so that changes don't silently overwrite each other.

**Why this priority**: Data loss from concurrent updates is a serious problem that corrupts data and frustrates users. Every application needs some form of concurrency control.

**Independent Test**: Simulate two users editing the same entity and verify the second update is blocked with a clear error message.

**Acceptance Scenarios**:

1. **Given** two users load the same entity, **When** user A saves their changes first, then user B tries to save, **Then** user B gets an error about the conflict.
2. **Given** an entity was modified after being loaded, **When** trying to save changes, **Then** the system detects the mismatch and prevents the save.
3. **Given** a concurrency conflict occurs, **When** the error is caught, **Then** it provides enough information to resolve the conflict (showing what changed).

---

### User Story 7 - Keep Database Schema Up to Date Automatically (Priority: P3)

As an application developer, I want the database schema to update automatically when the application starts so that I don't need to remember to run migration scripts manually during deployment.

**Why this priority**: Automatic migrations are convenient, but manual migration workflows work fine too. This is a developer experience improvement rather than a functional requirement.

**Independent Test**: Start an application with pending database migrations and verify the schema updates before the application tries to use the database.

**Acceptance Scenarios**:

1. **Given** an application starting up, **When** there are pending migrations, **Then** migrations are applied automatically.
2. **Given** an in-memory database for testing, **When** the application starts, **Then** the database is created without needing migrations.
3. **Given** a database that's already current, **When** the application starts, **Then** no migration work is done and the application starts normally.

---

### User Story 8 - Reduce Configuration with Smart Conventions (Priority: P2)

As an application developer, I want common entity behaviors to be configured automatically so that I don't have to write repetitive configuration code for every entity.

**Why this priority**: Convention-based configuration significantly reduces boilerplate, but manual configuration is always possible as a fallback.

**Independent Test**: Create entities inheriting from the base entity class and verify the conventions are applied without additional configuration.

**Acceptance Scenarios**:

1. **Given** an entity that inherits from the base class, **When** the model is configured, **Then** timestamp columns are set up automatically.
2. **Given** an entity with a version property for concurrency, **When** the model is configured, **Then** it's set up as a concurrency token automatically.
3. **Given** entities that support soft delete, **When** queries run, **Then** deleted entities are filtered out automatically.
4. **Given** configuration classes in an assembly, **When** the model is built, **Then** all configurations with default constructors are applied automatically.

---

### User Story 9 - Build High-Performance Applications with Native AOT (Priority: P1)

As an application developer, I want the persistence package to work with Native AOT compilation so that I can build fast-starting, small-footprint applications.

**Why this priority**: Native AOT is a core requirement for the framework. The package must support it to meet the framework's performance goals.

**Independent Test**: Publish an application using the package with Native AOT enabled and verify it runs without issues.

**Acceptance Scenarios**:

1. **Given** an application using the persistence package, **When** published with Native AOT, **Then** it compiles successfully.
2. **Given** explicit DI registrations, **When** the application is published with Native AOT, **Then** the service graph is resolved correctly without reflection trimming warnings.
3. **Given** an AOT-compiled application, **When** it runs, **Then** all persistence operations work normally.

---

### User Story 10 - Write Fast Unit Tests Without a Real Database (Priority: P2)

As an application developer, I want to write unit tests using an in-memory database so that my tests run quickly without needing an external database server.

**Why this priority**: Fast tests are important for developer productivity, but integration tests with real databases also have value. In-memory tests are a convenience.

**Independent Test**: Write unit tests that use an in-memory database and verify they run quickly and reliably.

**Acceptance Scenarios**:

1. **Given** a unit test using an in-memory database, **When** data is saved, **Then** it can be queried back within the same test.
2. **Given** multiple tests running in parallel, **When** each uses an in-memory database, **Then** tests don't interfere with each other.
3. **Given** a test completes, **When** the in-memory database is disposed, **Then** it doesn't affect other tests.

---

## Edge Cases

Things that can go wrong and how the system should handle them:

- **Null entity arguments**: When methods receive null entities, throw a clear exception explaining what's null.
- **Empty collections for bulk operations**: When bulk methods receive empty collections, succeed without doing any work.
- **Null items in collections**: When bulk operations encounter null entities in the collection, throw an exception explaining the problem.
- **Invalid pagination values**: When page size is zero or page index is negative, throw an exception with a helpful message.
- **Huge page numbers**: When page calculations would overflow, detect this before it happens and throw an exception.
- **Concurrent modification**: When someone else modified the record between read and write, throw a clear exception explaining the conflict.
- **Double soft delete**: When soft delete is called on an already-deleted entity, handle it gracefully without cascading again.
- **Invalid dynamic operators**: When a dynamic query uses an unknown operator, throw an exception listing the valid ones.
- **Wrong field names**: When a dynamic query references a field that doesn't exist, fail with a clear error about the invalid field.
- **Empty dynamic queries**: When a dynamic query has no filters or sorts, return all results without error.
- **Updating non-existent entities**: When trying to update something that was deleted, fail with a clear error.
- **Migration failures**: When a migration can't apply, provide details about what went wrong.
- **Disposal during operations**: When the database context is disposed while an operation is running, throw an appropriate exception.
- **Large result sets**: When a query would return more data than the limit, throw an exception rather than loading everything.
- **Circular relationships**: When loading related entities creates cycles, handle them without infinite loops.
- **Concurrent operations**: When multiple operations happen at once, each should work correctly within its own context.

## Requirements

### Functional Requirements

#### Repository Abstractions

- **FR-001**: Provide a package with zero external dependencies that defines repository interfaces and base classes
- **FR-002**: Include a base entity class with identity, timestamps, soft delete support, and concurrency control
- **FR-003**: Define async and sync repository interfaces for CRUD operations
- **FR-004**: Define interfaces for read-only query operations
- **FR-005**: Define pagination types with metadata about pages, counts, and navigation
- **FR-006**: Define types for dynamic querying with filters and sorting
- **FR-007**: Ensure the abstractions package has no dependencies on any specific database technology

#### Entity Framework Core Implementation

- **FR-008**: Provide a base repository class that implements all repository interfaces using EF Core
- **FR-009**: Support creating entities with automatic timestamp assignment
- **FR-010**: Support updating entities with timestamp updates and concurrency validation
- **FR-011**: Support deleting entities with both permanent and soft delete options
- **FR-012**: Support bulk operations for adding, updating, and deleting multiple entities at once
- **FR-013**: Support querying with predicates, includes, deleted entity filtering, and tracking control
- **FR-014**: Support paginated queries with ordering and includes
- **FR-015**: Support dynamic queries built at runtime from filter and sort criteria
- **FR-016**: Validate pagination parameters and throw helpful exceptions for invalid values
- **FR-017**: Support counting entities with and without filters
- **FR-018**: Support randomly selecting entities from queries
- **FR-019**: Provide methods to commit pending changes to the database
- **FR-020**: Detect and prevent concurrent updates using both version numbers and timestamps
- **FR-021**: Provide clear error messages when concurrency conflicts occur
- **FR-022**: Cascade soft deletes to related entities automatically
- **FR-023**: Support eager loading of related entities through include expressions
- **FR-024**: Allow disabling change tracking for read-only queries to improve performance
- **FR-025**: Support including soft-deleted entities in queries when needed
- **FR-026**: Handle large result sets by reading in chunks with configurable limits
- **FR-027**: Validate all entities in bulk operations before processing and fail fast on errors

#### Dynamic Querying

- **FR-028**: Support comparison operators (equals, not equals, less than, greater than, etc.)
- **FR-029**: Support null checking operators (is null, is not null)
- **FR-030**: Support string operators (starts with, ends with, contains, case sensitivity)
- **FR-031**: Support checking if a value is in a list of values
- **FR-032**: Support range queries with between operator
- **FR-033**: Support combining multiple filters with AND and OR logic
- **FR-034**: Support sorting by multiple fields in ascending or descending order
- **FR-035**: Validate operator names and throw exceptions for unknown operators
- **FR-036**: Validate field names and throw clear errors for invalid fields
- **FR-037**: Prevent SQL injection through proper parameterization of dynamic values

#### Pagination

- **FR-038**: Provide extension methods to paginate any queryable collection
- **FR-039**: Validate that page index and size are within acceptable ranges
- **FR-040**: Prevent integer overflow when calculating page offsets
- **FR-041**: Return metadata about total items, total pages, and current position
- **FR-042**: Indicate whether previous and next pages exist for navigation
- **FR-043**: Limit maximum page size to prevent excessive memory usage
- **FR-044**: Support both synchronous and asynchronous pagination

#### Entity Framework Configuration

- **FR-045**: Provide methods to configure timestamp columns for all entities automatically
- **FR-046**: Provide methods to configure optimistic concurrency for all entities automatically
- **FR-047**: Provide methods to configure soft delete filters for all entities automatically
- **FR-048**: Provide methods to apply all configuration classes from an assembly
- **FR-049**: Allow customization of column names through configuration parameters

#### Database Migrations

- **FR-050**: Provide an extension method to apply pending migrations on startup
- **FR-051**: Handle in-memory databases by creating the schema instead of using migrations
- **FR-052**: Handle relational databases by applying pending migrations
- **FR-053**: Check database connectivity before attempting migration

#### Testing Support

- **FR-061**: Work with EF Core's in-memory database provider for unit testing
- **FR-062**: Provide test doubles or fakes to enable testing without EF Core
- **FR-063**: Support unit tests that verify behavior without a real database
- **FR-064**: Include sample tests showing common testing patterns

#### Native AOT Compatibility

- **FR-065**: Ensure the abstractions package requires no reflection
- **FR-066**: Avoid reflection-heavy features in the EF Core package
- **FR-067**: Generate code that is fully compatible with AOT compilation
- **FR-068**: Validate AOT compatibility in continuous integration
- **FR-069**: Document any AOT limitations or required configurations

#### Data Protection

- **FR-070**: Keep the persistence package focused on data access without built-in encryption or sensitive field protection
- **FR-071**: Support applications in implementing encryption through EF Core value converters or extension points
- **FR-072**: Document that data protection should be handled at the database level (TDE, column encryption) or application level before persistence

#### Observability Extension Points

- **FR-070**: Provide extension points or interceptor patterns for applications to add custom logging, metrics, and tracing
- **FR-071**: Ensure public methods are virtual or the design uses composition to enable decorator/interceptor patterns
- **FR-072**: Log critical errors (concurrency conflicts, connection failures) while leaving all other observability to application code

#### Error Handling

- **FR-073**: Throw ArgumentNullException for null arguments with the parameter name
- **FR-074**: Throw ArgumentException for invalid pagination parameters with helpful messages
- **FR-075**: Throw concurrency exceptions with details about the conflict
- **FR-076**: Throw ArgumentException for invalid dynamic operators with a list of valid ones
- **FR-077**: Throw InvalidOperationException when result sets exceed configured limits

### Key Entities

- **Repository interfaces**: Define contracts for data access without tying to specific database technology
- **Base entity class**: Provides common properties like ID, timestamps, and deletion status for all entities
- **Pagination result**: Represents a page of data with metadata about total items and available pages
- **Dynamic query**: Represents a query built at runtime with filters and sorting
- **Filter**: Represents a single filter condition with field, operator, and value
- **Repository implementation**: EF Core-based implementation of repository interfaces
- **Migration applier**: Service for applying database schema migrations

## Success Criteria

### Measurable Outcomes

- **SC-001**: Applications compile without warnings when published with Native AOT
- **SC-003**: Single-entity operations complete in under 10 milliseconds
- **SC-004**: Pagination queries on 1000 records complete in under 100 milliseconds
- **SC-005**: Unit tests using in-memory database run in under 100 milliseconds each
- **SC-006**: Bulk operations process 1000 entities in under 5 seconds
- **SC-007**: Integration tests achieve 90% code coverage
- **SC-008**: All repository registrations follow standard .NET DI patterns for Native AOT compatibility
- **SC-009**: Abstractions package has zero external dependencies
- **SC-010**: All public APIs include XML documentation comments
- **SC-011**: CI pipeline validates AOT compatibility on every commit
- **SC-012**: Package works on .NET 11 without additional runtime dependencies

## Assumptions

- The target framework is .NET 11 with C# 14/15 language features
- Entity Framework Core 9.0 or later is used for data access
- Applications use the standard dependency injection container from Microsoft
- SQLite in-memory database is sufficient for integration testing
- Applications use standard Microsoft.Extensions.DependencyInjection for service registration
- Applications use a scoped database context per request
- Convention-based configuration applies to entities inheriting from the base class
- Soft delete is optional and can be bypassed when needed
- Optimistic concurrency uses both version numbers and timestamps for compatibility
- Dynamic queries use the System.Linq.Dynamic.Core library
- Bulk operations process in memory rather than using specialized bulk extensions

## Dependencies

- .NET 11 SDK
- Entity Framework Core packages (Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Relational)
- Microsoft.EntityFrameworkCore.InMemory for testing
- System.Linq.Dynamic.Core for dynamic queries
- Microsoft.Extensions.DependencyInjection

## Clarifications

### Session 2026-04-20

- **Q**: How should the persistence package support observability (logging, metrics, tracing)?
  **A**: Extension points only (hooks/interceptors) - no built-in logging, but provides extension points for applications to add logging/observability through decorators or delegation

- **Q**: How should the persistence package handle data encryption and protection for sensitive fields?
  **A**: Out of scope - rely on database-level encryption (Transparent Data Encryption, Always Encrypted) or application-level encryption before entities reach repositories

- **Q**: How should repositories be registered?
  **A**: Repositories must be registered explicitly in the application code (e.g., in a `RegistrationExtensions` class) to ensure maximum visibility and Native AOT compatibility.

- **Q**: Should we support MongoDB or other databases?
  **A**: Not initially. The focus is on relational databases through EF Core. Other databases can be added later through additional packages.

- **Q**: How should the package handle database-specific features?
  **A**: These should be built on top of the base abstractions, not included in the core package.

- **Q**: Should we support streaming large results with async enumerators?
  **A**: Not in the first version. The chunked GetAll method provides a basic pattern for large datasets.

- **Q**: How should transactions work across multiple repositories?
  **A**: They're coordinated at the database context level, not the repository level. Repositories in the same scope naturally share transactions.

- **Q**: Should repositories support different lifetimes like singleton or transient?
  **A**: No. Repositories must be scoped because they depend on the database context which is scoped.

- **Q**: What about entities that don't inherit from the base class?
  **A**: They won't work with the standard repository pattern. Developers would need to create custom repository implementations.

## Non-Goals

- Supporting non-relational databases like MongoDB or RavenDB in the first version
- Building a custom ORM instead of using EF Core
- Automatic code-first to schema synchronization (use EF Core migrations)
- Caching integration (handled separately as a cross-cutting concern)
- Audit logging beyond basic timestamps
- Built-in structured logging, metrics, or tracing instrumentation (applications add through extension points)
- Built-in data encryption or sensitive field protection (rely on database-level encryption or application-level encryption before persistence)
- Complex query features like projections and aggregations (use raw queries for these)
- Repository decoration or chaining patterns
- GraphQL integration
- Legacy ADO.NET patterns without EF Core
