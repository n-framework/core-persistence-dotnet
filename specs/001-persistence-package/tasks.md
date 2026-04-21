# Tasks: NFramework.Persistence Package

**Input**: Design documents from `/specs/001-persistence-package/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Test tasks included as specified in the feature specification (FR-061 to FR-064)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/NFramework.Persistence.*` for three packages
- **Tests**: `tests/NFramework.Persistence.*.Tests/` per package
- Paths follow feature-based structure: `Features/`, `Shared/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for three-package solution

- [ ] T001 Create solution file and three project directories: src/NFramework.Persistence.Abstractions, src/NFramework.Persistence.EfCore, src/NFramework.Persistence.Generators
- [ ] T002 Create Abstractions project with .csproj targeting net11.0 and zero external dependencies in src/NFramework.Persistence.Abstractions/NFramework.Persistence.Abstractions.csproj
- [ ] T003 [P] Create EfCore project with .csproj targeting net11.0 and EF Core 9.0+ dependencies in src/NFramework.Persistence.EfCore/NFramework.Persistence.EfCore.csproj
- [ ] T004 [P] Create Generators project with .csproj targeting netstandard2.0 and Roslyn dependencies in src/NFramework.Persistence.Generators/NFramework.Persistence.Generators.csproj
- [ ] T005 [P] Create test projects: NFramework.Persistence.Abstractions.Tests, NFramework.Persistence.EfCore.Tests, NFramework.Persistence.Generators.Tests in tests/
- [ ] T006 [P] Enable nullable reference types and implicit usings in all projects
- [ ] T007 [P] Configure XML documentation generation for all projects
- [ ] T008 Add editorconfig with .editorconfig at solution root for consistent code style (tabs for C#)
- [ ] T009 [P] Create Feature-based folder structure in each project: Features/, Shared/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core abstractions that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Entity Base Classes

- [ ] T010 [P] Create `Entity<TId>` base class with Id and RowVersion properties in src/NFramework.Persistence.Abstractions/Features/Entities/Entity.cs
- [ ] T011 [P] Create `AuditableEntity<TId>` inheriting `Entity<TId>` with CreatedAt and UpdatedAt in src/NFramework.Persistence.Abstractions/Features/Entities/AuditableEntity.cs
- [ ] T012 [P] Create `SoftDeletableEntity<TId>` inheriting `AuditableEntity<TId>` with IsDeleted and DeletedAt in src/NFramework.Persistence.Abstractions/Features/Entities/SoftDeletableEntity.cs

### Repository Interfaces

- [ ] T013 [P] Create `IAsyncRepository<TEntity, TId>` interface with CRUD method signatures in src/NFramework.Persistence.Abstractions/Features/Repositories/IAsyncRepository.cs
- [ ] T014 [P] Create `IRepository<TEntity, TId>` synchronous interface in src/NFramework.Persistence.Abstractions/Features/Repositories/IRepository.cs
- [ ] T015 [P] Create `IQuery<TEntity>` read-only interface in src/NFramework.Persistence.Abstractions/Features/Repositories/IQuery.cs

### Pagination Types

- [ ] T016 [P] Create `IPaginate<T>` interface with Items, Index, Size, Count, TotalCount, TotalPages, HasPrevious, HasNext in src/NFramework.Persistence.Abstractions/Features/Pagination/IPaginate.cs
- [ ] T017 Create `Paginate<T>` implementation class with validation in src/NFramework.Persistence.Abstractions/Features/Pagination/Paginate.cs

### Dynamic Query Types

- [ ] T018 [P] Create DynamicQuery class with Filters, Sorts, PageIndex, PageSize properties in src/NFramework.Persistence.Abstractions/Features/Dynamic/DynamicQuery.cs
- [ ] T019 [P] Create Filter class with Field, Operator, Value, IsNot properties in src/NFramework.Persistence.Abstractions/Features/Dynamic/Filter.cs
- [ ] T020 [P] Create Sort class with Field, Direction properties in src/NFramework.Persistence.Abstractions/Features/Dynamic/Sort.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Create Repository with Standard CRUD Operations (Priority: P1) 🎯 MVP

**Goal**: Define repository interfaces and have them automatically registered with dependency injection

**Independent Test**: Create a repository interface and verify it's registered in the DI container with the correct lifetime and implementation type

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T021 [P] [US1] Create unit test for entity base classes in tests/NFramework.Persistence.Abstractions.Tests/Features/Entities/EntityTests.cs
- [ ] T022 [P] [US1] Create unit test for pagination types in tests/NFramework.Persistence.Abstractions.Tests/Features/Pagination/PaginateTests.cs
- [ ] T023 [P] [US1] Create unit test for dynamic query types in tests/NFramework.Persistence.Abstractions.Tests/Features/Dynamic/DynamicQueryTests.cs

### Implementation for User Story 1

#### Source Generator (FR-054 to FR-060)

- [ ] T024 [P] [US1] Create PersistenceGenerator incremental generator class in src/NFramework.Persistence.Generators/PersistenceGenerator.cs
- [ ] T025 [US1] Implement syntax receiver to detect `IAsyncRepository<,>` interfaces in src/NFramework.Persistence.Generators/Syntax/RepositorySyntaxReceiver.cs
- [ ] T026 [US1] Implement source generation logic for DI registration code in src/NFramework.Persistence.Generators/Generators/DependencyInjectionGenerator.cs
- [ ] T027 [US1] Add diagnostic descriptors for PG001 (no implementation), PG002 (no DbContext constructor), PG003 (generic constraints) in src/NFramework.Persistence.Generators/Diagnostics/DiagnosticDescriptors.cs

#### EF Core Repository Base (FR-008 to FR-027)

- [ ] T028 [P] [US1] Create `EfRepositoryBase<TEntity, TId>` abstract partial class with DbContext injection in src/NFramework.Persistence.EfCore/Features/Repositories/EfRepositoryBase.cs
- [ ] T029 [P] [US1] Create EfRepositoryBase.Create.cs partial with AddAsync and BulkAddAsync in src/NFramework.Persistence.EfCore/Features/Repositories/EfRepositoryBase.Create.cs
- [ ] T030 [P] [US1] Create EfRepositoryBase.Read.cs partial with GetByIdAsync, GetAllAsync, CountAsync, RandomAsync in src/NFramework.Persistence.EfCore/Features/Repositories/EfRepositoryBase.Read.cs
- [ ] T031 [US1] Create EfRepositoryBase.Update.cs partial with UpdateAsync and BulkUpdateAsync in src/NFramework.Persistence.EfCore/Features/Repositories/EfRepositoryBase.Update.cs (depends on T029, T030)
- [ ] T032 [US1] Create EfRepositoryBase.Delete.cs partial with DeleteAsync and BulkDeleteAsync in src/NFramework.Persistence.EfCore/Features/Repositories/EfRepositoryBase.Delete.cs

#### Integration Tests for US1

- [ ] T033 [US1] Create golden file tests for source generator output in tests/NFramework.Persistence.Generators.Tests/GeneratorTests.cs
- [ ] T034 [P] [US1] Create integration test for repository CRUD operations with in-memory database in tests/NFramework.Persistence.EfCore.Tests/Features/Repositories/RepositoryCrudTests.cs

**Checkpoint**: At this point, User Story 1 should be fully functional - repositories can be defined and automatically registered

---

## Phase 4: User Story 2 - Query Entities with Pagination (Priority: P1)

**Goal**: Query entities with pagination to display large datasets without loading everything into memory

**Independent Test**: Query a dataset with thousands of records and verify that requesting a specific page returns only that page's worth of data with accurate metadata

### Tests for User Story 2

- [ ] T035 [P] [US2] Create unit test for pagination validation (invalid page index, invalid page size) in tests/NFramework.Persistence.Abstractions.Tests/Features/Pagination/PaginateValidationTests.cs
- [ ] T036 [P] [US2] Create integration test for pagination with ordering in tests/NFramework.Persistence.EfCore.Tests/Features/Paging/PaginationTests.cs

### Implementation for User Story 2

- [ ] T037 [P] [US2] Create IQueryablePaginateExtensions with ToPaginateAsync extension method in src/NFramework.Persistence.EfCore/Features/Paging/IQueryablePaginateExtensions.cs
- [ ] T038 [US2] Implement page index and size validation in ToPaginateAsync (depends on T037)
- [ ] T039 [US2] Add integer overflow protection when calculating page offsets in ToPaginateAsync (depends on T037)
- [ ] T040 [US2] Set maximum page size limit to prevent excessive memory usage in ToPaginateAsync (depends on T037)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 6 - Prevent Lost Updates from Conflicting Edits (Priority: P1)

**Goal**: Detect when two people try to update the same record at the same time so changes don't silently overwrite each other

**Independent Test**: Simulate two users editing the same entity and verify the second update is blocked with a clear error message

### Tests for User Story 6

- [ ] T041 [P] [US6] Create unit test for concurrency conflict detection in tests/NFramework.Persistence.EfCore.Tests/Features/Repositories/ConcurrencyTests.cs

### Implementation for User Story 6

- [ ] T042 [US6] Add RowVersion property configuration as concurrency token in src/NFramework.Persistence.EfCore/Features/Configuration/ModelBuilderExtensions.cs (ApplyOptimisticConcurrencyConfiguration method)
- [ ] T043 [US6] Implement DbUpdateConcurrencyException handling in EfRepositoryBase.UpdateAsync with clear error messages in src/NFramework.Persistence.EfCore/Features/Repositories/EfRepositoryBase.Update.cs (depends on T031)
- [ ] T044 [US6] Add UpdatedAt as secondary concurrency token in ApplyOptimisticConcurrencyConfiguration (depends on T042)

**Checkpoint**: Concurrency control now prevents lost updates

---

## Phase 6: User Story 9 - Build High-Performance Applications with Native AOT (Priority: P1)

**Goal**: Persistence package works with Native AOT compilation for fast-starting, small-footprint applications

**Independent Test**: Publish an application using the package with Native AOT enabled and verify it runs without issues

### Tests for User Story 9

- [ ] T045 [P] [US9] Create AOT-compatible test project in tests/NFramework.Persistence.AotTests/
- [ ] T046 [US9] Create GitHub Actions workflow to validate AOT publishing on every commit in .github/workflows/aot-validation.yml

### Implementation for User Story 9

- [ ] T047 [US9] Verify abstractions package has zero reflection - audit all code for reflection usage in src/NFramework.Persistence.Abstractions/
- [ ] T048 [US9] Verify EfCore package avoids reflection-heavy features - audit dynamic querying implementation in src/NFramework.Persistence.EfCore/
- [ ] T049 [US9] Add DynamicallyAccessedMembers attributes where runtime type access is unavoidable in src/NFramework.Persistence.EfCore/Features/Dynamic/
- [ ] T050 [US9] Verify generated DI registration code is trimmable and AOT-compatible in src/NFramework.Persistence.Generators/Generators/DependencyInjectionGenerator.cs

**Checkpoint**: All P1 user stories complete - package supports AOT compilation

---

## Phase 7: User Story 3 - Filter Data Dynamically (Priority: P2)

**Goal**: Build queries at runtime based on user input for complex search screens

**Independent Test**: Build various filter combinations from user input and verify the correct results are returned

### Tests for User Story 3

- [ ] T051 [P] [US3] Create unit test for each supported operator (eq, neq, lt, lte, gt, gte, starts with, ends with, contains, in) in tests/NFramework.Persistence.Abstractions.Tests/Features/Dynamic/FilterOperatorTests.cs
- [ ] T052 [P] [US3] Create integration test for dynamic queries with multiple filters in tests/NFramework.Persistence.EfCore.Tests/Features/Dynamic/DynamicQueryTests.cs

### Implementation for User Story 3

- [ ] T053 [P] [US3] Create IQueryableDynamicFilterExtensions with ApplyFilter method in src/NFramework.Persistence.EfCore/Features/Dynamic/IQueryableDynamicFilterExtensions.cs
- [ ] T054 [US3] Implement comparison operators (eq, neq, lt, lte, gt, gte) using System.Linq.Dynamic.Core in ApplyFilter (depends on T053)
- [ ] T055 [US3] Implement null checking operators (is null, is not null) in ApplyFilter (depends on T053)
- [ ] T056 [US3] Implement string operators (starts with, ends with, contains, does not contain) in ApplyFilter (depends on T053)
- [ ] T057 [US3] Implement collection operator (in) in ApplyFilter (depends on T053)
- [ ] T058 [US3] Implement logical operators (and, or) for combining filters in ApplyFilter (depends on T053)
- [ ] T059 [US3] Add field name validation and throw ArgumentException for invalid fields in ApplyFilter (depends on T053)
- [ ] T060 [US3] Add operator validation and throw ArgumentException with list of valid operators in ApplyFilter (depends on T053)
- [ ] T061 [US3] Implement ApplySort method for sorting by multiple fields in IQueryableDynamicFilterExtensions (depends on T053)
- [ ] T062 [US3] Integrate dynamic filtering and sorting into EfRepositoryBase.FindAsync in src/NFramework.Persistence.EfCore/Features/Repositories/EfRepositoryBase.Read.cs (depends on T030, T058, T061)

**Checkpoint**: Dynamic filtering enables complex search scenarios

---

## Phase 8: User Story 4 - Process Multiple Records at Once (Priority: P2)

**Goal**: Insert, update, or delete many entities in a single operation for batch processing

**Independent Test**: Perform bulk operations on collections of entities and verify all are processed correctly with better performance than individual operations

### Tests for User Story 4

- [ ] T063 [P] [US4] Create integration test for bulk add with 1000 entities in tests/NFramework.Persistence.EfCore.Tests/Features/Repositories/BulkOperationsTests.cs
- [ ] T064 [P] [US4] Create integration test for bulk update with entities in tests/NFramework.Persistence.EfCore.Tests/Features/Repositories/BulkOperationsTests.cs
- [ ] T065 [P] [US4] Create integration test for bulk delete with entities in tests/NFramework.Persistence.EfCore.Tests/Features/Repositories/BulkOperationsTests.cs

### Implementation for User Story 4

- [ ] T066 [US4] Implement BulkAddAsync with batching (default 1000) in EfRepositoryBase.Create.cs - validate all entities non-null before processing (depends on T029)
- [ ] T067 [US4] Implement BulkUpdateAsync with RowVersion checking for each entity in EfRepositoryBase.Update.cs (depends on T031)
- [ ] T068 [US4] Implement BulkDeleteAsync with soft delete support in EfRepositoryBase.Delete.cs (depends on T032)
- [ ] T069 [US4] Add empty collection handling in bulk operations - succeed without work (depends on T066, T067, T068)
- [ ] T070 [US4] Add null item detection in bulk operations - throw ArgumentException explaining the problem (depends on T066, T067, T068)

**Checkpoint**: Batch operations improve performance for large datasets

---

## Phase 9: User Story 5 - Keep Deleted Data for Recovery (Priority: P2)

**Goal**: Mark entities as deleted instead of actually removing them for data recovery and audit trails

**Independent Test**: Delete entities and verify they still exist in the database with deletion timestamps and are hidden from normal queries

### Tests for User Story 5

- [ ] T071 [P] [US5] Create unit test for soft delete with IsDeleted and DeletedAt in tests/NFramework.Persistence.EfCore.Tests/Features/Repositories/SoftDeleteTests.cs
- [ ] T072 [P] [US5] Create integration test for soft delete query filtering (deleted entities excluded) in tests/NFramework.Persistence.EfCore.Tests/Features/Repositories/SoftDeleteTests.cs
- [ ] T073 [P] [US5] Create integration test for includeDeleted flag (deleted entities included) in tests/NFramework.Persistence.EfCore.Tests/Features/Repositories/SoftDeleteTests.cs

### Implementation for User Story 5

- [ ] T074 [US5] Add ApplySoftDeleteConfiguration method to ModelBuilderExtensions in src/NFramework.Persistence.EfCore/Features/Configuration/ModelBuilderExtensions.cs
- [ ] T075 [US5] Configure IsDeleted boolean flag for query optimization in ApplySoftDeleteConfiguration (depends on T074)
- [ ] T076 [US5] Configure DeletedAt DateTime? for audit trail in ApplySoftDeleteConfiguration (depends on T074)
- [ ] T077 [US5] Implement global query filter for IsDeleted in ApplySoftDeleteConfiguration (depends on T075)
- [ ] T078 [US5] Add _includeDeleted flag to EfRepositoryBase for toggling filter in src/NFramework.Persistence.EfCore/Features/Repositories/EfRepositoryBase.cs
- [ ] T079 [US5] Implement IncludeDeleted() method to temporarily include deleted entities in queries in EfRepositoryBase.Read.cs (depends on T030, T078)
- [ ] T080 [US5] Modify DeleteAsync to check entity type and perform soft delete for `SoftDeletableEntity<TId>` in EfRepositoryBase.Delete.cs (depends on T032)
- [ ] T081 [US5] Add double soft delete protection - check if already deleted before cascading in EfRepositoryBase.Delete.cs (depends on T080)

**Checkpoint**: Soft delete enables data recovery and audit trails

---

## Phase 10: User Story 8 - Reduce Configuration with Smart Conventions (Priority: P2)

**Goal**: Common entity behaviors configured automatically to reduce repetitive configuration code

**Independent Test**: Create entities inheriting from base classes and verify conventions are applied without additional configuration

### Tests for User Story 8

- [ ] T082 [P] [US8] Create unit test for timestamp configuration convention in tests/NFramework.Persistence.EfCore.Tests/Features/Configuration/ConventionTests.cs
- [ ] T083 [P] [US8] Create unit test for optimistic concurrency configuration convention in tests/NFramework.Persistence.EfCore.Tests/Features/Configuration/ConventionTests.cs
- [ ] T084 [P] [US8] Create unit test for soft delete configuration convention in tests/NFramework.Persistence.EfCore.Tests/Features/Configuration/ConventionTests.cs

### Implementation for User Story 8

- [ ] T085 [US8] Implement ApplyTimestampsConfiguration method - configure CreatedAt and UpdatedAt with defaults in src/NFramework.Persistence.EfCore/Features/Configuration/ModelBuilderExtensions.cs
- [ ] T086 [US8] Update CreateInterceptor to set CreatedAt on entity addition in src/NFramework.Persistence.EfCore/Shared/Interceptors/TimestampsInterceptor.cs
- [ ] T087 [US8] Update CreateInterceptor to set UpdatedAt on entity modification in TimestampsInterceptor (depends on T086)
- [ ] T088 [US8] Create ApplyConfigurationsFromAssembly method to scan for `IEntityTypeConfiguration<T>` classes in ModelBuilderExtensions (depends on T085)
- [ ] T089 [US8] Create ApplyAllConventions method that calls all convention methods in ModelBuilderExtensions (depends on T042, T074, T085)
- [ ] T090 [US8] Register TimestampsInterceptor in DbContext example in quickstart.md documentation

**Checkpoint**: Convention-based configuration reduces boilerplate

---

## Phase 11: User Story 10 - Write Fast Unit Tests Without a Real Database (Priority: P2)

**Goal**: Write unit tests using an in-memory database so tests run quickly without external database servers

**Independent Test**: Write unit tests that use an in-memory database and verify they run quickly and reliably

### Tests for User Story 10

- [ ] T091 [P] [US10] Create example unit test using in-memory database in tests/NFramework.Persistence.EfCore.Tests/Examples/InMemoryDatabaseExampleTests.cs
- [ ] T092 [P] [US10] Create unit test demonstrating parallel test execution with isolated databases in tests/NFramework.Persistence.EfCore.Tests/Examples/ParallelTestExample.cs

### Implementation for User Story 10

- [ ] T093 [US10] Add Microsoft.EntityFrameworkCore.InMemory package to EfCore test project in tests/NFramework.Persistence.EfCore.Tests/NFramework.Persistence.EfCore.Tests.csproj
- [ ] T094 [US10] Create TestDbContextFactory helper class for creating in-memory contexts in tests/NFramework.Persistence.EfCore.Tests/Helpers/TestDbContextFactory.cs
- [ ] T095 [US10] Document testing patterns in quickstart.md with in-memory database examples
- [ ] T096 [US10] Add test sample showing how to use unique database names per test in quickstart.md

**Checkpoint**: Fast in-memory tests enable rapid development

---

## Phase 12: User Story 7 - Keep Database Schema Up to Date Automatically (Priority: P3)

**Goal**: Database schema updates automatically when application starts

**Independent Test**: Start an application with pending database migrations and verify the schema updates before the application tries to use the database

### Tests for User Story 7

- [ ] T097 [P] [US7] Create integration test for automatic migration with in-memory database in tests/NFramework.Persistence.EfCore.Tests/Features/Migration/MigrationTests.cs
- [ ] T098 [P] [US7] Create integration test for automatic migration with relational database in tests/NFramework.Persistence.EfCore.Tests/Features/Migration/MigrationTests.cs

### Implementation for User Story 7

- [ ] T099 [US7] Create EnsureDatabaseCreated extension method for IServiceProvider in src/NFramework.Persistence.EfCore/Features/Migration/DatabaseFacadeExtensions.cs
- [ ] T100 [US7] Add in-memory database detection - call EnsureCreatedAsync for in-memory in DatabaseFacadeExtensions (depends on T099)
- [ ] T101 [US7] Add relational database detection - call MigrateAsync for relational in DatabaseFacadeExtensions (depends on T099)
- [ ] T102 [US7] Add connectivity check before migration - fail fast if database unavailable in DatabaseFacadeExtensions (depends on T099)
- [ ] T103 [US7] Document migration usage in quickstart.md with example startup code

**Checkpoint**: Automatic migrations simplify deployment

---

## Phase 13: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T104 [P] Add XML documentation comments to all public APIs in src/NFramework.Persistence.Abstractions/
- [ ] T105 [P] Add XML documentation comments to all public APIs in src/NFramework.Persistence.EfCore/
- [ ] T106 [P] Add XML documentation comments to source generator APIs in src/NFramework.Persistence.Generators/
- [ ] T107 [P] Create README.md for Abstractions package with API overview in src/NFramework.Persistence.Abstractions/README.md
- [ ] T108 [P] Create README.md for EfCore package with usage examples in src/NFramework.Persistence.EfCore/README.md
- [ ] T109 [P] Create README.md for Generators package with diagnostic reference in src/NFramework.Persistence.Generators/README.md
- [ ] T110 Verify all examples in quickstart.md work correctly
- [ ] T111 [P] Add performance benchmarks for single-entity operations (<10ms target) in tests/NFramework.Persistence.EfCore.Benchmarks/
- [ ] T112 [P] Add performance benchmarks for pagination queries (<100ms target for 1000 records) in tests/NFramework.Persistence.EfCore.Benchmarks/
- [ ] T113 [P] Add performance benchmarks for bulk operations (<5s target for 1000 entities) in tests/NFramework.Persistence.EfCore.Benchmarks/
- [ ] T114 Run all tests and ensure 90% code coverage (FR-007, SC-007)
- [ ] T115 Validate package works with Native AOT on all three packages (FR-065 to FR-069, SC-001, SC-011, SC-012)
- [ ] T116 Verify abstractions package has zero external dependencies (FR-007, SC-009)
- [ ] T117 Add ArgumentException for null entity arguments with parameter name (FR-073)
- [ ] T118 Add ArgumentException for invalid pagination parameters with helpful messages (FR-074)
- [ ] T119 Add ArgumentException for invalid dynamic operators with list of valid ones (FR-076)
- [ ] T120 Add InvalidOperationException when result sets exceed configured limits (FR-077)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-12)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 13)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - No dependencies on US1
- **User Story 6 (P1)**: Can start after Foundational (Phase 2) - No dependencies on US1/US2
- **User Story 9 (P1)**: Can start after Foundational (Phase 2) - No dependencies on US1/US2/US6
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) - No dependencies on P1 stories
- **User Story 4 (P2)**: Can start after Foundational (Phase 2) - No dependencies on other P2 stories
- **User Story 5 (P2)**: Can start after Foundational (Phase 2) - No dependencies on US3/US4
- **User Story 8 (P2)**: Depends on US5 completion (requires soft delete configuration)
- **User Story 10 (P2)**: Can start after Foundational (Phase 2) - No dependencies
- **User Story 7 (P3)**: Can start after Foundational (Phase 2) - No dependencies

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Interfaces/base classes before implementations
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

#### Setup Phase (Phase 1)

- T003, T004, T005, T006, T007, T008, T009 can all run in parallel (different .csproj files, folders)

#### Foundational Phase (Phase 2)

- T010, T011, T012 can run in parallel (three separate entity base classes)
- T013, T014, T015 can run in parallel (three separate repository interfaces)
- T018, T019, T020 can run in parallel (three separate dynamic query types)
- After entity bases and interfaces exist, T017 (Paginate implementation) can proceed

#### User Story 1 Phase

- T021, T022, T023 tests can run in parallel
- T024, T025 generator tasks can run in parallel
- T028, T029, T030 repository partials can run in parallel
- T033, T034 integration tests can run in parallel

#### User Story 2 Phase

- T035, T036 tests can run in parallel

#### User Story 3 Phase

- T051, T052 tests can run in parallel
- T054 through T061 filter operator implementations can potentially run in parallel

#### Polish Phase (Phase 13)

- T104, T105, T106 XML documentation can run in parallel
- T107, T108, T109 README files can run in parallel
- T111, T112, T113 benchmarks can run in parallel

#### Cross-Story Parallelization (After Foundational Phase)

With sufficient team capacity:

- Team A: User Story 1 (T021-T034)
- Team B: User Story 2 (T035-T040)
- Team C: User Story 6 (T041-T044)
- Team D: User Story 9 (T045-T050)
All can proceed simultaneously after Phase 2 completes.

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Create unit test for entity base classes in tests/.../EntityTests.cs"
Task: "Create unit test for pagination types in tests/.../PaginateTests.cs"
Task: "Create unit test for dynamic query types in tests/.../DynamicQueryTests.cs"

# Launch repository partial files together (after entity bases exist):
Task: "Create EfRepositoryBase abstract partial class in src/.../EfRepositoryBase.cs"
Task: "Create EfRepositoryBase.Create.cs partial in src/.../EfRepositoryBase.Create.cs"
Task: "Create EfRepositoryBase.Read.cs partial in src/.../EfRepositoryBase.Read.cs"
```

