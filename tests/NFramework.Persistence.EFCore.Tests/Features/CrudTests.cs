using NFramework.Persistence.EFCore.Tests.Helpers;
using NFramework.Persistence.EFCore.Tests.Repositories;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Features;

/// <summary>
/// Tests for basic CRUD operations and audit interceptor behavior.
/// </summary>
public class CrudTests
{
    [Fact]
    public async Task AddAsync_ShouldSetTimestamps()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct product = new()
        {
            Id = Guid.NewGuid(),
            Name = "Widget",
            Price = 9.99m,
        };
        TestProduct result = await repo.AddAsync(product);

        result.CreatedAt.ShouldNotBe(default);
        result.UpdatedAt.ShouldNotBe(default);
        result.CreatedAt.ShouldBe(result.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        Guid id = Guid.NewGuid();
        await repo.AddAsync(
            new TestProduct
            {
                Id = id,
                Name = "Gadget",
                Price = 19.99m,
            }
        );

        TestProduct? found = await repo.GetByIdAsync(id);
        found.ShouldNotBeNull();
        found.Name.ShouldBe("Gadget");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullForMissing()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct? found = await repo.GetByIdAsync(Guid.NewGuid());
        found.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTimestamp()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct product = await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "OldName",
                Price = 5.00m,
            }
        );

        DateTime originalUpdatedAt = product.UpdatedAt;

        await Task.Delay(10);
        product.Name = "NewName";
        TestProduct updated = await repo.UpdateAsync(product);

        updated.UpdatedAt.ShouldBeGreaterThanOrEqualTo(originalUpdatedAt);
        updated.Name.ShouldBe("NewName");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct product = await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "Deletable",
                Price = 1.00m,
            }
        );

        await repo.DeleteAsync(product);

        TestProduct? found = await repo.GetByIdAsync(product.Id);
        found.ShouldNotBeNull();
        found.IsDeleted.ShouldBeTrue();
        found.DeletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnItemCount()
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

        int count = await repo.CountAsync();
        count.ShouldBe(2);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueWhenExists()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "Exists",
                Price = 1.00m,
            }
        );

        bool exists = await repo.AnyAsync(p => p.Name == "Exists");
        exists.ShouldBeTrue();

        bool notExists = await repo.AnyAsync(p => p.Name == "Nope");
        notExists.ShouldBeFalse();
    }

    [Fact]
    public async Task BulkAddAsync_ShouldInsertAll()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        List<TestProduct> products =
        [
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Bulk1",
                Price = 1.00m,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Bulk2",
                Price = 2.00m,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Bulk3",
                Price = 3.00m,
            },
        ];

        int result = await repo.BulkAddAsync(products);
        result.ShouldBe(3);

        int count = await repo.CountAsync();
        count.ShouldBe(3);
    }

    [Fact]
    public async Task BulkAddAsync_EmptyCollection_ShouldReturnZero()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        int result = await repo.BulkAddAsync([]);
        result.ShouldBe(0);
    }

    [Fact]
    public async Task UpsertAsync_ShouldInsertWhenNew()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct product = new()
        {
            Id = Guid.NewGuid(),
            Name = "Upserted",
            Price = 7.00m,
        };
        TestProduct result = await repo.UpsertAsync(product);

        result.Name.ShouldBe("Upserted");
        int count = await repo.CountAsync();
        count.ShouldBe(1);
    }

    [Fact]
    public async Task CategoryAdd_ShouldSetAuditTimestamps()
    {
        using TestDbContext context = TestDbContext.Create();
        TestCategoryRepository repo = new(context);

        TestCategory category = new() { Id = 1, Name = "Electronics" };
        TestCategory result = await repo.AddAsync(category);

        result.CreatedAt.ShouldNotBe(default);
        result.UpdatedAt.ShouldNotBe(default);
    }
}
