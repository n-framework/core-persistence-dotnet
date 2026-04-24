using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Repositories;

public class BulkOperationEdgeCaseTests
{
    [Fact]
    public async Task BulkAddAsync_EmptyCollection_ReturnsEmpty()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);

        var result = await repo.BulkAddAsync(new List<TestProduct>());

        Assert.Empty(result);
        Assert.Equal(0, await context.Products.CountAsync());
    }

    [Fact]
    public async Task BulkAddAsync_NullCollection_ThrowsArgumentNullException()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.BulkAddAsync(null!));
    }

    [Fact]
    public async Task BulkAddAsync_ManyItems_Succeeds()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);
        var products = Enumerable
            .Range(1, 100)
            .Select(i => new TestProduct(Guid.NewGuid()) { Name = $"Product {i}", Price = i })
            .ToList();

        var result = await repo.BulkAddAsync(products);
        await repo.SaveChangesAsync();

        Assert.Equal(100, result.Count);
        Assert.Equal(100, await context.Products.CountAsync());
    }

    [Fact]
    public async Task BulkAddAsync_1000Items_Succeeds()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);
        var products = Enumerable
            .Range(1, 1000)
            .Select(i => new TestProduct(Guid.NewGuid()) { Name = $"Product {i}", Price = i })
            .ToList();

        var result = await repo.BulkAddAsync(products);
        await repo.SaveChangesAsync();

        Assert.Equal(1000, result.Count);
        Assert.Equal(1000, await context.Products.CountAsync());
    }

    [Fact]
    public async Task BulkUpdateAsync_MixedNullsAndValid_ThrowsArgumentException()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);
        var p1 = new TestProduct(Guid.NewGuid()) { Name = "Valid", Price = 10 };
        await repo.AddAsync(p1);
        await repo.SaveChangesAsync();

        p1.Name = "Updated";
        var items = new List<TestProduct> { p1, null! };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => repo.BulkUpdateAsync(items));
        Assert.Contains("Collection contains null entities.", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
