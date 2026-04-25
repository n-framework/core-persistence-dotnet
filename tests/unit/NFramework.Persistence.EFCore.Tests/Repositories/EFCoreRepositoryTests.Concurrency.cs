using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Exceptions;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Repositories;

/// <summary>
/// Concurrency conflict detection tests using SQLite provider.
/// SQLite enforces RowVersion concurrency tokens, unlike InMemory.
/// </summary>
public class ConcurrencyConflictTests
{
    [Fact]
    public async Task UpdateAsync_WithStaleRowVersion_ShouldThrowConcurrencyConflictException()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        TestProduct product = new(Guid.NewGuid()) { Name = "Original", Price = 10.00m };

        await repo.AddAsync(product);
        await repo.SaveChangesAsync();

        // Simulate a concurrent modification by directly updating the database
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Name = 'ConcurrentUpdate', RowVersion = X'0102' WHERE Id = {0}",
            product.Id
        );

        // Now try to update with the stale entity (still has old RowVersion)
        product.Name = "MyUpdate";

        await Should.ThrowAsync<ConcurrencyConflictException>(async () =>
        {
            _ = await repo.UpdateAsync(product);
            await repo.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task UpsertAsync_WithStaleRowVersion_ShouldThrowConcurrencyConflictException()
    {
        using SqliteTestDbContext context1 = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo1 = new(context1);

        TestProduct product = new(Guid.NewGuid()) { Name = "Original", Price = 10.00m };

        await repo1.AddAsync(product);
        await repo1.SaveChangesAsync();

        byte[] originalRowVersion = product.RowVersion;

        // Simulate concurrent modification
        await context1.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Name = 'ConcurrentUpdate', RowVersion = X'0102' WHERE Id = {0}",
            product.Id
        );

        // Detach the entity so UpsertAsync will FindAsync + SetValues
        context1.Entry(product).State = EntityState.Detached;

        // Create a stale copy with the old RowVersion
        TestProduct staleEntity = new(product.Id)
        {
            Name = "MyUpsert",
            Price = 20.00m,
            RowVersion = originalRowVersion,
        };

        await repo1.UpsertAsync(staleEntity);

        await Should.ThrowAsync<ConcurrencyConflictException>(async () =>
        {
            await repo1.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoConcurrencyConflict_ShouldSucceed()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        TestProduct product = new(Guid.NewGuid()) { Name = "TestProduct", Price = 5.00m };

        await repo.AddAsync(product);
        int result = await repo.SaveChangesAsync();

        result.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ConcurrencyConflictException_ShouldWrapOriginalException()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        TestProduct product = new(Guid.NewGuid()) { Name = "Original", Price = 10.00m };

        await repo.AddAsync(product);
        await repo.SaveChangesAsync();

        // Simulate a concurrent modification
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Name = 'ConcurrentUpdate', RowVersion = X'0102' WHERE Id = {0}",
            product.Id
        );

        product.Name = "MyUpdate";

        ConcurrencyConflictException ex = await Should.ThrowAsync<ConcurrencyConflictException>(async () =>
        {
            _ = await repo.UpdateAsync(product);
            await repo.SaveChangesAsync();
        });

        ex.InnerException.ShouldNotBeNull();
        ex.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
    }

    [Fact]
    public async Task BulkUpdateAsync_WithStaleRowVersion_ShouldThrowConcurrencyConflictException()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        List<TestProduct> products =
        [
            new(Guid.NewGuid()) { Name = "Bulk1", Price = 10.00m },
            new(Guid.NewGuid()) { Name = "Bulk2", Price = 20.00m },
        ];

        await repo.BulkAddAsync(products);
        await repo.SaveChangesAsync();

        // Simulate concurrent modification on the first entity
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Name = 'ConcurrentUpdate', RowVersion = X'0102' WHERE Id = {0}",
            products[0].Id
        );

        products[0].Name = "MyBulkUpdate1";
        products[1].Name = "MyBulkUpdate2";

        await Should.ThrowAsync<ConcurrencyConflictException>(async () =>
        {
            await repo.BulkUpdateAsync(products);
            await repo.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task DeleteAsync_WithStaleRowVersion_ShouldThrowConcurrencyConflictException()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        TestProduct product = new(Guid.NewGuid()) { Name = "Original", Price = 10.00m };

        await repo.AddAsync(product);
        await repo.SaveChangesAsync();

        // Simulate concurrent modification
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Name = 'ConcurrentUpdate', RowVersion = X'0102' WHERE Id = {0}",
            product.Id
        );

        // Delete with stale entity
        await Should.ThrowAsync<ConcurrencyConflictException>(async () =>
        {
            await repo.DeleteAsync(product);
            await repo.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task BulkDeleteAsync_WithStaleRowVersion_ShouldThrowConcurrencyConflictException()
    {
        using SqliteTestDbContext context = await SqliteTestDbContext.CreateAsync();
        SqliteTestProductRepository repo = new(context);

        List<TestProduct> products =
        [
            new(Guid.NewGuid()) { Name = "Bulk1", Price = 10.00m },
            new(Guid.NewGuid()) { Name = "Bulk2", Price = 20.00m },
        ];

        await repo.BulkAddAsync(products);
        await repo.SaveChangesAsync();

        // Simulate concurrent modification on the first entity
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE Products SET Name = 'ConcurrentUpdate', RowVersion = X'0102' WHERE Id = {0}",
            products[0].Id
        );

        await Should.ThrowAsync<ConcurrencyConflictException>(async () =>
        {
            await repo.BulkDeleteAsync(products);
            await repo.SaveChangesAsync();
        });
    }
}
