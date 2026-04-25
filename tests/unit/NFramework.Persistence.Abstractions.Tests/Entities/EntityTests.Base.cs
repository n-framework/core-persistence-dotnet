using NFramework.Persistence.Abstractions.Entities;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Entities;

public class EntityTests
{
    private sealed class TestEntity(Guid id) : Entity<Guid>(id)
    {
        public TestEntity()
            : this(Guid.Empty) { }
    }

    [Fact]
    public void Entity_DefaultId_ShouldThrowArgumentException()
    {
        Should.Throw<ArgumentException>(() => new TestEntity(Guid.Empty));
    }

    [Fact]
    public void Entity_RowVersion_ShouldBeEmpty()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.RowVersion.ShouldBeEmpty();
    }

    [Fact]
    public void Entity_CanSetId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        entity.Id.ShouldBe(id);
    }

    [Fact]
    public void Entity_ValueTypeTId_IntIds()
    {
        var entity = new IntEntity(42);
        entity.Id.ShouldBe(42);
    }

    private sealed class IntEntity(int id) : Entity<int>(id);
}
