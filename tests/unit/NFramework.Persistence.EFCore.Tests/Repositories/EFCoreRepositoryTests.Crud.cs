using NFramework.Persistence.EFCore.Repositories;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using UnionRailway;
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

        TestProduct product = new(Guid.NewGuid()) { Name = "Widget", Price = 9.99m };
        TestProduct result = (await repo.AddAsync(product)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        result.CreatedAt.ShouldNotBe(default);
        result.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        Guid id = Guid.NewGuid();
        (
            await repo.AddAsync(
                new TestProduct(Guid.NewGuid())
                {
                    Id = id,
                    Name = "Gadget",
                    Price = 19.99m,
                }
            )
        ).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        TestProduct found = (await repo.GetByIdAsync(id)).Unwrap();
        found.ShouldNotBeNull();
        found.Name.ShouldBe("Gadget");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnErrorForMissing()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        Rail<TestProduct> result = await repo.GetByIdAsync(Guid.NewGuid());
        result.IsSuccess(out _, out _).ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTimestamp()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct product = (
            await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "OldName", Price = 5.00m })
        ).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        DateTime? originalUpdatedAt = product.UpdatedAt;

        product.Name = "NewName";
        TestProduct updated = (await repo.UpdateAsync(product)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        updated.UpdatedAt.ShouldNotBeNull();
        updated.UpdatedAt.Value.ShouldBeGreaterThanOrEqualTo(originalUpdatedAt ?? DateTime.MinValue);
        updated.Name.ShouldBe("NewName");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct product = (
            await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Deletable", Price = 1.00m })
        ).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        (await repo.DeleteAsync(product)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        TestProduct found = (await repo.GetByIdAsync(product.Id)).Unwrap();
        found.ShouldNotBeNull();
        found.IsDeleted.ShouldBeTrue();
        found.DeletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnItemCount()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "A", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "B", Price = 2.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        int count = (await repo.CountAsync()).Unwrap();
        count.ShouldBe(2);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueWhenExists()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Exists", Price = 1.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        bool exists = (await repo.AnyAsync(static p => p.Name == "Exists")).Unwrap();
        exists.ShouldBeTrue();

        bool notExists = (await repo.AnyAsync(static p => p.Name == "Nope")).Unwrap();
        notExists.ShouldBeFalse();
    }

    [Fact]
    public async Task BulkAddAsync_ShouldInsertAll()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        List<TestProduct> products =
        [
            new(Guid.NewGuid()) { Name = "Bulk1", Price = 1.00m },
            new(Guid.NewGuid()) { Name = "Bulk2", Price = 2.00m },
            new(Guid.NewGuid()) { Name = "Bulk3", Price = 3.00m },
        ];

        ICollection<TestProduct> result = (await repo.BulkAddAsync(products)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();
        result.Count.ShouldBe(3);

        int count = (await repo.CountAsync()).Unwrap();
        count.ShouldBe(3);
    }

    [Fact]
    public async Task BulkAddAsync_EmptyCollection_ShouldReturnZero()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        ICollection<TestProduct> result = (await repo.BulkAddAsync([])).Unwrap();
        result.Count.ShouldBe(0);
    }

    [Fact]
    public async Task UpsertAsync_ShouldInsertWhenNew()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct product = new(Guid.NewGuid()) { Name = "Upserted", Price = 7.00m };
        TestProduct result = (await repo.UpsertAsync(product)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        result.Name.ShouldBe("Upserted");
        int count = (await repo.CountAsync()).Unwrap();
        count.ShouldBe(1);
    }

    [Fact]
    public async Task UpsertAsync_ShouldUpdateWhenExisting()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        TestProduct product = (
            await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "UpsertedOriginal", Price = 5.00m })
        ).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        product.Name = "UpsertedUpdated";
        TestProduct result = (await repo.UpsertAsync(product)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        result.Name.ShouldBe("UpsertedUpdated");
        int count = (await repo.CountAsync()).Unwrap();
        count.ShouldBe(1);
    }

    [Fact]
    public async Task BulkUpdateAsync_ShouldUpdateAll()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        List<TestProduct> products =
        [
            new(Guid.NewGuid()) { Name = "BulkUpd1", Price = 1.00m },
            new(Guid.NewGuid()) { Name = "BulkUpd2", Price = 2.00m },
        ];

        (await repo.BulkAddAsync(products)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        products[0].Name = "BulkUpd1_Changed";
        products[1].Name = "BulkUpd2_Changed";

        ICollection<TestProduct> result = (await repo.BulkUpdateAsync(products)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();
        result.Count.ShouldBe(2);

        TestProduct found = (await repo.GetByIdAsync(products[0].Id)).Unwrap();
        found.Name.ShouldBe("BulkUpd1_Changed");
    }

    [Fact]
    public async Task BulkDeleteAsync_ShouldDeleteAll()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        List<TestProduct> products =
        [
            new(Guid.NewGuid()) { Name = "BulkDel1", Price = 1.00m },
            new(Guid.NewGuid()) { Name = "BulkDel2", Price = 2.00m },
        ];

        (await repo.BulkAddAsync(products)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        ICollection<TestProduct> result = (await repo.BulkDeleteAsync(products)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();
        result.Count.ShouldBe(2);

        int count = (await repo.CountAsync()).Unwrap();
        count.ShouldBe(0);
    }

    [Fact]
    public async Task CategoryAdd_ShouldSetAuditTimestamps()
    {
        using TestDbContext context = TestDbContext.Create();
        TestCategoryRepository repo = new(context);

        TestCategory category = new(1) { Name = "Electronics" };
        TestCategory result = (await repo.AddAsync(category)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        result.CreatedAt.ShouldNotBe(default);
        result.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithPredicate_ShouldReturnMatchingItems()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Apple", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Banana", Price = 2.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Apricot", Price = 3.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var results = (await repo.GetAllAsync(new(static p => p.Name.StartsWith('A')))).Unwrap();
        results.Count.ShouldBe(2);
        results.ShouldContain(static p => p.Name == "Apple");
        results.ShouldContain(static p => p.Name == "Apricot");
    }

    [Fact]
    public async Task GetAsync_WithPredicate_ShouldReturnFirstMatchingItem()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Apple", Price = 1.00m })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "Banana", Price = 2.00m })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var result = (await repo.GetAsync(static p => p.Name == "Banana")).Unwrap();
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Banana");
    }

    [Fact]
    public async Task UpdateAsync_WithAlreadyTrackedEntity_ShouldNotDuplicateWork()
    {
        using TestDbContext context = TestDbContext.Create();
        TestProductRepository repo = new(context);

        var id = Guid.NewGuid();
        var product = new TestProduct(Guid.NewGuid())
        {
            Id = id,
            Name = "Tracked Item",
            Price = 1.00m,
        };

        (await repo.AddAsync(product)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var trackedEntity = (await repo.GetByIdAsync(id)).Unwrap();
        trackedEntity.ShouldNotBeNull();

        trackedEntity.Price = 50.00m;

        var result = (await repo.UpdateAsync(trackedEntity)).Unwrap();

        (await repo.SaveChangesAsync()).Unwrap();

        var verify = (await repo.GetByIdAsync(id)).Unwrap();
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
            (await repo.GetAllAsync(cancellationToken: cts.Token)).Unwrap()
        );
    }

    [Fact]
    public async Task GetAllAsync_WhenExceedingLimit_ShouldThrowInvalidOperationException()
    {
        using TestDbContext context = TestDbContext.Create();
        LimitedTestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "1", Price = 1 })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "2", Price = 2 })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "3", Price = 3 })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        await Should.ThrowAsync<InvalidOperationException>(async () => (await repo.GetAllAsync()).Unwrap());
    }

    [Fact]
    public async Task GetAllAsync_WhenAtLimit_ShouldSucceed()
    {
        using TestDbContext context = TestDbContext.Create();
        LimitedTestProductRepository repo = new(context);

        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "1", Price = 1 })).Unwrap();
        (await repo.AddAsync(new TestProduct(Guid.NewGuid()) { Name = "2", Price = 2 })).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        var results = (await repo.GetAllAsync()).Unwrap();
        results.Count.ShouldBe(2);
    }

    private class LimitedTestProductRepository(TestDbContext context)
        : EFCoreRepository<TestProduct, Guid, TestDbContext>(context)
    {
        protected override int? MaxResultSetSize => 2;
    }
}
