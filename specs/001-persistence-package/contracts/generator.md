# Source Generator Contract

**Feature**: 001-persistence-package
**Package**: NFramework.Persistence.Generators
**Version**: 1.0.0

## Purpose

This document describes the contract for the Roslyn source generator. The generator automatically discovers your repository interfaces during compilation and emits dependency injection registration code, so you don't have to manually register each repository.

---

## Generator Behavior

### Discovery Phase

The generator scans compilation for repository interfaces matching these criteria:

**Pattern:**

```csharp
public interface IRepositoryName : IAsyncRepository<TEntity, TId>
{
}
```

**Detection Rules:**

1. Interface must directly or indirectly inherit from `IAsyncRepository<,>`
2. Generic type arguments must be concrete types (not open generics)
3. Interface must be accessible (public or internal)

**Example Matches:**

```csharp
// ✓ Detected - direct inheritance
public interface IUserRepository : IAsyncRepository<User, int> { }

// ✓ Detected - inherits through base interface
public interface IOrderRepository : ICustomRepository, IAsyncRepository<Order, long> { }

// ✗ Not detected - doesn't inherit from IAsyncRepository
public interface IProductQuery { }

// ✗ Not detected - open generic
public interface IRepository<T> : IAsyncRepository<T, int> { }
```

---

### Code Generation Phase

For each detected repository interface, the generator searches for a corresponding implementation class.

**Implementation Pattern:**

```csharp
public class RepositoryName : EfRepositoryBase<TEntity, TId>, IRepositoryName
{
    public RepositoryName(DbContext context)
        : base(context) { }
}
```

**Matching Rules:**

1. Class name matches interface name without "I" prefix
2. Class implements the detected repository interface
3. Class has a constructor accepting `DbContext`

**Example Matches:**

```csharp
// Interface: IUserRepository
// Implementation: UserRepository (✓)

// Interface: IOrderRepository
// Implementation: OrderRepository (✓)
```

---

### Emitted Code Structure

The generator emits a static `DependencyInjection` class in the `NFramework.Persistence.Generated` namespace.

**Generated Signature:**

```csharp
namespace NFramework.Persistence.Generated
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceRepositories(
            this IServiceCollection services)
        {
            // Registration statements
            return services;
        }
    }
}
```

**Registration Pattern:**

```csharp
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IOrderRepository, OrderRepository>();
// ... one per discovered repository
```

**Lifetime:** All repositories registered as **Scoped** (required for DbContext lifetime).

---

## Diagnostics

### PG001: Implementation Not Found

**Severity:** Error

**Condition:**
Repository interface detected but no corresponding implementation class found.

**Message:**

```text
PG001: Repository interface '{InterfaceName}' has no corresponding implementation class '{ExpectedClassName}'. Expected a class named '{ExpectedClassName}' that implements '{InterfaceName}' and has a constructor accepting DbContext.
```

**Example:**

```csharp
// Detected interface but missing implementation
public interface IUserRepository : IAsyncRepository<User, int> { }
// No UserRepository class found
```

---

### PG002: Constructor Missing

**Severity:** Error

**Condition:**
Implementation class exists but has no constructor accepting `DbContext`.

**Message:**

```text
PG002: Repository implementation '{ClassName}' must have a public or internal constructor accepting 'DbContext'. Found {count} constructors but none match the signature '(DbContext context)'.
```

**Example:**

```csharp
// Invalid - no DbContext constructor
public class UserRepository : EfRepositoryBase<User, int>, IUserRepository
{
    // Missing: public UserRepository(DbContext context) { }
}
```

---

### PG003: Generic Constraints Not Satisfied

**Severity:** Error

**Condition:**
Repository interface's generic type arguments don't satisfy constraints.

**Message:**

```text
PG003: Repository interface '{InterfaceName}' uses type arguments that don't satisfy generic constraints. Type '{TEntity}' must inherit from 'BaseEntity<TId>' and type 'TId' must be a non-nullable value type.
```

**Example:**

```csharp
// Invalid - Entity doesn't inherit from BaseEntity
public interface IInvalidRepository : IAsyncRepository<string, int> { }
```

---

## Configuration Options

### Analyzer Configuration

The generator supports configuration via `.editorconfig`:

```ini
# Namespace for generated code (default: NFramework.Persistence.Generated)
dotnet_code_quality_generated_namespace = MyNamespace.Generated

# Enable/disable specific diagnostics
dotnet_diagnostic_PG001.severity = error
dotnet_diagnostic_PG002.severity = error
dotnet_diagnostic_PG003.severity = error
```

