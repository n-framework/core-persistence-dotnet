using NFramework.Persistence.Abstractions.Entities;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Entities;

public partial class AuditableEntityTests
{
    private sealed class TestAuditableEntity(Guid id) : AuditableEntity<Guid>(id)
    {
        public TestAuditableEntity()
            : this(Guid.NewGuid()) { }
    }

    [Fact]
    public void AuditableEntity_ShouldInheritFromEntity()
    {
        var entity = new TestAuditableEntity(Guid.NewGuid());
        entity.ShouldBeAssignableTo<Entity<Guid>>();
    }

    [Fact]
    public void AuditableEntity_DefaultTimestamps_ShouldBeDefault()
    {
        var entity = new TestAuditableEntity(Guid.NewGuid());
        entity.CreatedAt.ShouldBe(default);
        entity.UpdatedAt.ShouldBe(default);
    }
}
