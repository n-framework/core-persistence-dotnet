using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Repositories;

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
        await repo.SaveChangesAsync();

        result.CreatedAt.ShouldNotBe(default);
        result.UpdatedAt.ShouldBeNull();
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
        await repo.SaveChangesAsync();

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
        await repo.SaveChangesAsync();

        DateTime? originalUpdatedAt = product.UpdatedAt;

        product.Name = "NewName";
        TestProduct updated = await repo.UpdateAsync(product);
        await repo.SaveChangesAsync();

        updated.UpdatedAt.ShouldNotBeNull();
        updated.UpdatedAt.Value.ShouldBeGreaterThanOrEqualTo(originalUpdatedAt ?? DateTime.MinValue);
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
        await repo.SaveChangesAsync();

        await repo.DeleteAsync(product);
        await repo.SaveChangesAsync();

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
        await repo.SaveChangesAsync();

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
        await repo.SaveChangesAsync();

        bool exists = await repo.AnyAsync(static p => p.Name == "Exists");
        exists.ShouldBeTrue();

        bool notExists = await repo.AnyAsync(static p => p.Name == "Nope");
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

        ICollection<TestProduct> result = await repo.BulkAddAsync(products);
        await repo.SaveChangesAsync();
        result.Count.ShouldBe(3);

        int count = await repo.CountAsync();
        count.ShouldBe(3);
    }

    [Fact]
    public async Task BulkAddAsync_EmptyCollection_ShouldReturnZero()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        ICollection<TestProduct> result = await repo.BulkAddAsync([]);
        result.Count.ShouldBe(0);
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
        await repo.SaveChangesAsync();

        result.Name.ShouldBe("Upserted");
        int count = await repo.CountAsync();
        count.ShouldBe(1);
    }

    [Fact]
    public async Task UpsertAsync_ShouldUpdateWhenExisting()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct product = await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "UpsertedOriginal",
                Price = 5.00m,
            }
        );
        await repo.SaveChangesAsync();

        product.Name = "UpsertedUpdated";
        TestProduct result = await repo.UpsertAsync(product);
        await repo.SaveChangesAsync();

        result.Name.ShouldBe("UpsertedUpdated");
        int count = await repo.CountAsync();
        count.ShouldBe(1);
    }

    [Fact]
    public async Task BulkUpdateAsync_ShouldUpdateAll()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        List<TestProduct> products =
        [
            new()
            {
                Id = Guid.NewGuid(),
                Name = "BulkUpd1",
                Price = 1.00m,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "BulkUpd2",
                Price = 2.00m,
            },
        ];

        await repo.BulkAddAsync(products);
        await repo.SaveChangesAsync();

        products[0].Name = "BulkUpd1_Changed";
        products[1].Name = "BulkUpd2_Changed";

        ICollection<TestProduct> result = await repo.BulkUpdateAsync(products);
        await repo.SaveChangesAsync();
        result.Count.ShouldBe(2);

        TestProduct? found = await repo.GetByIdAsync(products[0].Id);
        found!.Name.ShouldBe("BulkUpd1_Changed");
    }

    [Fact]
    public async Task BulkDeleteAsync_ShouldDeleteAll()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        List<TestProduct> products =
        [
            new()
            {
                Id = Guid.NewGuid(),
                Name = "BulkDel1",
                Price = 1.00m,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "BulkDel2",
                Price = 2.00m,
            },
        ];

        await repo.BulkAddAsync(products);
        await repo.SaveChangesAsync();

        ICollection<TestProduct> result = await repo.BulkDeleteAsync(products);
        await repo.SaveChangesAsync();
        result.Count.ShouldBe(2);

        int count = await repo.CountAsync();
        count.ShouldBe(0);
    }

    [Fact]
    public async Task CategoryAdd_ShouldSetAuditTimestamps()
    {
        using TestDbContext context = TestDbContext.Create();
        TestCategoryRepository repo = new(context);

        TestCategory category = new() { Id = 1, Name = "Electronics" };
        TestCategory result = await repo.AddAsync(category);
        await repo.SaveChangesAsync();

        result.CreatedAt.ShouldNotBe(default);
        result.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithPredicate_ShouldReturnMatchingItems()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "Apple",
                Price = 1.00m,
            }
        );
        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "Banana",
                Price = 2.00m,
            }
        );
        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "Apricot",
                Price = 3.00m,
            }
        );
        await repo.SaveChangesAsync();

        var results = await repo.GetAllAsync(new(static p => p.Name.StartsWith('A')));
        results.Count.ShouldBe(2);
        results.ShouldContain(static p => p.Name == "Apple");
        results.ShouldContain(static p => p.Name == "Apricot");
    }

    [Fact]
    public async Task GetAsync_WithPredicate_ShouldReturnFirstMatchingItem()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "Apple",
                Price = 1.00m,
            }
        );
        await repo.AddAsync(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "Banana",
                Price = 2.00m,
            }
        );
        await repo.SaveChangesAsync();

        var result = await repo.GetAsync(static p => p.Name == "Banana");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Banana");
    }

    [Fact]
    public async Task UpdateAsync_WithAlreadyTrackedEntity_ShouldNotDuplicateWork()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        var id = Guid.NewGuid();
        var product = new TestProduct
        {
            Id = id,
            Name = "Tracked Item",
            Price = 1.00m,
        };

        await repo.AddAsync(product);
        await repo.SaveChangesAsync();

        // 1. Fetch the entity (now it's tracked by ChangeTracker)
        var trackedEntity = await repo.GetByIdAsync(id);
        trackedEntity.ShouldNotBeNull();

        // 2. Modify properties
        trackedEntity.Price = 50.00m;

        // 3. Update the identical tracked entity
        // With our ReferenceEquals optimization, this should return immediately without doing SetValues
        var result = await repo.UpdateAsync(trackedEntity);

        // 4. Save Changes
        await repo.SaveChangesAsync();

        // 5. Verify the save went through
        var verify = await repo.GetByIdAsync(id);
        verify.ShouldNotBeNull();
        verify.Price.ShouldBe(50.00m);
        ReferenceEquals(result, trackedEntity).ShouldBeTrue();
    }

    [Fact]
    public async Task GetAllAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await repo.GetAllAsync(cancellationToken: cts.Token)
        );
    }
}
