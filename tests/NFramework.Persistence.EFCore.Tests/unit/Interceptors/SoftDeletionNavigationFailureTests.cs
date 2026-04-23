using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Tests.Unit.Helpers;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Unit.Interceptors;

public class SoftDeletionNavigationFailureTests
{
    [Fact]
    public async Task SavingChanges_WhenNavigationLoadingFails_ThrowsInvalidOperationExceptionWithContext()
    {
        // Arrange
        using var context = TestDbContext.CreateSqlite();
        var order = new TestOrder { Id = Guid.NewGuid(), OrderNumber = "FAIL-LOAD" };
        var item = new TestOrderItem { Id = Guid.NewGuid(), Order = order };

        context.Orders.Add(order);
        context.OrderItems.Add(item);
        await context.SaveChangesAsync();

        // Clear tracker to force loading
        context.ChangeTracker.Clear();

        // Re-attach parent as deleted
        var trackedOrder = context.Orders.Attach(order);
        trackedOrder.State = EntityState.Deleted;

        // Act & Assert
        // We close the connection to force an exception during Load()
        await context.Database.GetDbConnection().CloseAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());

        Assert.Contains("Failed to load navigation collection 'Items'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("TestOrder", exception.Message, StringComparison.Ordinal);
    }
}
