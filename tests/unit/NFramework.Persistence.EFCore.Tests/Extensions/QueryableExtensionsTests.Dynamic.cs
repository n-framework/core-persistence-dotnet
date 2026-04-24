using NFramework.Persistence.Abstractions.Dynamic;
using NFramework.Persistence.Abstractions.Pagination;
using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Extensions;

/// <summary>
/// Tests for pagination and dynamic query capabilities.
/// </summary>
public class DynamicQueryTests
{
    [Fact]
    public async Task GetListAsync_ShouldReturnPaginatedResults()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        for (int i = 0; i < 25; i++)
        {
            await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = $"Product_{i}", Price = i * 1.0m });
        }

        await repo.SaveChangesAsync();

        PaginatedList<TestProduct> page1 = await repo.GetListAsync(
            new PageableQueryOption<TestProduct> { Page = new Paging(0, 10) }
        );

        page1.Items.Count.ShouldBe(10);
        page1.Meta.TotalCount.ShouldBe(25);
        page1.Meta.TotalPages.ShouldBe(3);
        page1.Meta.HasPrevious.ShouldBeFalse();
        page1.Meta.HasNext.ShouldBeTrue();
    }

    [Fact]
    public async Task GetListAsync_LastPage_ShouldHaveCorrectMeta()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        for (int i = 0; i < 25; i++)
        {
            await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = $"Product_{i}", Price = i * 1.0m });
        }

        await repo.SaveChangesAsync();

        PaginatedList<TestProduct> lastPage = await repo.GetListAsync(
            new PageableQueryOption<TestProduct> { Page = new Paging(2, 10) }
        );

        lastPage.Items.Count.ShouldBe(5);
        lastPage.Meta.HasPrevious.ShouldBeTrue();
        lastPage.Meta.HasNext.ShouldBeFalse();
    }

    [Fact]
    public async Task GetAllByDynamicAsync_WithEqualFilter_ShouldReturnMatches()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Alpha", Price = 10.00m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Beta", Price = 20.00m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Alpha", Price = 30.00m });
        await repo.SaveChangesAsync();

        DynamicQueryOption options = new(
            Filters:
            [
                new Filter
                {
                    Field = "Name",
                    Operator = FilterOperator.Equal,
                    Value = "Alpha",
                },
            ]
        );

        IReadOnlyList<TestProduct> results = await repo.GetAllByDynamicAsync(options);
        results.Count.ShouldBe(2);
    }

    [Fact]
    public async Task CountByDynamicAsync_ShouldCountMatchingEntities()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 2.00m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 3.00m });
        await repo.SaveChangesAsync();

        DynamicQueryOption options = new(
            Filters:
            [
                new Filter
                {
                    Field = "Name",
                    Operator = FilterOperator.Equal,
                    Value = "A",
                },
            ]
        );

        int count = await repo.CountByDynamicAsync(options);
        count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllByDynamicAsync_NoFilters_ShouldReturnAll()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "X", Price = 1.00m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Y", Price = 2.00m });
        await repo.SaveChangesAsync();

        DynamicQueryOption options = new();
        IReadOnlyList<TestProduct> results = await repo.GetAllByDynamicAsync(options);
        results.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetListByDynamicAsync_ShouldReturnPaginatedDynamicResults()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        for (int i = 0; i < 15; i++)
        {
            await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = $"DynProduct_{i}", Price = i * 1.0m });
        }

        await repo.SaveChangesAsync();

        PageableDynamicQueryOption options = new() { Page = new Paging(0, 5) };
        PaginatedList<TestProduct> result = await repo.GetListByDynamicAsync(options);

        result.Items.Count.ShouldBe(5);
        result.Meta.TotalCount.ShouldBe(15);
        result.Meta.TotalPages.ShouldBe(3);
    }

    [Fact]
    public async Task GetAllByDynamicAsync_WithIsNullFilter_ShouldReturnMatches()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(
            new TestProduct(Guid.NewGuid())
            {
                Name = "With Desc",
                Description = "Exists",
                Price = 10.0m,
            }
        );
        await repo.AddAsync(
            new TestProduct(Guid.NewGuid())
            {
                Name = "No Desc",
                Description = null,
                Price = 20.0m,
            }
        );
        await repo.SaveChangesAsync();

        DynamicQueryOption options = new(
            Filters: [new Filter { Field = "Description", Operator = FilterOperator.IsNull }]
        );

        IReadOnlyList<TestProduct> results = await repo.GetAllByDynamicAsync(options);
        results.Count.ShouldBe(1);
        results[0].Price.ShouldBe(20.0m);
    }

    [Fact]
    public async Task GetAllByDynamicAsync_WithStartsWithFilter_ShouldReturnMatches()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Apple", Price = 10.0m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Banana", Price = 20.0m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Apricot", Price = 30.0m });
        await repo.SaveChangesAsync();

        DynamicQueryOption options = new(
            Filters:
            [
                new Filter
                {
                    Field = "Name",
                    Operator = FilterOperator.StartsWith,
                    Value = "Ap",
                },
            ]
        );

        IReadOnlyList<TestProduct> results = await repo.GetAllByDynamicAsync(options);
        results.Count.ShouldBe(2);
    }

    private static readonly string[] Filters = ["Apple", "Cherry"];

    [Fact]
    public async Task GetAllByDynamicAsync_WithInFilter_ShouldReturnMatches()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Apple", Price = 10.0m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Banana", Price = 20.0m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Cherry", Price = 30.0m });
        await repo.SaveChangesAsync();

        DynamicQueryOption options = new(
            Filters:
            [
                new Filter
                {
                    Field = "Name",
                    Operator = FilterOperator.In,
                    Value = Filters,
                },
            ]
        );

        IReadOnlyList<TestProduct> results = await repo.GetAllByDynamicAsync(options);
        results.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllByDynamicAsync_WithIsNotNegation_ShouldReturnMatches()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Apple", Price = 10.0m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Banana", Price = 20.0m });
        await repo.SaveChangesAsync();

        DynamicQueryOption options = new(
            Filters:
            [
                new Filter
                {
                    Field = "Name",
                    Operator = FilterOperator.Equal,
                    Value = "Apple",
                    IsNot = true,
                },
            ]
        );

        IReadOnlyList<TestProduct> results = await repo.GetAllByDynamicAsync(options);
        results.Count.ShouldBe(1);
        results[0].Name.ShouldBe("Banana");
    }

    [Fact]
    public async Task GetAllByDynamicAsync_WithIncludeDeleted_ShouldReturnSoftDeleted()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct active = await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Active" });
        TestProduct deleted = await repo.AddAsync(
            new TestProduct(Guid.NewGuid())
            {
                Name = "Deleted",
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow,
            }
        );
        await repo.SaveChangesAsync();

        DynamicQueryOptionWithSoftDelete options = new(
            Filters:
            [
                new Filter
                {
                    Field = "Name",
                    Operator = FilterOperator.Contains,
                    Value = "e",
                },
            ],
            IncludeDeleted: true
        );

        IReadOnlyList<TestProduct> results = await repo.GetAllByDynamicAsync(options);
        results.Count.ShouldBe(2);

        DynamicQueryOptionWithSoftDelete optionsActiveOnly = new(
            Filters:
            [
                new Filter
                {
                    Field = "Name",
                    Operator = FilterOperator.Contains,
                    Value = "e",
                },
            ],
            IncludeDeleted: false
        );

        IReadOnlyList<TestProduct> resultsActive = await repo.GetAllByDynamicAsync(optionsActiveOnly);
        resultsActive.Count.ShouldBe(1);
        resultsActive[0].Name.ShouldBe("Active");
    }

    [Fact]
    public async Task GetAllByDynamicAsync_WithLogicalOperators_ShouldCombineFilters()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 10 });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 20 });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "C", Price = 30 });
        await repo.SaveChangesAsync();

        DynamicQueryOption options = new(
            Filters:
            [
                new Filter
                {
                    Logic = FilterLogic.Or,
                    Filters =
                    [
                        new Filter
                        {
                            Field = "Name",
                            Operator = FilterOperator.Equal,
                            Value = "A",
                        },
                        new Filter
                        {
                            Field = "Price",
                            Operator = FilterOperator.Equal,
                            Value = 30m,
                        },
                    ],
                },
            ]
        );

        IReadOnlyList<TestProduct> results = await repo.GetAllByDynamicAsync(options);
        results.Count.ShouldBe(2);
        results.ShouldContain(p => p.Name == "A");
        results.ShouldContain(p => p.Name == "C");
    }

    [Fact]
    public async Task GetAllByDynamicAsync_WithOrder_ShouldSortResults()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 20 });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 30 });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "C", Price = 10 });
        await repo.SaveChangesAsync();

        DynamicQueryOption options = new(Orders: [new Order { Field = "Name", Direction = OrderDirection.Asc }]);

        IReadOnlyList<TestProduct> results = await repo.GetAllByDynamicAsync(options);
        results.Count.ShouldBe(3);
        results[0].Name.ShouldBe("A");
        results[1].Name.ShouldBe("B");
        results[2].Name.ShouldBe("C");

        DynamicQueryOption optionsDesc = new(Orders: [new Order { Field = "Price", Direction = OrderDirection.Desc }]);

        IReadOnlyList<TestProduct> resultsDesc = await repo.GetAllByDynamicAsync(optionsDesc);
        resultsDesc.Count.ShouldBe(3);
        resultsDesc[0].Price.ShouldBe(30m);
        resultsDesc[1].Price.ShouldBe(20m);
        resultsDesc[2].Price.ShouldBe(10m);
    }

    [Fact]
    public async Task AnyByDynamicAsync_ShouldReturnTrueIfMatchesExist()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Match" });
        await repo.SaveChangesAsync();

        DynamicQueryOption optionsMatch = new(
            Filters:
            [
                new Filter
                {
                    Field = "Name",
                    Operator = FilterOperator.Equal,
                    Value = "Match",
                },
            ]
        );
        DynamicQueryOption optionsNoMatch = new(
            Filters:
            [
                new Filter
                {
                    Field = "Name",
                    Operator = FilterOperator.Equal,
                    Value = "NoMatch",
                },
            ]
        );

        bool hasMatch = await repo.AnyByDynamicAsync(optionsMatch);
        bool hasNoMatch = await repo.AnyByDynamicAsync(optionsNoMatch);

        hasMatch.ShouldBeTrue();
        hasNoMatch.ShouldBeFalse();
    }

    [Fact]
    public async Task GetAllByDynamicAsync_WithNumericOperators_ShouldReturnMatches()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "10", Price = 10m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "20", Price = 20m });
        await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "30", Price = 30m });
        await repo.SaveChangesAsync();

        // GreaterThan
        IReadOnlyList<TestProduct> gt20 = await repo.GetAllByDynamicAsync(
            new DynamicQueryOption(
                Filters:
                [
                    new Filter
                    {
                        Field = "Price",
                        Operator = FilterOperator.GreaterThan,
                        Value = 20m,
                    },
                ]
            )
        );
        gt20.Count.ShouldBe(1);
        gt20[0].Price.ShouldBe(30m);

        // LessThanOrEqual
        IReadOnlyList<TestProduct> lte20 = await repo.GetAllByDynamicAsync(
            new DynamicQueryOption(
                Filters:
                [
                    new Filter
                    {
                        Field = "Price",
                        Operator = FilterOperator.LessThanOrEqual,
                        Value = 20m,
                    },
                ]
            )
        );
        lte20.Count.ShouldBe(2);
    }
}
