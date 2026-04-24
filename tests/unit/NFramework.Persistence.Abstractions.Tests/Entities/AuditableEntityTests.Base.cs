using NFramework.Persistence.Abstractions.Entities;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Entities;

public partial class AuditableEntityTests
{
    private sealed class TestAuditableEntity : AuditableEntity<Guid>;

    [Fact]
    public void AuditableEntity_ShouldInheritFromEntity()
    {
        var entity = new TestAuditableEntity();
        entity.ShouldBeAssignableTo<Entity<Guid>>();
    }

    [Fact]
    public void AuditableEntity_DefaultTimestamps_ShouldBeDefault()
    {
        var entity = new TestAuditableEntity();
        entity.CreatedAt.ShouldBe(default);
        entity.UpdatedAt.ShouldBe(default);
    }
}
