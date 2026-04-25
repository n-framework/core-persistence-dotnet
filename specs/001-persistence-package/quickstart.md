# Quickstart: NFramework.Persistence Package

**Last Updated**: 2026-04-20

This guide gets you up and running with the NFramework.Persistence package in about 5 minutes. You'll define entities, create repositories, and start working with data right away.

## Prerequisites

Before you begin, make sure you have:

- .NET 11 SDK installed
- A project that uses dependency injection (the default Microsoft container works great)

## Step 1: Install the Packages

Add the NuGet packages to your project:

```bash
dotnet add package NFramework.Persistence.Abstractions
dotnet add package NFramework.Persistence.EfCore
```

You'll also need an EF Core database provider:

```bash
# SQL Server
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.PostgreSQL

# SQLite (great for development and testing)
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

## Step 2: Define Your Entities

Create entity classes that inherit from `SoftDeletableEntity<TId>`. The base class gives you common properties for free:

```csharp
using NFramework.Persistence.Abstractions.Features.Entities;

namespace MyApp.Domain
{
    // Use SoftDeletableEntity when you need soft delete support
    public class User : SoftDeletableEntity<int>
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
    }

    // Use Entity for entities that don't need soft delete
    public class SystemLog : Entity<int>
    {
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }
}
```

**Entity vs SoftDeletableEntity:**

| Type                       | When to use                                                            | What you get                         |
| -------------------------- | ---------------------------------------------------------------------- | ------------------------------------ |
| `Entity<TId>`              | Entities that should never be soft deleted (logs, audit records, etc.) | Id, CreatedAt, UpdatedAt, RowVersion |
| `SoftDeletableEntity<TId>` | Regular entities that support soft delete                              | All of Entity + IsDeleted, DeletedAt |

**What you get without writing any code:**

- `Id` - Primary key (an `int` in this example)
- `CreatedAt` - Automatically set when you create the entity
- `UpdatedAt` - Automatically updated whenever you save changes
- `RowVersion` - Handles concurrent edits so changes don't get lost
- `IsDeleted` - Soft delete flag (SoftDeletableEntity only)
- `DeletedAt` - When the entity was soft deleted (SoftDeletableEntity only)

## Step 3: Create Your DbContext

Set up your EF Core DbContext and apply conventions:

```csharp
using Microsoft.EntityFrameworkCore;
using MyApp.Domain;
using NFramework.Persistence.EfCore.Features.Configuration;
using MyApp.Data.Configurations; // Your entity configurations

namespace MyApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all conventions - timestamps, soft delete, concurrency
            modelBuilder.ApplyAllConventions();

            // Apply entity configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
```

## Step 3.5: Configure Each Entity

Each entity gets its own configuration file. This keeps your configurations organized and discoverable:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Domain;

namespace MyApp.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table name
            builder.ToTable("Users");

            // Property configurations
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            // Indexes
            builder.HasIndex(u => u.Username)
                .IsUnique();

            builder.HasIndex(u => u.Email)
                .IsUnique();
        }
    }

    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Price)
                .HasPrecision(18, 2);
        }
    }
}
```

The `ApplyConfigurationsFromAssembly` call automatically finds all classes that implement `IEntityTypeConfiguration<T>` and have a parameterless constructor.

## Step 4: Define Repository Interfaces

Create repository interfaces for your entities:

```csharp
using NFramework.Persistence.Abstractions.Features.Repositories;

namespace MyApp.Repositories
{
    public interface IUserRepository : IAsyncRepository<User, int>
    {
        // Add custom methods here if you need something specific
        Task<User?> GetByUsernameAsync(string username);
    }

    public interface IProductRepository : IAsyncRepository<Product, int>
    {
        // Custom methods for products
        Task<IReadOnlyList<Product>> GetActiveProductsAsync();
    }
}
```

## Step 5: Implement Repositories

Create your repository implementations by inheriting from the base class:

```csharp
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Domain;
using MyApp.Repositories;
using NFramework.Persistence.EfCore.Features.Repositories;

namespace MyApp.Repositories
{
    public class UserRepository : EfRepositoryBase<User, int>, IUserRepository
    {
        public UserRepository(AppDbContext context)
            : base(context) { }

        // Custom methods use Table directly with EF Core
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await Table
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        // For complex queries, you can use all of EF Core's power
        public async Task<IReadOnlyList<User>> GetActiveUsersAsync(DateTime since)
        {
            return await Table
                .Where(u => u.LastLoginAt >= since)
                .OrderByDescending(u => u.LastLoginAt)
                .ToListAsync();
        }
    }

    public class ProductRepository : EfRepositoryBase<Product, int>, IProductRepository
    {
        public ProductRepository(AppDbContext context)
            : base(context) { }

        public async Task<IReadOnlyList<Product>> GetActiveProductsAsync()
        {
            return await Table
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}
```

**The `Table` property** gives you direct access to the `DbSet<TEntity>` so you can use all of EF Core's query capabilities. This is more efficient and flexible than using `GetAll()` for custom queries.

## Step 6: Register Everything

Configure dependency injection in your application:

```csharp
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add your DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Add all repositories explicitly
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();
```

That's it! Your repositories are registered and ready to use.

## Step 7: Use Your Repositories

Inject and use repositories throughout your application:

