using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Interceptors;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Interceptors;

/// <summary>
/// Tests for soft delete querying and filtering logic via the <see cref="AuditSaveChangesInterceptor"/>.
/// </summary>
public sealed class SoftDeleteQueryTests
{
    [Fact]
    public async Task Query_DefaultFilter_ExcludesSoftDeletedEntities()
    {
        // Arrange - QF-01
        using TestDbContext context = TestDbContext.Create();
        TestProduct activeProduct = new() { Id = Guid.NewGuid(), Name = "Active" };
        TestProduct deletedProduct = new() { Id = Guid.NewGuid(), Name = "Deleted" };

        await context.Products.AddRangeAsync(activeProduct, deletedProduct);
        _ = await context.SaveChangesAsync();

        _ = context.Products.Remove(deletedProduct);
        _ = await context.SaveChangesAsync();

        // Act
        List<TestProduct> results = await context.Products.ToListAsync();

        // Assert
        Assert.Single(results);
        Assert.Equal(activeProduct.Id, results[0].Id);
    }

    [Fact]
    public async Task Query_IgnoreQueryFilters_IncludesSoftDeletedEntities()
    {
        // Arrange - QF-02
        using TestDbContext context = TestDbContext.Create();
        TestProduct activeProduct = new() { Id = Guid.NewGuid(), Name = "Active" };
        TestProduct deletedProduct = new() { Id = Guid.NewGuid(), Name = "Deleted" };

        await context.Products.AddRangeAsync(activeProduct, deletedProduct);
        _ = await context.SaveChangesAsync();

        _ = context.Products.Remove(deletedProduct);
        _ = await context.SaveChangesAsync();

        // Act
        List<TestProduct> results = await context.Products.IgnoreQueryFilters().ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, p => p.Id == deletedProduct.Id);
    }
}
