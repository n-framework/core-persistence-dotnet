using NFramework.Persistence.Abstractions.Dynamic;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Dynamic;

public class FilterTests
{
    [Fact]
    public void Filter_DefaultValues_ShouldBeEmpty()
    {
        var filter = new Filter();
        filter.Field.ShouldBeEmpty();
        filter.Operator.ShouldBe(FilterOperator.Equal);
        filter.Value.ShouldBeNull();
        filter.IsNot.ShouldBeFalse();
        filter.Logic.ShouldBeNull();
        filter.Filters.ShouldBeNull();
        filter.CaseSensitive.ShouldBeFalse();
    }

    [Theory]
    [InlineData(FilterOperator.Equal, "Equal")]
    [InlineData(FilterOperator.NotEqual, "NotEqual")]
    [InlineData(FilterOperator.GreaterThan, "GreaterThan")]
    [InlineData(FilterOperator.Contains, "Contains")]
    [InlineData(FilterOperator.In, "In")]
    public void Filter_WithVariousOperators_ShouldStoreCorrectOperator(FilterOperator op, string _)
    {
        var filter = new Filter { Field = "Name", Operator = op, Value = "test" };
        filter.Operator.ShouldBe(op);
    }

    [Fact]
    public void Filter_NestedFilters_ShouldBeSettable()
    {
        var filter = new Filter
        {
            Logic = FilterLogic.And,
            Filters = new List<Filter>
            {
                new()
                {
                    Field = "Name",
                    Operator = FilterOperator.Contains,
                    Value = "test",
                },
                new()
                {
                    Field = "Age",
                    Operator = FilterOperator.GreaterThan,
                    Value = "18",
                },
            },
        };

        filter.Filters.Count.ShouldBe(2);
        filter.Logic.ShouldBe(FilterLogic.And);
    }
}
