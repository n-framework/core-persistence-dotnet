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
        filter.Operator.ShouldBe(FilterOperator.Eq);
        filter.Value.ShouldBeNull();
        filter.IsNot.ShouldBeFalse();
        filter.Logic.ShouldBeNull();
        filter.Filters.ShouldBeNull();
        filter.CaseSensitive.ShouldBeFalse();
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
                    Operator = FilterOperator.Gt,
                    Value = "18",
                },
            },
        };

        filter.Filters.Count.ShouldBe(2);
        filter.Logic.ShouldBe(FilterLogic.And);
    }
}
