using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.EFCore.Interceptors;

namespace NFramework.Persistence.EFCore.Tests.Helpers;

/// <summary>
/// Test entity with full soft-delete and audit support.
/// </summary>
internal sealed class TestProduct : SoftDeletableEntity<Guid>
{
    /// <summary>Product name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Product price.</summary>
    public decimal Price { get; set; }
}

/// <summary>
/// Simple auditable entity without soft-delete.
/// </summary>
internal sealed class TestCategory : AuditableEntity<int>
{
    /// <summary>Category name.</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// InMemory test DbContext.
/// </summary>
/// <inheritdoc />
internal sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    /// <summary>Products table.</summary>
    public DbSet<TestProduct> Products => Set<TestProduct>();

    /// <summary>Categories table.</summary>
    public DbSet<TestCategory> Categories => Set<TestCategory>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<TestProduct>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.RowVersion).IsRequired().HasDefaultValue(Array.Empty<byte>());
        });

        _ = modelBuilder.Entity<TestCategory>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.RowVersion).IsRequired().HasDefaultValue(Array.Empty<byte>());
        });
    }

    /// <summary>
    /// Creates a new InMemory TestDbContext with a unique database name.
    /// </summary>
    public static TestDbContext Create()
    {
        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new AuditSaveChangesInterceptor())
            .Options;

        TestDbContext context = new(options);
        context.Database.EnsureCreated();
        return context;
    }
}
