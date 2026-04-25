using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.EFCore.Constants;
using NFramework.Persistence.EFCore.Extensions;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Extensions;

public class ModelBuilderExtensionsTests
{
    private sealed class EntityConventionsTestEntity : SoftDeletableEntity<Guid>
    {
        [Obsolete("Only for ORM use", true)]
#pragma warning disable CS0618
        public EntityConventionsTestEntity() { }
#pragma warning restore CS0618

        public EntityConventionsTestEntity(Guid id)
            : base(id) { }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class BaseEntityTestEntity : Entity<Guid>
    {
        [Obsolete("Only for ORM use", true)]
#pragma warning disable CS0618
        public BaseEntityTestEntity() { }
#pragma warning restore CS0618

        public BaseEntityTestEntity(Guid id)
            : base(id) { }
    }

    private sealed class ConventionTestDbContext(DbContextOptions<ConventionTestDbContext> options) : DbContext(options)
    {
        public DbSet<EntityConventionsTestEntity> Entities => Set<EntityConventionsTestEntity>();
        public DbSet<BaseEntityTestEntity> BaseEntities => Set<BaseEntityTestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityConventionsTestEntity>(builder =>
            {
                _ = builder
                    .ConfigureEntity<EntityConventionsTestEntity, Guid>()
                    .ConfigureAuditable<EntityConventionsTestEntity, Guid>()
                    .ConfigureSoftDeletable<EntityConventionsTestEntity, Guid>();
                _ = builder.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<BaseEntityTestEntity>(builder =>
            {
                builder.ConfigureEntity<BaseEntityTestEntity, Guid>();
            });

            base.OnModelCreating(modelBuilder);
        }
    }

    [Fact]
    public void Extensions_ConfigureSoftDeletable_AppliesHierarchicalConfigurations()
    {
        DbContextOptions<ConventionTestDbContext> options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using ConventionTestDbContext context = new(options);

        // Fix CA1812 by ensuring instantiation
        _ = new EntityConventionsTestEntity(Guid.NewGuid());

        Microsoft.EntityFrameworkCore.Metadata.IEntityType? entityType = context.Model.FindEntityType(
            typeof(EntityConventionsTestEntity)
        );
        entityType.ShouldNotBeNull();

        // Assert Key and Concurrency (from ConfigureEntity)
        entityType.FindProperty(nameof(EntityConventionsTestEntity.Id)).ShouldNotBeNull();
        entityType.FindProperty(nameof(EntityConventionsTestEntity.RowVersion))?.IsConcurrencyToken.ShouldBeTrue();

        // Assert Timestamps (from ConfigureAuditable)
        entityType.FindProperty(nameof(EntityConventionsTestEntity.CreatedAt)).ShouldNotBeNull();
        entityType.FindProperty(nameof(EntityConventionsTestEntity.UpdatedAt)).ShouldNotBeNull();

        // Assert Filtered Index (from ConfigureSoftDeletable performance setting)
        var index = entityType
            .GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(EntityConventionsTestEntity.IsDeleted)));
        index.ShouldNotBeNull();
        index.GetFilter().ShouldBe("IsDeleted = 0");
    }

    [Fact]
    public async Task Extensions_ConfigureSoftDeletable_AllowsSelectiveDisabling()
    {
        DbContextOptions<ConventionTestDbContext> options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using ConventionTestDbContext context = new(options);
        var entity = new EntityConventionsTestEntity(Guid.NewGuid()) { Name = "Test", IsDeleted = true };
        context.Add(entity);
        await context.SaveChangesAsync();

        // Standard query should NOT return it
        int count = await context.Set<EntityConventionsTestEntity>().CountAsync();
        count.ShouldBe(0);

        // Selective disabling of "NFW_SoftDeletionFilter" should return it
        int countWithDeleted = await context
            .Set<EntityConventionsTestEntity>()
            .IgnoreQueryFilters([QueryFilters.SoftDeletion])
            .CountAsync();
        countWithDeleted.ShouldBe(1);
    }

    [Fact]
    public void Extensions_ConfigureEntity_AppliesBaseConfiguration()
    {
        DbContextOptions<ConventionTestDbContext> options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using ConventionTestDbContext context = new(options);

        // Fix CA1812 by ensuring instantiation
        _ = new BaseEntityTestEntity(Guid.NewGuid());

        Microsoft.EntityFrameworkCore.Metadata.IEntityType? entityType = context.Model.FindEntityType(
            typeof(BaseEntityTestEntity)
        );
        entityType.ShouldNotBeNull();

        entityType.FindProperty(nameof(BaseEntityTestEntity.Id)).ShouldNotBeNull();
        entityType.FindProperty(nameof(BaseEntityTestEntity.RowVersion))?.IsConcurrencyToken.ShouldBeTrue();
    }
}
