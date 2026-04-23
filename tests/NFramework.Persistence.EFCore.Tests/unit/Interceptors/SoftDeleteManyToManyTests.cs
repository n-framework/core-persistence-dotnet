using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Interceptors;
using NFramework.Persistence.EFCore.Tests.Unit.Helpers;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Unit.Interceptors;

/// <summary>
/// Tests for many-to-many soft delete safety via the <see cref="SoftDeletionInterceptor"/>.
/// </summary>
public sealed class SoftDeleteManyToManyTests
{
    [Fact]
    public async Task DeleteAsync_ParentWithManyToMany_DoesNotCascadeToTarget()
    {
        // Arrange
        using TestDbContext context = TestDbContext.Create();
        TestRole adminRole = new() { Id = Guid.NewGuid(), Name = "Admin" };
        TestUser user1 = new() { Id = Guid.NewGuid(), Name = "Alice" };

        user1.Roles.Add(adminRole);
        await context.Roles.AddAsync(adminRole);
        await context.Users.AddAsync(user1);
        _ = await context.SaveChangesAsync();

        // Act
        _ = context.Users.Remove(user1);
        _ = await context.SaveChangesAsync();

        // Assert
        TestUser? deletedUser = await context.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == user1.Id);
        TestRole? activeRole = await context.Roles.FirstAsync(r => r.Id == adminRole.Id);

        Assert.True(deletedUser.IsDeleted);
        Assert.False(activeRole.IsDeleted); // Target entity MUST remain untouched
    }
}
