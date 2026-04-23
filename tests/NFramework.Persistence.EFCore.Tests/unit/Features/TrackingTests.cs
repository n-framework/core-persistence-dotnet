using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Tests.Unit.Helpers;
using NFramework.Persistence.EFCore.Tests.Unit.Repositories;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Unit.Features;

public class TrackingTests
{
    [Fact]
    public async Task GetAllAsync_WithNoTracking_ShouldNotTrackEntities()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);
        Guid id = Guid.NewGuid();
        await repo.AddAsync(
            new TestProduct
            {
                Id = id,
                Name = "P1",
                Price = 10,
            }
        );
        await repo.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var results = await repo.GetAllAsync(new QueryOption<TestProduct>(Tracking: QueryTrackingMode.NoTracking));

        results.Count.ShouldBe(1);
        context.ChangeTracker.Entries<TestProduct>().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithTracking_ShouldTrackEntities()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);
        Guid id = Guid.NewGuid();
        await repo.AddAsync(
            new TestProduct
            {
                Id = id,
                Name = "P1",
                Price = 10,
            }
        );
        await repo.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var results = await repo.GetAllAsync(new QueryOption<TestProduct>(Tracking: QueryTrackingMode.Tracking));

        results.Count.ShouldBe(1);
        context.ChangeTracker.Entries<TestProduct>().Count().ShouldBe(1);
        context.ChangeTracker.Entries<TestProduct>().First().Entity.Id.ShouldBe(id);
    }

    [Fact]
    public async Task GetAllAsync_WithNoTrackingWithIdentityResolution_ShouldNotTrackEntities()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);
        Guid id = Guid.NewGuid();
        await repo.AddAsync(
            new TestProduct
            {
                Id = id,
                Name = "P1",
                Price = 10,
            }
        );
        await repo.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var results = await repo.GetAllAsync(
            new QueryOption<TestProduct>(Tracking: QueryTrackingMode.NoTrackingWithIdentityResolution)
        );

        results.Count.ShouldBe(1);
        context.ChangeTracker.Entries<TestProduct>().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllByDynamicAsync_WithNoTracking_ShouldNotTrackEntities()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);
        Guid id = Guid.NewGuid();
        await repo.AddAsync(
            new TestProduct
            {
                Id = id,
                Name = "P1",
                Price = 10,
            }
        );
        await repo.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var results = await repo.GetAllByDynamicAsync(new DynamicQueryOption(Tracking: QueryTrackingMode.NoTracking));

        results.Count.ShouldBe(1);
        context.ChangeTracker.Entries<TestProduct>().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllByDynamicAsync_WithTracking_ShouldTrackEntities()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);
        Guid id = Guid.NewGuid();
        await repo.AddAsync(
            new TestProduct
            {
                Id = id,
                Name = "P1",
                Price = 10,
            }
        );
        await repo.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var results = await repo.GetAllByDynamicAsync(new DynamicQueryOption(Tracking: QueryTrackingMode.Tracking));

        results.Count.ShouldBe(1);
        context.ChangeTracker.Entries<TestProduct>().Count().ShouldBe(1);
    }
}