```csharp
using MyApp.Repositories;
using MyApp.Domain;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // Create a new user
    public async Task<User> CreateUserAsync(string username, string email)
    {
        var user = new User
        {
            Username = username,
            Email = email
        };

        return await _userRepository.AddAsync(user);
        // CreatedAt and UpdatedAt are set automatically
    }

    // Get a user by ID
    public async Task<User?> GetUserAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    // Update a user
    public async Task UpdateUserAsync(User user)
    {
        await _userRepository.UpdateAsync(user);
        // UpdatedAt is updated automatically
    }

    // Delete a user (soft delete)
    public async Task DeleteUserAsync(User user)
    {
        await _userRepository.DeleteAsync(user);
        // IsDeleted = true, DeletedAt = now
        // The user won't appear in normal queries anymore
    }
}
```

## Dynamic Querying

Need flexible search? Use the `DynamicQuery` class:

```csharp
using NFramework.Persistence.Abstractions.Features.Dynamic;

public async Task<IPaginate<User>> SearchUsersAsync(string searchTerm, int page)
{
    var query = new DynamicQuery
    {
        Filters = new List<Filter>
        {
            new Filter
            {
                Field = "Username",
                Operator = "contains",
                Value = searchTerm
            },
            new Filter
            {
                Field = "IsDeleted",
                Operator = "eq",
                Value = false
            }
        },
        Sorts = new List<Sort>
        {
            new Sort { Field = "CreatedAt", Direction = "desc" }
        },
        PageIndex = page,
        PageSize = 20
    };

    return await _userRepository.FindAsync(query);
}
```

**Operators you can use:**

- Comparisons: `eq` (equals), `neq` (not equals), `lt` (less than), `lte` (less than or equal), `gt` (greater than), `gte` (greater than or equal)
- Null checks: `is null`, `is not null`
- String operations: `starts with`, `ends with`, `contains`, `does not contain`
- Collections: `in`

## Bulk Operations

Need to process many records at once? Use bulk operations:

```csharp
// Bulk insert - create many users
var newUsers = Enumerable.Range(0, 1000)
    .Select(i => new User { Username = $"user{i}", Email = $"user{i}@example.com" })
    .ToList();

var addedCount = await _userRepository.BulkAddAsync(newUsers);

// Bulk update - update many users
foreach (var user in usersToUpdate)
{
    user.LastLoginAt = DateTime.UtcNow;
}

var updatedCount = await _userRepository.BulkUpdateAsync(usersToUpdate);

// Bulk delete - soft delete many users
var deletedCount = await _userRepository.BulkDeleteAsync(usersToDelete);
```

## Automatic Migrations

Keep your database schema up to date automatically when your application starts:

```csharp
using NFramework.Persistence.EfCore.Features.Migration;

var app = builder.Build();

// Apply pending migrations or create the database
await app.Services.EnsureDatabaseCreated<AppDbContext>();

app.Run();
```

This takes care of:

- Creating the database if you're using the in-memory provider
- Applying pending migrations if you're using a relational database
- Failing fast if the database isn't reachable

## Configuration

### Connection String (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MyAppDb;Integrated Security=True"
  }
}
```

### Customize Conventions

If you need to customize how conventions apply:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Apply conventions with a custom schema name
    modelBuilder.ApplyAllConventions(schemaName: "app");

    // Or apply individual conventions
    // modelBuilder.ApplyTimestampsConfiguration();
    // modelBuilder.ApplyOptimisticConcurrencyConfiguration();
    // modelBuilder.ApplySoftDeleteConfiguration();
}
```

## Testing

### Unit Tests with In-Memory Database

The in-memory database provider makes unit tests fast. For maximum isolation and performance, use `TestDbContextFactory`:

```csharp
using NFramework.Persistence.EFCore.Tests.Helpers; // From your test project helpers
using MyApp.Domain;
using MyApp.Repositories;
using Shouldly;
using Xunit;

public class UserRepositoryTests
{
    [Fact]
    public async Task AddAsync_SetsTimestamps()
    {
        // Arrange
        // Each call to CreateInMemory() uses a unique database name (Guid)
        using var context = TestDbContextFactory.CreateInMemory();
        var repository = new UserRepository(context);
        var user = new User { Username = "test", Email = "test@example.com" };

        // Act
        var result = await repository.AddAsync(user);

        // Assert
        result.CreatedAt.ShouldNotBe(default);
        result.UpdatedAt.ShouldNotBe(default);
    }

    [Fact]
    public async Task Parallel_Tests_Are_Isolated()
    {
        // This test can run in parallel with others because
        // each context has its own unique in-memory database name.
        using var context = TestDbContextFactory.CreateInMemory();
        // ...
    }
}
```

### Relational Testing with SQLite

If your tests require specific relational behavior (like unique constraints or concurrency tokens), use the SQLite provider:

```csharp
[Fact]
public async Task Update_With_Conflicting_RowVersion_Throws()
{
    // SQLite enforcing RowVersion behavior
    using var context = TestDbContextFactory.CreateSqlite();
    // ...
}
```

## Troubleshooting

### Concurrency Conflict

**Error:** `DbUpdateConcurrencyException: The database operation was expected to affect 1 row(s), but actually affected 0 row(s).`

**What this means:** Someone else modified the record after you loaded it but before you saved.

**How to handle:**

```csharp
try
{
    await _repository.UpdateAsync(user);
}
catch (DbUpdateConcurrencyException ex)
{
    // Reload the entity to see what changed
    var entry = ex.Entries.Single();
    var databaseValues = await entry.GetDatabaseValuesAsync();

    // Present the conflict to the user so they can decide what to do
}
```

## Next Steps

- **Eager Loading:** Learn about loading related entities with `Include()`
- **Testing Patterns:** Read more about integration testing with EF Core
- **Performance Tuning:** Adjust batch sizes for bulk operations in your scenario
- **Custom Queries:** Add your own methods to repositories for complex scenarios

## Full Example

A complete working example is available in the examples directory.

Happy coding!
