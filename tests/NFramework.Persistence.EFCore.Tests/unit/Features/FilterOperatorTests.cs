using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Dynamic;
using NFramework.Persistence.EFCore.Extensions;
using NFramework.Persistence.EFCore.Tests.Unit.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Unit.Features;

public class FilterOperatorTests
{
    [Theory]
    [InlineData(FilterOperator.Equal, "Order-1", 1)]
    [InlineData(FilterOperator.NotEqual, "Order-1", 2)]
    [InlineData(FilterOperator.Contains, "Order", 3)]
    [InlineData(FilterOperator.StartsWith, "Order-1", 1)]
    [InlineData(FilterOperator.EndsWith, "3", 1)]
    public async Task ApplyFilters_StringOperators_WorkCorrectly(
        FilterOperator @operator,
        string value,
        int expectedCount
    )
    {
        // Arrange
        using var context = TestDbContext.Create();
        context.Orders.AddRange(
            new TestOrder { Id = Guid.NewGuid(), OrderNumber = "Order-1" },
            new TestOrder { Id = Guid.NewGuid(), OrderNumber = "Order-2" },
            new TestOrder { Id = Guid.NewGuid(), OrderNumber = "Order-3" }
        );
        await context.SaveChangesAsync();

        var filters = new[]
        {
            new Filter
            {
                Field = "OrderNumber",
                Operator = @operator,
                Value = value,
            },
        };

        // Act
        var result = await context.Orders.ApplyFilters(filters).ToListAsync();

        // Assert
        result.Count.ShouldBe(expectedCount);
    }

    [Theory]
    [InlineData(FilterOperator.GreaterThan, 50, 1)]
    [InlineData(FilterOperator.GreaterThanOrEqual, 50, 2)]
    [InlineData(FilterOperator.LessThan, 50, 1)]
    [InlineData(FilterOperator.LessThanOrEqual, 50, 2)]
    public async Task ApplyFilters_NumericOperators_WorkCorrectly(
        FilterOperator @operator,
        decimal value,
        int expectedCount
    )
    {
        // Arrange
        using var context = TestDbContext.Create();
        context.Products.AddRange(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "P1",
                Price = 10,
            },
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "P2",
                Price = 50,
            },
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "P3",
                Price = 100,
            }
        );
        await context.SaveChangesAsync();

        var filters = new[]
        {
            new Filter
            {
                Field = "Price",
                Operator = @operator,
                Value = value,
            },
        };

        // Act
        var result = await context.Products.ApplyFilters(filters).ToListAsync();

        // Assert
        result.Count.ShouldBe(expectedCount);
    }

    [Fact]
    public async Task ApplyFilters_MultipleFilters_UseAndSemantics()
    {
        // Arrange
        using var context = TestDbContext.Create();
        context.Products.AddRange(
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "A",
                Price = 10,
            },
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "A",
                Price = 50,
            },
            new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = "B",
                Price = 10,
            }
        );
        await context.SaveChangesAsync();

        var filters = new[]
        {
            new Filter
            {
                Field = "Name",
                Operator = FilterOperator.Equal,
                Value = "A",
            },
            new Filter
            {
                Field = "Price",
                Operator = FilterOperator.Equal,
                Value = 10m,
            },
        };

        // Act
        var result = await context.Products.ApplyFilters(filters).ToListAsync();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("A");
        result[0].Price.ShouldBe(10);
    }
}
