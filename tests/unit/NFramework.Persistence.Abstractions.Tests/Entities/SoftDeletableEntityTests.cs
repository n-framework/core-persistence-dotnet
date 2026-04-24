using NFramework.Persistence.Abstractions.Entities;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Entities;

public class SoftDeletableEntityTests
{
    private sealed class TestSoftDeletableEntity : SoftDeletableEntity<int>;

    [Fact]
    public void SoftDeletableEntity_ShouldInheritFromAuditable()
    {
        var entity = new TestSoftDeletableEntity();
        entity.ShouldBeAssignableTo<AuditableEntity<int>>();
        entity.ShouldBeAssignableTo<Entity<int>>();
    }

    [Fact]
    public void SoftDeletableEntity_DefaultValues_ShouldNotBeDeleted()
    {
        var entity = new TestSoftDeletableEntity();
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAt.ShouldBeNull();
    }

    [Fact]
    public void SoftDeletableEntity_CanMarkAsDeleted()
    {
        var now = DateTime.UtcNow;
        var entity = new TestSoftDeletableEntity { IsDeleted = true, DeletedAt = now };
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAt.ShouldBe(now);
    }

    [Fact]
    public void SoftDeletableEntity_SettingIsDeletedTrue_ShouldSetDeletedAt()
    {
        var entity = new TestSoftDeletableEntity { IsDeleted = true };

        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAt.ShouldNotBeNull();
    }

    [Fact]
    public void SoftDeletableEntity_SettingIsDeletedFalse_ShouldClearDeletedAt()
    {
        var entity = new TestSoftDeletableEntity { IsDeleted = true, DeletedAt = DateTime.UtcNow };

        entity.IsDeleted = false;

        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAt.ShouldBeNull();
    }

    [Fact]
    public void SoftDeletableEntity_SettingDeletedAt_ShouldUpdateIsDeleted()
    {
        var entity = new TestSoftDeletableEntity { DeletedAt = DateTime.UtcNow };

        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAt.ShouldNotBeNull();

        entity.DeletedAt = null;

        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAt.ShouldBeNull();
    }

    [Fact]
    public void SoftDeletableEntity_ConcurrentUpdates_ShouldNotThrowExceptionsOrDeadlock()
    {
        var entity = new TestSoftDeletableEntity();
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        Parallel.For(
            0,
            1000,
            i =>
            {
                try
                {
                    if (i % 2 == 0)
                    {
                        entity.IsDeleted = true;
                        entity.IsDeleted = false;
                    }
                    else
                    {
                        entity.DeletedAt = DateTime.UtcNow;
                        entity.DeletedAt = null;
                    }
                }
#pragma warning disable CA1031
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    exceptions.Add(ex);
                }
            }
        );

        exceptions.ShouldBeEmpty();
    }
}
