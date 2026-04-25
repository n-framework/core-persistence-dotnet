using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Entities;

public partial class SoftDeletableEntityTests
{
    [Fact]
    public void SoftDeletableEntity_IsDeletedTrue_ShouldSetDeletedAt()
    {
        var entity = new TestSoftDeletableEntity { IsDeleted = true };
        entity.DeletedAt.ShouldNotBeNull();
    }

    [Fact]
    public void SoftDeletableEntity_DeletedAtNotNull_ShouldSetIsDeleted()
    {
        var entity = new TestSoftDeletableEntity { DeletedAt = DateTime.UtcNow };
        entity.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void SoftDeletableEntity_DeleteAndRestore_ShouldNotStackOverflow()
    {
        var entity = new TestSoftDeletableEntity { IsDeleted = true };

        // Should start deleted
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAt.ShouldNotBeNull();

        // Restore
        entity.IsDeleted = false;
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAt.ShouldBeNull();

        // Direct DeletedAt manipulation
        entity.DeletedAt = DateTime.UtcNow;
        entity.IsDeleted.ShouldBeTrue();

        entity.DeletedAt = null;
        entity.IsDeleted.ShouldBeFalse();
    }
}