---

## Contract Guarantees

### What the Generator Guarantees

1. **Idempotent Output:** Running the generator multiple times produces identical output
2. **Deterministic Order:** Repository registrations are emitted in alphabetical order by interface name
3. **Incremental Execution:** Only re-analyzes changed syntax nodes, uses caching
4. **AOT Compatibility:** Generated code uses no reflection, fully trimmable
5. **No Runtime Dependencies:** Generated code has no dependencies on the generator

### What the Generator Does NOT Guarantee

1. **Implementation Correctness:** Only verifies existence, not implementation behavior
2. **DbContext Compatibility:** Does not verify that implementation's DbContext is compatible
3. **Duplicate Detection:** Does not prevent multiple repositories for same entity type
4. **Naming Conflicts:** Does not detect conflicts with user-defined `DependencyInjection` class

---

## Integration Contract

### Usage in Application Code

```csharp
// Generated extension method is automatically available
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => ...);
builder.Services.AddPersistenceRepositories(); // ← Generated call

var app = builder.Build();
```

**Expected Behavior:**

- All repository interfaces are registered with scoped lifetime
- Constructor injection of `IUserRepository` works throughout application
- Repositories share DbContext scope per HTTP request (or async scope)

---

### Generated File Location

Generated source file: `NFramework.Persistence.Generated.DependencyInjection.g.cs`

**Characteristics:**

- File is generated during compilation (not in source tree)
- Visible via "Show All Files" in IDE
- Should not be manually edited or checked into source control
- Automatically excluded from source control via `.gitignore` patterns

---

## Versioning Policy

- **Breaking Changes:** Any change to emitted code structure, namespace, or diagnostic IDs requires major version bump
- **New Features:** Additional configuration options or new diagnostics are minor version changes
- **Bug Fixes:** Generator bug fixes that don't change emitted API are patch changes

---

## Implementation Requirements

The generator implementation MUST:

1. **Use IncrementalGenerator API:** For performance and caching
2. **Be Resilient to Malformed Code:** Gracefully handle incomplete or invalid syntax
3. **Provide Clear Diagnostics:** Error messages must explain what's wrong and how to fix
4. **Support Partial Classes:** Detect implementations split across multiple files
5. **Handle Multiple Assemblies:** Work correctly with multi-project solutions
6. **Preserve User Code Comments:** Do not remove or modify user's XML documentation

---

## Testing Requirements

### Golden File Tests

Generated output must match pre-approved "golden" files for known inputs.

**Test Pattern:**

1. Prepare source code with repository interfaces
2. Run generator
3. Compare emitted code to golden file
4. Fail on any difference (whitespace-insensitive)

### AOT Compilation Tests

Generated code must:

- Compile successfully with `-p:PublishAot=true`
- Pass IL trimmer analysis with `-p:TrimMode=link`
- Not produce warnings about reflection or dynamic access

### Diagnostic Tests

Each diagnostic (PG001, PG002, PG003) must have:

- Positive test case (triggers the diagnostic)
- Negative test case (similar valid code, no diagnostic)
- Verification of diagnostic message content

---

## Example: Complete Generation Flow

**Input Source:**

```csharp
using NFramework.Persistence;

namespace MyApp.Repositories
{
    public interface IUserRepository : IAsyncRepository<User, int> { }
    public interface IOrderRepository : IAsyncRepository<Order, long> { }
}

namespace MyApp.Data
{
    public class UserRepository : EfRepositoryBase<User, int>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context) { }
    }

    public class OrderRepository : EfRepositoryBase<Order, long>, IOrderRepository
    {
        public OrderRepository(DbContext context) : base(context) { }
    }
}
```

**Emitted Code:**

```csharp
// <auto-generated /> This file is generated by NFramework.Persistence.Generators

namespace NFramework.Persistence.Generated
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceRepositories(
            this IServiceCollection services)
        {
            services.AddScoped<MyApp.Repositories.IUserRepository, MyApp.Data.UserRepository>();
            services.AddScoped<MyApp.Repositories.IOrderRepository, MyApp.Data.OrderRepository>();
            return services;
        }
    }
}
```

**Application Usage:**

```csharp
builder.Services.AddPersistenceRepositories();
// IUserRepository and IOrderRepository now registered
```
