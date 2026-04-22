using NFramework.Persistence.Abstractions.Dynamic;
using NFramework.Persistence.Abstractions.Pagination;
using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Tests.Helpers;
using NFramework.Persistence.EFCore.Tests.Repositories;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Features;

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
            await repo.AddAsync(
                new TestProduct
                {
                    Id = Guid.NewGuid(),
                    Name = $"Product_{i}",
                    Price = i * 1.0m,
                }
            );
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
            await repo.AddAsync(
                new TestProduct
                {
                    Id = Guid.NewGuid(),
                    Name = $"Product_{i}",
                    Price = i * 1.0m,
                }
            );
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

        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "Alpha",
                Price = 10.00m,
            }
        );
        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "Beta",
                Price = 20.00m,
            }
        );
        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "Alpha",
                Price = 30.00m,
            }
        );
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

        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "A",
                Price = 1.00m,
            }
        );
        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "B",
                Price = 2.00m,
            }
        );
        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "A",
                Price = 3.00m,
            }
        );
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

        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "X",
                Price = 1.00m,
            }
        );
        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "Y",
                Price = 2.00m,
            }
        );
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
            await repo.AddAsync(
                new TestProduct
                {
                    Id = Guid.NewGuid(),
                    Name = $"DynProduct_{i}",
                    Price = i * 1.0m,
                }
            );
        }

        await repo.SaveChangesAsync();

        PageableDynamicQueryOption options = new() { Page = new Paging(0, 5) };
        PaginatedList<TestProduct> result = await repo.GetListByDynamicAsync(options);

        result.Items.Count.ShouldBe(5);
        result.Meta.TotalCount.ShouldBe(15);
        result.Meta.TotalPages.ShouldBe(3);
    }
}
