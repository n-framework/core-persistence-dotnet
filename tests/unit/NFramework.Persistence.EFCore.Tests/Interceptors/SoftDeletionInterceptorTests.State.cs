using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Interceptors;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Interceptors;

/// <summary>
/// Tests for state changes and timestamp behavior via the <see cref="SoftDeletionInterceptor"/> and <see cref="AuditableInterceptor"/>.
/// </summary>
public sealed class SoftDeleteStateTests
{
    [Fact]
    public async Task SaveChanges_PartialModification_UpdatesTimestampOnly()
    {
        // Arrange - SU-01
        using TestDbContext context = TestDbContext.Create();
        TestProduct product = new() { Id = Guid.NewGuid(), Name = "Original" };

        await context.Products.AddAsync(product);
        _ = await context.SaveChangesAsync();
        DateTime? originalUpdatedAt = product.UpdatedAt;

        // Act
        product.Name = "New Name";
        _ = await context.SaveChangesAsync();

        // Assert
        Assert.False(product.IsDeleted);
        Assert.Null(product.DeletedAt);
        Assert.True(product.UpdatedAt > (originalUpdatedAt ?? DateTime.MinValue));
    }

    [Fact]
    public async Task SaveChanges_NoModifications_TimestampsUnchanged()
    {
        // Arrange - SU-04
        using TestDbContext context = TestDbContext.Create();
        TestProduct product = new() { Id = Guid.NewGuid(), Name = "Original" };

        await context.Products.AddAsync(product);
        _ = await context.SaveChangesAsync();
        DateTime? originalUpdatedAt = product.UpdatedAt;

        // Act - No changes, just SaveChanges
        _ = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(originalUpdatedAt, product.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_ParentWithNonDeletableChild_HardDeletesChildAndSoftDeletesParent()
    {
        // Arrange - CB-04 (Non-deletable child logic)
        using TestDbContext context = TestDbContext.Create();
        TestOrder order = new() { Id = Guid.NewGuid(), OrderNumber = "ORD-009" };
        TestOrderLog log = new()
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Message = "Created",
        };

        await context.Orders.AddAsync(order);
        await context.OrderLogs.AddAsync(log);
        _ = await context.SaveChangesAsync();

        // Act
        _ = context.Orders.Remove(order);
        _ = await context.SaveChangesAsync();

        // Assert
        TestOrder? dbOrder = await context.Orders.IgnoreQueryFilters().FirstAsync(o => o.Id == order.Id);

        // Assert child is physically deleted!
        TestOrderLog? dbLog = await context.OrderLogs.FirstOrDefaultAsync(l => l.Id == log.Id);

        Assert.True(dbOrder.IsDeleted);
        Assert.Null(dbLog); // Log is hard-deleted because the PARENT triggered Delete and Log is not soft-deletable!
    }
}
