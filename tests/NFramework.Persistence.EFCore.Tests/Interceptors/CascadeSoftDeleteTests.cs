using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Interceptors;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Interceptors;

/// <summary>
/// Tests for cascade soft-delete behavior via the <see cref="AuditSaveChangesInterceptor"/>.
/// </summary>
public sealed class CascadeSoftDeleteTests
{
    [Fact]
    public async Task DeleteAsync_ParentWithChildren_CascadesSoftDeleteToChildren()
    {
        // Arrange
        using TestDbContext context = TestDbContext.Create();
        TestOrder order = new() { Id = Guid.NewGuid(), OrderNumber = "ORD-001" };
        TestOrderItem item1 = new()
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Description = "Item 1",
        };
        TestOrderItem item2 = new()
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Description = "Item 2",
        };

        await context.Orders.AddAsync(order);
        await context.OrderItems.AddRangeAsync(item1, item2);
        _ = await context.SaveChangesAsync();

        // Act — Remove the parent, triggering interceptor cascade
        _ = context.Orders.Remove(order);
        _ = await context.SaveChangesAsync();

        // Assert — Parent and children should be soft-deleted, not hard-deleted
        TestOrder? parentInDb = await context.Orders.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Id == order.Id);
        Assert.NotNull(parentInDb);
        Assert.True(parentInDb.IsDeleted);
        Assert.NotNull(parentInDb.DeletedAt);

        List<TestOrderItem> itemsInDb = await context
            .OrderItems.IgnoreQueryFilters()
            .Where(i => i.OrderId == order.Id)
            .ToListAsync();

        Assert.Equal(2, itemsInDb.Count);
        Assert.All(
            itemsInDb,
            item =>
            {
                Assert.True(item.IsDeleted);
                Assert.NotNull(item.DeletedAt);
            }
        );
    }

    [Fact]
    public async Task DeleteAsync_ParentWithChildren_AllShareSameDeletedAtTimestamp()
    {
        // Arrange
        using TestDbContext context = TestDbContext.Create();
        TestOrder order = new() { Id = Guid.NewGuid(), OrderNumber = "ORD-002" };
        TestOrderItem item = new()
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Description = "Single Item",
        };

        await context.Orders.AddAsync(order);
        await context.OrderItems.AddAsync(item);
        _ = await context.SaveChangesAsync();

        // Act
        _ = context.Orders.Remove(order);
        _ = await context.SaveChangesAsync();

        // Assert — Parent and child should share the same DeletedAt timestamp
        TestOrder? parent = await context.Orders.IgnoreQueryFilters().FirstAsync(o => o.Id == order.Id);
        TestOrderItem? child = await context.OrderItems.IgnoreQueryFilters().FirstAsync(i => i.Id == item.Id);

        Assert.Equal(parent.DeletedAt, child.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_ChildAlreadyDeleted_DoesNotReDelete()
    {
        // Arrange
        using TestDbContext context = TestDbContext.Create();
        TestOrder order = new() { Id = Guid.NewGuid(), OrderNumber = "ORD-003" };
        TestOrderItem item = new()
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Description = "Pre-deleted",
        };

        await context.Orders.AddAsync(order);
        await context.OrderItems.AddAsync(item);
        _ = await context.SaveChangesAsync();

        // Pre-delete the child
        DateTime originalDeleteTime = DateTime.UtcNow.AddDays(-1);
        item.IsDeleted = true;
        item.DeletedAt = originalDeleteTime;
        _ = await context.SaveChangesAsync();

        // Act — Delete the parent
        _ = context.Orders.Remove(order);
        _ = await context.SaveChangesAsync();

        // Assert — Child should retain its original deletion timestamp
        TestOrderItem? childInDb = await context.OrderItems.IgnoreQueryFilters().FirstAsync(i => i.Id == item.Id);
        Assert.True(childInDb.IsDeleted);
        Assert.Equal(originalDeleteTime, childInDb.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_ParentWithNoChildren_SoftDeletesOnlyParent()
    {
        // Arrange
        using TestDbContext context = TestDbContext.Create();
        TestOrder order = new() { Id = Guid.NewGuid(), OrderNumber = "ORD-004" };

        await context.Orders.AddAsync(order);
        _ = await context.SaveChangesAsync();

        // Act
        _ = context.Orders.Remove(order);
        _ = await context.SaveChangesAsync();

        // Assert
        TestOrder? parent = await context.Orders.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Id == order.Id);
        Assert.NotNull(parent);
        Assert.True(parent.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_NonSoftDeletableEntity_HardDeletes()
    {
        // Arrange — TestCategory is AuditableEntity, NOT SoftDeletableEntity
        using TestDbContext context = TestDbContext.Create();
        TestCategory category = new() { Id = 1, Name = "Electronics" };

        await context.Categories.AddAsync(category);
        _ = await context.SaveChangesAsync();

        // Act
        _ = context.Categories.Remove(category);
        _ = await context.SaveChangesAsync();

        // Assert — Should be hard-deleted, not present in DB
        TestCategory? result = await context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == 1);
        Assert.Null(result);
    }

    [Fact]
    public async Task BulkDelete_MultipleParents_CascadesToAllChildren()
    {
        // Arrange
        using TestDbContext context = TestDbContext.Create();
        TestOrder order1 = new() { Id = Guid.NewGuid(), OrderNumber = "ORD-005" };
        TestOrder order2 = new() { Id = Guid.NewGuid(), OrderNumber = "ORD-006" };
        TestOrderItem item1 = new()
        {
            Id = Guid.NewGuid(),
            OrderId = order1.Id,
            Description = "Item A",
        };
        TestOrderItem item2 = new()
        {
            Id = Guid.NewGuid(),
            OrderId = order2.Id,
            Description = "Item B",
        };
        TestOrderItem item3 = new()
        {
            Id = Guid.NewGuid(),
            OrderId = order2.Id,
            Description = "Item C",
        };

        await context.Orders.AddRangeAsync(order1, order2);
        await context.OrderItems.AddRangeAsync(item1, item2, item3);
        _ = await context.SaveChangesAsync();

        // Act
        context.Orders.RemoveRange(order1, order2);
        _ = await context.SaveChangesAsync();

        // Assert
        List<TestOrder> orders = await context.Orders.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, orders.Count);
        Assert.All(orders, o => Assert.True(o.IsDeleted));

        List<TestOrderItem> items = await context.OrderItems.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(3, items.Count);
        Assert.All(items, i => Assert.True(i.IsDeleted));
    }

    [Fact]
    public async Task DeleteAsync_MultiLevelHierarchy_CascadesToAllLevels()
    {
        // Arrange
        using TestDbContext context = TestDbContext.Create();
        TestOrder order = new() { Id = Guid.NewGuid(), OrderNumber = "ORD-007" };
        TestOrderItem item = new() { Id = Guid.NewGuid(), OrderId = order.Id, Description = "Item X" };
        TestOrderSubItem subItem = new() { Id = Guid.NewGuid(), ItemId = item.Id, Details = "Sub-item Y" };

        await context.Orders.AddAsync(order);
        await context.OrderItems.AddAsync(item);
        await context.OrderSubItems.AddAsync(subItem);
        _ = await context.SaveChangesAsync();

        // Act
        _ = context.Orders.Remove(order);
        _ = await context.SaveChangesAsync();

        // Assert
        TestOrder? orderDb = await context.Orders.IgnoreQueryFilters().FirstAsync(o => o.Id == order.Id);
        TestOrderItem? itemDb = await context.OrderItems.IgnoreQueryFilters().FirstAsync(i => i.Id == item.Id);
        TestOrderSubItem? subItemDb = await context.OrderSubItems.IgnoreQueryFilters().FirstAsync(s => s.Id == subItem.Id);

        Assert.True(orderDb.IsDeleted);
        Assert.True(itemDb.IsDeleted);
        Assert.True(subItemDb.IsDeleted);
        Assert.Equal(orderDb.DeletedAt, itemDb.DeletedAt);
        Assert.Equal(orderDb.DeletedAt, subItemDb.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_SelfReferencingHierarchy_CascadesToAllSubordinates()
    {
        // Arrange
        using TestDbContext context = TestDbContext.Create();
        TestEmployee ceo = new() { Id = Guid.NewGuid(), Name = "CEO" };
        TestEmployee vp = new() { Id = Guid.NewGuid(), Name = "VP", ManagerId = ceo.Id };
        TestEmployee dev = new() { Id = Guid.NewGuid(), Name = "Dev", ManagerId = vp.Id };

        await context.Employees.AddRangeAsync(ceo, vp, dev);
        _ = await context.SaveChangesAsync();

        // Act - Fire CEO, which should cascade to VP and Dev
        _ = context.Employees.Remove(ceo);
        _ = await context.SaveChangesAsync();

        // Assert
        TestEmployee? ceoDb = await context.Employees.IgnoreQueryFilters().FirstAsync(e => e.Id == ceo.Id);
        TestEmployee? vpDb = await context.Employees.IgnoreQueryFilters().FirstAsync(e => e.Id == vp.Id);
        TestEmployee? devDb = await context.Employees.IgnoreQueryFilters().FirstAsync(e => e.Id == dev.Id);

        Assert.True(ceoDb.IsDeleted);
        Assert.True(vpDb.IsDeleted);
        Assert.True(devDb.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeleteSingleChild_LeavesParentIntact()
    {
        // Arrange
        using TestDbContext context = TestDbContext.Create();
        TestOrder order = new() { Id = Guid.NewGuid(), OrderNumber = "ORD-008" };
        TestOrderItem item = new() { Id = Guid.NewGuid(), OrderId = order.Id, Description = "Item Z" };

        await context.Orders.AddAsync(order);
        await context.OrderItems.AddAsync(item);
        _ = await context.SaveChangesAsync();

        // Act - Delete just the child
        _ = context.OrderItems.Remove(item);
        _ = await context.SaveChangesAsync();

        // Assert
        TestOrder? orderDb = await context.Orders.IgnoreQueryFilters().FirstAsync(o => o.Id == order.Id);
        TestOrderItem? itemDb = await context.OrderItems.IgnoreQueryFilters().FirstAsync(i => i.Id == item.Id);

        Assert.False(orderDb.IsDeleted);
        Assert.True(itemDb.IsDeleted);
    }
}