---

## Implementation Strategy

### MVP First (User Stories 1, 2, 6, 9 Only - All P1 Stories)

1. Complete Phase 1: Setup (T001-T009)
2. Complete Phase 2: Foundational (T010-T020) - CRITICAL blocks all stories
3. Complete Phase 3: User Story 1 (T021-T034)
4. Complete Phase 4: User Story 2 (T035-T040)
5. Complete Phase 5: User Story 6 (T041-T044)
6. Complete Phase 6: User Story 9 (T045-T050)
7. **STOP and VALIDATE**: Test all P1 stories independently
8. Deploy/demo P1 MVP - full CRUD with pagination, concurrency control, and AOT support

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Core repositories work
3. Add User Story 2 → Test independently → Pagination works
4. Add User Story 6 → Test independently → Concurrency control works
5. Add User Story 9 → Test independently → AOT validation passes
6. Add User Story 3 → Test independently → Dynamic filtering works
7. Add User Story 4 → Test independently → Bulk operations work
8. Add User Story 5 → Test independently → Soft delete works
9. Add User Story 8 → Test independently → Conventions reduce boilerplate
10. Add User Story 10 → Test independently → Fast tests available
11. Add User Story 7 → Test independently → Automatic migrations work
12. Polish → Production ready

### Parallel Team Strategy

With multiple developers after Foundational phase:

1. Team completes Setup + Foundational together (T001-T020)
2. Once Foundational done:
   - **Team A**: User Story 1 (T021-T034) - Core repository registration
   - **Team B**: User Story 2 (T035-T040) - Pagination
   - **Team C**: User Story 6 (T041-T044) - Concurrency control
   - **Team D**: User Story 9 (T045-T050) - AOT validation
3. After P1 stories complete:
   - **Team A**: User Story 3 (T051-T062) - Dynamic filtering
   - **Team B**: User Story 4 (T063-T070) - Bulk operations
   - **Team C**: User Story 5 (T071-T081) - Soft delete
   - **Team D**: User Story 8 (T082-T090) - Conventions (must wait for US5)
4. Final phase: All teams contribute to Polish (T104-T120)

---

## Notes

- [P] tasks = different files, no dependencies, can run in parallel
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing (TDD where tests are specified)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Total tasks: 120 (including test tasks as specified)
- P1 stories (MVP): T001-T050 (50 tasks)
- P2 stories: T051-T096 (46 tasks)
- P3 stories: T097-T103 (7 tasks)
- Polish: T104-T120 (17 tasks)
