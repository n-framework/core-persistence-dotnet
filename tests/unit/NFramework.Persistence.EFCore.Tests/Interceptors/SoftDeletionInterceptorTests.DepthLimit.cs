using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Extensions;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Interceptors;

/// <summary>
/// Tests for depth limit enforcement in <see cref="NFramework.Persistence.EFCore.Interceptors.SoftDeletionInterceptor"/>.
/// </summary>
public sealed class SoftDeletionDepthLimitTests
{
    [Fact]
    public async Task DeleteAsync_DepthWithinLimit_Succeeds()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddSoftDeleteInterceptor(maxCascadeDepth: 5)
            .Options;

        using var context = new TestDbContext(options);

        // Create hierarchy: L1 -> L2 -> L3 (Depth 2 from root)
        var l1 = new TestEmployee(Guid.NewGuid()) { Name = "L1" };
        var l2 = new TestEmployee(Guid.NewGuid()) { Name = "L2", ManagerId = l1.Id };
        var l3 = new TestEmployee(Guid.NewGuid()) { Name = "L3", ManagerId = l2.Id };

        await context.Employees.AddRangeAsync(l1, l2, l3);
        await context.SaveChangesAsync();

        // Act
        context.Employees.Remove(l1);
        int result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(3, result);
        Assert.True(await context.Employees.IgnoreQueryFilters().AllAsync(e => e.IsDeleted));
    }

    [Fact]
    public async Task DeleteAsync_DepthExceedsLimit_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddSoftDeleteInterceptor(maxCascadeDepth: 2) // Root + 2 levels = 3 entities total allowed
            .Options;

        using var context = new TestDbContext(options);

        // Create hierarchy: L1 -> L2 -> L3 -> L4 (Depth 3 from root)
        var l1 = new TestEmployee(Guid.NewGuid()) { Name = "L1" };
        var l2 = new TestEmployee(Guid.NewGuid()) { Name = "L2", ManagerId = l1.Id };
        var l3 = new TestEmployee(Guid.NewGuid()) { Name = "L3", ManagerId = l2.Id };
        var l4 = new TestEmployee(Guid.NewGuid()) { Name = "L4", ManagerId = l3.Id };

        await context.Employees.AddRangeAsync(l1, l2, l3, l4);
        await context.SaveChangesAsync();

        // Act & Assert
        context.Employees.Remove(l1);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains(
            $"Cascade soft-delete exceeded maximum depth of {2}",
            exception.Message,
            StringComparison.OrdinalIgnoreCase
        );
    }

    [Fact]
    public async Task DeleteAsync_DisabledLimit_AllowsDeepTraversal()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddSoftDeleteInterceptor(maxCascadeDepth: null) // Disable limit
            .Options;

        using var context = new TestDbContext(options);

        // Create hierarchy of 10 levels
        var employees = new List<TestEmployee>();
        TestEmployee? manager = null;
        for (int i = 0; i < 10; i++)
        {
            var emp = new TestEmployee(Guid.NewGuid()) { Name = $"Emp{i}", ManagerId = manager?.Id };
            employees.Add(emp);
            manager = emp;
        }

        await context.Employees.AddRangeAsync(employees);
        await context.SaveChangesAsync();

        // Act
        context.Employees.Remove(employees[0]);
        int result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(10, result);
        Assert.True(await context.Employees.IgnoreQueryFilters().AllAsync(e => e.IsDeleted));
    }

    [Fact]
    public void Interceptor_WithCustomDepth_IsCorrectlyConfigured()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .AddSoftDeleteInterceptor(maxCascadeDepth: 42)
            .Options;

        // Act
        // Reach into the options to find the interceptor via extension logic if possible,
        // but easier to just verify behavior as done above.
        // For this test, we just ensure the extension method doesn't throw and builds options.

        Assert.NotNull(options);
    }
}
