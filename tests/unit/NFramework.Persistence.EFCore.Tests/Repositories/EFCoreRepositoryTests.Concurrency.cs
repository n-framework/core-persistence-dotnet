using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using UnionRailway;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Repositories;

/// <summary>
/// Concurrency conflict detection tests using SQLite provider.
/// SQLite enforces RowVersion concurrency tokens, unlike InMemory.
/// </summary>
public class ConcurrencyConflictTests
{
    [Fact]
    public async Task UpdateAsync_WithStaleRowVersion_ShouldReturnConflict()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        TestProduct product = new(Guid.NewGuid()) { Name = "Original", Price = 10.00m };

        (await repo.AddAsync(product)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Name = 'ConcurrentUpdate', RowVersion = X'0102' WHERE Id = {0}",
            product.Id
        );

        product.Name = "MyUpdate";
        (await repo.UpdateAsync(product)).Unwrap();
        Rail<int> saveResult = await repo.SaveChangesAsync();

        saveResult.IsSuccess(out _, out UnionError? error).ShouldBeFalse();
        error!.Value.TryGet(out UnionError.Conflict? _).ShouldBeTrue();
    }

    [Fact]
    public async Task UpsertAsync_WithStaleRowVersion_ShouldReturnConflict()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        TestProduct product = new(Guid.NewGuid()) { Name = "Original", Price = 10.00m };

        (await repo.AddAsync(product)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        byte[] originalRowVersion = product.RowVersion;

        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Name = 'ConcurrentUpdate', RowVersion = X'0102' WHERE Id = {0}",
            product.Id
        );

        context.Entry(product).State = EntityState.Detached;

        TestProduct staleEntity = new(product.Id)
        {
            Name = "MyUpsert",
            Price = 20.00m,
            RowVersion = originalRowVersion,
        };

        (await repo.UpsertAsync(staleEntity)).Unwrap();
        Rail<int> saveResult = await repo.SaveChangesAsync();

        saveResult.IsSuccess(out _, out UnionError? error).ShouldBeFalse();
        error!.Value.TryGet(out UnionError.Conflict? _).ShouldBeTrue();
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoConcurrencyConflict_ShouldSucceed()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        TestProduct product = new(Guid.NewGuid()) { Name = "TestProduct", Price = 5.00m };

        (await repo.AddAsync(product)).Unwrap();
        int result = (await repo.SaveChangesAsync()).Unwrap();

        result.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task DeleteAsync_WithStaleRowVersion_ShouldReturnConflict()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        TestProduct product = new(Guid.NewGuid()) { Name = "Original", Price = 10.00m };

        (await repo.AddAsync(product)).Unwrap();
        (await repo.SaveChangesAsync()).Unwrap();

        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Name = 'ConcurrentUpdate', RowVersion = X'0102' WHERE Id = {0}",
            product.Id
        );

        (await repo.DeleteAsync(product)).Unwrap();
        Rail<int> saveResult = await repo.SaveChangesAsync();

        saveResult.IsSuccess(out _, out UnionError? error).ShouldBeFalse();
        error!.Value.TryGet(out UnionError.Conflict? _).ShouldBeTrue();
    }

    [Fact]
    public async Task BulkUpdateAsync_WithStaleRowVersion_ShouldReturnConflict()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        List<TestProduct> products =
        [
            new(Guid.NewGuid()) { Name = "Bulk1", Price = 10.00m },
            new(Guid.NewGuid()) { Name = "Bulk2", Price = 20.00m },
        ];

        (await repo.BulkAddAsync(products)).Unwrap();

        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Name = 'ConcurrentUpdate', RowVersion = X'0102' WHERE Id = {0}",
            products[0].Id
        );

        products[0].Name = "MyBulkUpdate1";
        products[1].Name = "MyBulkUpdate2";

        Rail<ICollection<TestProduct>> result = await repo.BulkUpdateAsync(products);
        result.IsSuccess(out _, out UnionError? error).ShouldBeFalse();
        error!.Value.TryGet(out UnionError.Conflict? _).ShouldBeTrue();
    }

    [Fact]
    public async Task BulkDeleteAsync_WithStaleRowVersion_ShouldReturnConflict()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        List<TestProduct> products =
        [
            new(Guid.NewGuid()) { Name = "Bulk1", Price = 10.00m },
            new(Guid.NewGuid()) { Name = "Bulk2", Price = 20.00m },
        ];

        (await repo.BulkAddAsync(products)).Unwrap();

        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Name = 'ConcurrentUpdate', RowVersion = X'0102' WHERE Id = {0}",
            products[0].Id
        );

        Rail<ICollection<TestProduct>> result = await repo.BulkDeleteAsync(products);
        result.IsSuccess(out _, out UnionError? error).ShouldBeFalse();
        error!.Value.TryGet(out UnionError.Conflict? _).ShouldBeTrue();
    }
}
