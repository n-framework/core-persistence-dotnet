using NFramework.Persistence.Abstractions.Entities;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Entities;

public class EntityTests
{
    private sealed class TestEntity : Entity<Guid>;

    [Fact]
    public void Entity_DefaultId_ShouldBeDefault()
    {
        var entity = new TestEntity();
        entity.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Entity_RowVersion_ShouldBeEmpty()
    {
        var entity = new TestEntity();
        entity.RowVersion.ShouldBeEmpty();
    }

    [Fact]
    public void Entity_CanSetId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity { Id = id };
        entity.Id.ShouldBe(id);
    }

    [Fact]
    public void Entity_ValueTypeTId_IntIds()
    {
        var entity = new IntEntity { Id = 42 };
        entity.Id.ShouldBe(42);
    }

    private sealed class IntEntity : Entity<int>;
}
