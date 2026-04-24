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
}
