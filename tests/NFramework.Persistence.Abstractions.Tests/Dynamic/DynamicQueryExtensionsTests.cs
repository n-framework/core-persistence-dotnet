using NFramework.Persistence.Abstractions.Dynamic;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Dynamic;

public class DynamicQueryExtensionsTests
{
    private sealed class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Fact]
    public void Filter_For_ShouldSetFieldName()
    {
        _ = new TestEntity(); // Resolve CA1812
        var filter = new Filter().For<TestEntity>(e => e.Name);
        filter.Field.ShouldBe("Name");
    }

    [Fact]
    public void Filter_For_WithValueType_ShouldSetFieldName()
    {
        var filter = new Filter().For<TestEntity>(e => e.Age);
        filter.Field.ShouldBe("Age");
    }

    [Fact]
    public void Order_For_ShouldSetFieldName()
    {
        var order = new Order().For<TestEntity>(e => e.Name);
        order.Field.ShouldBe("Name");
    }
}
