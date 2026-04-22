using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.EFCore.Interceptors;

namespace NFramework.Persistence.EFCore.Tests.Unit.Helpers;

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
/// Soft-deletable parent entity with child collection for cascade tests.
/// </summary>
internal sealed class TestOrder : SoftDeletableEntity<Guid>
{
    /// <summary>Order number.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>Child items in this order.</summary>
    public ICollection<TestOrderItem> Items { get; set; } = [];
}

/// <summary>
/// Soft-deletable child entity for cascade tests.
/// </summary>
internal sealed class TestOrderItem : SoftDeletableEntity<Guid>
{
    /// <summary>FK to parent order.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Navigation to parent.</summary>
    public TestOrder? Order { get; set; }

    /// <summary>Item description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Sub-items for this item.</summary>
    public ICollection<TestOrderSubItem> SubItems { get; set; } = [];
}

/// <summary>
/// Soft-deletable entity for multi-level cascade tests.
/// </summary>
internal sealed class TestOrderSubItem : SoftDeletableEntity<Guid>
{
    /// <summary>FK to parent item.</summary>
    public Guid ItemId { get; set; }

    /// <summary>Navigation to parent.</summary>
    public TestOrderItem? Item { get; set; }

    /// <summary>Sub-item details.</summary>
    public string Details { get; set; } = string.Empty;
}

/// <summary>
/// Soft-deletable entity for self-referencing hierarchy tests.
/// </summary>
internal sealed class TestEmployee : SoftDeletableEntity<Guid>
{
    /// <summary>Employee name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Manager FK.</summary>
    public Guid? ManagerId { get; set; }

    /// <summary>Manager navigation.</summary>
    public TestEmployee? Manager { get; set; }

    /// <summary>Subordinates navigation.</summary>
    public ICollection<TestEmployee> Subordinates { get; set; } = [];
}

/// <summary>
/// Soft-deletable entity for Many-To-Many tests.
/// </summary>
internal sealed class TestUser : SoftDeletableEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public ICollection<TestRole> Roles { get; set; } = [];
}

/// <summary>
/// Soft-deletable entity for Many-To-Many tests.
/// </summary>
internal sealed class TestRole : SoftDeletableEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public ICollection<TestUser> Users { get; set; } = [];
}

/// <summary>
/// Auditable (but not soft-deletable) child entity.
/// </summary>
internal sealed class TestOrderLog : AuditableEntity<Guid>
{
    public Guid OrderId { get; set; }
    public TestOrder? Order { get; set; }
    public string Message { get; set; } = string.Empty;
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

    /// <summary>Orders table.</summary>
    public DbSet<TestOrder> Orders => Set<TestOrder>();

    /// <summary>Order items table.</summary>
    public DbSet<TestOrderItem> OrderItems => Set<TestOrderItem>();

    /// <summary>Order sub-items table.</summary>
    public DbSet<TestOrderSubItem> OrderSubItems => Set<TestOrderSubItem>();

    /// <summary>Employees table.</summary>
    public DbSet<TestEmployee> Employees => Set<TestEmployee>();

    /// <summary>Users table.</summary>
    public DbSet<TestUser> Users => Set<TestUser>();

    /// <summary>Roles table.</summary>
    public DbSet<TestRole> Roles => Set<TestRole>();

    /// <summary>Order logs table.</summary>
    public DbSet<TestOrderLog> OrderLogs => Set<TestOrderLog>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<TestProduct>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.RowVersion).IsRequired().HasDefaultValue(Array.Empty<byte>());
            _ = entity.HasQueryFilter(e => !e.IsDeleted);
        });

        _ = modelBuilder.Entity<TestCategory>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.RowVersion).IsRequired().HasDefaultValue(Array.Empty<byte>());
            // Auditable only, no filter
        });

        _ = modelBuilder.Entity<TestOrder>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.RowVersion).IsRequired().HasDefaultValue(Array.Empty<byte>());
            _ = entity.HasMany(e => e.Items).WithOne(e => e.Order).HasForeignKey(e => e.OrderId);
            _ = entity.HasQueryFilter(e => !e.IsDeleted);
        });

        _ = modelBuilder.Entity<TestOrderItem>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.RowVersion).IsRequired().HasDefaultValue(Array.Empty<byte>());
            _ = entity.HasMany(e => e.SubItems).WithOne(e => e.Item).HasForeignKey(e => e.ItemId);
            _ = entity.HasQueryFilter(e => !e.IsDeleted);
        });

        _ = modelBuilder.Entity<TestOrderSubItem>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.RowVersion).IsRequired().HasDefaultValue(Array.Empty<byte>());
            _ = entity.HasQueryFilter(e => !e.IsDeleted);
        });

        _ = modelBuilder.Entity<TestEmployee>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.RowVersion).IsRequired().HasDefaultValue(Array.Empty<byte>());
            _ = entity
                .HasMany(e => e.Subordinates)
                .WithOne(e => e.Manager)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.ClientCascade);
            _ = entity.HasQueryFilter(e => !e.IsDeleted);
        });

        _ = modelBuilder.Entity<TestUser>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.RowVersion).IsRequired().HasDefaultValue(Array.Empty<byte>());
            _ = entity.HasMany(e => e.Roles).WithMany(e => e.Users);
            _ = entity.HasQueryFilter(e => !e.IsDeleted);
        });

        _ = modelBuilder.Entity<TestRole>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.RowVersion).IsRequired().HasDefaultValue(Array.Empty<byte>());
            _ = entity.HasQueryFilter(e => !e.IsDeleted);
        });

        _ = modelBuilder.Entity<TestOrderLog>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.RowVersion).IsRequired().HasDefaultValue(Array.Empty<byte>());
            _ = entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            // Auditable only, no filter
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
