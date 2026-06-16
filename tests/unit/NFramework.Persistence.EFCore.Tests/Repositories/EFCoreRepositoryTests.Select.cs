using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Repositories;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using UnionRailway;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Repositories;

/// <summary>
/// Tests for select/projection query support.
/// </summary>
public class SelectTests
{
    [Fact]
    public async Task GetSelectAsync_ShouldProjectSingleEntity()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Widget", Price = 9.99m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        string name = (
            await repo.GetSelectAsync(
                static p => p.Name,
                new QueryOption<TestProduct>(Predicate: static p => p.Name == "Widget")
            )
        ).Unwrap();

        name.ShouldBe("Widget");
    }

    [Fact]
    public async Task GetSelectAsync_ShouldReturnNotFoundWhenNoMatch()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        Rail<string> result = await repo.GetSelectAsync(
            static p => p.Name,
            new QueryOption<TestProduct>(Predicate: static p => p.Name == "NonExistent")
        );

        result.IsSuccess(out _, out _).ShouldBeFalse();
    }

    [Fact]
    public async Task GetSelectAsync_ShouldProjectToAnonymousType()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Gadget", Price = 19.99m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var projected = (
            await repo.GetSelectAsync(
                static p => new { p.Name, p.Price },
                new QueryOption<TestProduct>(Predicate: static p => p.Name == "Gadget")
            )
        ).Unwrap();

        projected.Name.ShouldBe("Gadget");
        projected.Price.ShouldBe(19.99m);
    }

    [Fact]
    public async Task GetSelectAsync_WithNullPredicate_ShouldReturnFirst()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Only", Price = 5.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        string name = (await repo.GetSelectAsync(static p => p.Name)).Unwrap();

        name.ShouldBe("Only");
    }

    [Fact]
    public async Task GetSelectAsync_OnEmptyTable_ShouldReturnNotFound()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        Rail<string> result = await repo.GetSelectAsync(static p => p.Name);

        result.IsSuccess(out _, out _).ShouldBeFalse();
    }

    [Fact]
    public async Task GetAllSelectAsync_ShouldProjectList()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 2.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "C", Price = 3.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var names = (await repo.GetAllSelectAsync(static p => p.Name)).Unwrap();

        names.Count.ShouldBe(3);
        names.ShouldContain("A");
        names.ShouldContain("B");
        names.ShouldContain("C");
    }

    [Fact]
    public async Task GetAllSelectAsync_ShouldApplyFilter()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Apple", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Banana", Price = 2.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Apricot", Price = 3.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var names = (
            await repo.GetAllSelectAsync(
                static p => p.Name,
                new QueryOption<TestProduct>(static p => p.Name.StartsWith('A'))
            )
        ).Unwrap();

        names.Count.ShouldBe(2);
        names.ShouldContain("Apple");
        names.ShouldContain("Apricot");
    }

    [Fact]
    public async Task GetAllSelectAsync_EmptyResult_ShouldReturnEmptyList()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        var names = (
            await repo.GetAllSelectAsync(
                static p => p.Name,
                new QueryOption<TestProduct>(static p => p.Name == "NonExistent")
            )
        ).Unwrap();

        names.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetAllSelectAsync_ShouldApplyOrdering()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "C", Price = 3.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 2.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var names = (
            await repo.GetAllSelectAsync(
                static p => p.Name,
                new QueryOption<TestProduct>(OrderBy: static q => q.OrderBy(p => p.Name))
            )
        ).Unwrap();

        names.Count.ShouldBe(3);
        names[0].ShouldBe("A");
        names[1].ShouldBe("B");
        names[2].ShouldBe("C");
    }

    [Fact]
    public async Task GetAllSelectAsync_WithValueType_ShouldProjectDecimals()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 2.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var prices = (await repo.GetAllSelectAsync(static p => p.Price)).Unwrap();

        prices.Count.ShouldBe(2);
        prices.ShouldContain(1.00m);
        prices.ShouldContain(2.00m);
    }

    [Fact]
    public async Task GetAllSelectAsync_WithSplitting_ShouldCombineWithProjection()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 2.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var names = (
            await repo.GetAllSelectAsync(
                static p => p.Name,
                new QueryOption<TestProduct>(Splitting: QuerySplittingMode.SplitQuery)
            )
        ).Unwrap();

        names.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllSelectAsync_WithNoTracking_ShouldNotTrackEntities()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();
        context.ChangeTracker.Clear();

        var names = (
            await repo.GetAllSelectAsync(
                static p => p.Name,
                new QueryOption<TestProduct>(Tracking: QueryTrackingMode.NoTracking)
            )
        ).Unwrap();

        names.Count.ShouldBe(1);
        context.ChangeTracker.Entries<TestProduct>().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllSelectAsync_WithSoftDelete_ShouldIncludeDeleted()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct product = (
            await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Gone", Price = 1.00m })
        ).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        (await repo.DeleteAsync(product)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var withDeleted = (
            await repo.GetAllSelectAsync(
                static p => p.Name,
                new QueryOptionWithSoftDelete<TestProduct>(IncludeDeleted: true)
            )
        ).Unwrap();

        withDeleted.Count.ShouldBe(1);
        withDeleted[0].ShouldBe("Gone");
    }

    [Fact]
    public async Task GetListSelectAsync_ShouldPaginateProjectedResults()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 2.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "C", Price = 3.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var page = (
            await repo.GetListSelectAsync(
                static p => p.Name,
                new PageableQueryOption<TestProduct> { Page = new(0, 2), OrderBy = static q => q.OrderBy(p => p.Name) }
            )
        ).Unwrap();

        page.Items.Count.ShouldBe(2);
        page.Meta.TotalCount.ShouldBe(3);
        page.Meta.TotalPages.ShouldBe(2);
        page.Meta.HasNext.ShouldBeTrue();
        page.Meta.HasPrevious.ShouldBeFalse();
    }

    [Fact]
    public async Task GetListSelectAsync_SecondPage_ShouldReturnRemaining()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 2.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "C", Price = 3.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var page = (
            await repo.GetListSelectAsync(
                static p => p.Name,
                new PageableQueryOption<TestProduct> { Page = new(1, 2), OrderBy = static q => q.OrderBy(p => p.Name) }
            )
        ).Unwrap();

        page.Items.Count.ShouldBe(1);
        page.Items[0].ShouldBe("C");
        page.Meta.HasPrevious.ShouldBeTrue();
        page.Meta.HasNext.ShouldBeFalse();
    }

    [Fact]
    public async Task GetListSelectAsync_EmptyTable_ShouldReturnEmptyPage()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        var page = (
            await repo.GetListSelectAsync(
                static p => p.Name,
                new PageableQueryOption<TestProduct> { Page = new(0, 10) }
            )
        ).Unwrap();

        page.Items.Count.ShouldBe(0);
        page.Meta.TotalCount.ShouldBe(0);
        page.Meta.TotalPages.ShouldBe(0);
    }

    [Fact]
    public async Task GetListSelectAsync_WithValueType_ShouldPaginateDecimals()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 10.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 20.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "C", Price = 30.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var page = (
            await repo.GetListSelectAsync(
                static p => p.Price,
                new PageableQueryOption<TestProduct> { Page = new(0, 2), OrderBy = static q => q.OrderBy(p => p.Price) }
            )
        ).Unwrap();

        page.Items.Count.ShouldBe(2);
        page.Items[0].ShouldBe(10.00m);
        page.Items[1].ShouldBe(20.00m);
        page.Meta.TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task GetAllSelectAsync_WhenExceedingLimit_ShouldReturnError()
    {
        using TestDbContext context = TestDbContext.Create();
        LimitedSelectTestRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "1", Price = 1 })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "2", Price = 2 })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "3", Price = 3 })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        Rail<IReadOnlyList<string>> result = await repo.GetAllSelectAsync(static p => p.Name);

        result.IsSuccess(out _, out UnionError? error).ShouldBeFalse();
        (error is UnionError.Custom).ShouldBeTrue();
    }

    [Fact]
    public async Task GetSelectAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            (await repo.GetSelectAsync(static p => p.Name, cancellationToken: cts.Token)).Unwrap()
        );
    }

    [Fact]
    public async Task GetAllSelectAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            (await repo.GetAllSelectAsync(static p => p.Name, cancellationToken: cts.Token)).Unwrap()
        );
    }

    private class LimitedSelectTestRepository(TestDbContext context)
        : EFCoreRepository<TestProduct, Guid, TestDbContext>(context)
    {
        protected override int? MaxResultSetSize => 2;
    }
}
