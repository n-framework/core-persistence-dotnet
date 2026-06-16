using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using UnionRailway;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Repositories;

/// <summary>
/// Tests for query splitting mode support.
/// </summary>
public class SplittingTests
{
    [Theory]
    [InlineData(QuerySplittingMode.Default)]
    [InlineData(QuerySplittingMode.SplitQuery)]
    [InlineData(QuerySplittingMode.SingleQuery)]
    public async Task GetAllAsync_WithAnySplittingMode_ShouldReturnResults(QuerySplittingMode mode)
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 2.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var results = (await repo.GetAllAsync(new QueryOption<TestProduct>(Splitting: mode))).Unwrap();

        results.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllAsync_WithSplitting_ShouldCombineWithFilterAndOrdering()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Cherry", Price = 3.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Apple", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Apricot", Price = 2.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Banana", Price = 4.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var results = (
            await repo.GetAllAsync(
                new QueryOption<TestProduct>(
                    Predicate: static p => p.Name.StartsWith('A'),
                    OrderBy: static q => q.OrderBy(p => p.Name),
                    Splitting: QuerySplittingMode.SplitQuery
                )
            )
        ).Unwrap();

        results.Count.ShouldBe(2);
        results[0].Name.ShouldBe("Apple");
        results[1].Name.ShouldBe("Apricot");
    }

    [Fact]
    public async Task GetAllAsync_WithSplitting_ShouldCombineWithTracking()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();
        context.ChangeTracker.Clear();

        var results = (
            await repo.GetAllAsync(
                new QueryOption<TestProduct>(
                    Tracking: QueryTrackingMode.NoTracking,
                    Splitting: QuerySplittingMode.SplitQuery
                )
            )
        ).Unwrap();

        results.Count.ShouldBe(1);
        context.ChangeTracker.Entries<TestProduct>().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetListAsync_WithSplitting_ShouldReturnPaginatedResults()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 2.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "C", Price = 3.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var results = (
            await repo.GetListAsync(
                new PageableQueryOption<TestProduct> { Page = new(0, 2), Splitting = QuerySplittingMode.SplitQuery }
            )
        ).Unwrap();

        results.Items.Count.ShouldBe(2);
        results.Meta.TotalCount.ShouldBe(3);
    }

    [Theory]
    [InlineData(QuerySplittingMode.Default)]
    [InlineData(QuerySplittingMode.SplitQuery)]
    [InlineData(QuerySplittingMode.SingleQuery)]
    public async Task GetAllByDynamicAsync_WithAnySplittingMode_ShouldReturnResults(QuerySplittingMode mode)
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 2.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var results = (await repo.GetAllByDynamicAsync(new DynamicQueryOption(Splitting: mode))).Unwrap();

        results.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllAsync_WithSplittingAndSoftDelete_ShouldIncludeDeleted()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct product = (
            await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Deleted", Price = 1.00m })
        ).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        (await repo.DeleteAsync(product)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var withDeleted = (
            await repo.GetAllAsync(
                new QueryOptionWithSoftDelete<TestProduct>(
                    IncludeDeleted: true,
                    Splitting: QuerySplittingMode.SplitQuery
                )
            )
        ).Unwrap();

        withDeleted.Count.ShouldBe(1);
        withDeleted[0].IsDeleted.ShouldBeTrue();
    }
}
