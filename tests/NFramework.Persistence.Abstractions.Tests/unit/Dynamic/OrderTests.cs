using NFramework.Persistence.Abstractions.Dynamic;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Unit.Dynamic;

public class OrderTests
{
    [Fact]
    public void Order_DefaultValues()
    {
        var order = new Order();
        order.Field.ShouldBeEmpty();
        order.Direction.ShouldBe(OrderDirection.Asc);
    }

    [Theory]
    [InlineData("Name", OrderDirection.Asc)]
    [InlineData("Age", OrderDirection.Desc)]
    [InlineData("CreatedAt", OrderDirection.Asc)]
    public void Order_ShouldInitializeWithCorrectValues(string field, OrderDirection direction)
    {
        var order = new Order { Field = field, Direction = direction };
        order.Field.ShouldBe(field);
        order.Direction.ShouldBe(direction);
    }
}
