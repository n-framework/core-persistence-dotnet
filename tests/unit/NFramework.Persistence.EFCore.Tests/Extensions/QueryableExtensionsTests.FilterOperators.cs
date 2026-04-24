using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Dynamic;
using NFramework.Persistence.EFCore.Extensions;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Extensions;

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
            new TestOrder(Guid.NewGuid()) { OrderNumber = "Order-1" },
            new TestOrder(Guid.NewGuid()) { OrderNumber = "Order-2" },
            new TestOrder(Guid.NewGuid()) { OrderNumber = "Order-3" }
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
            new TestProduct(Guid.NewGuid()) { Name = "P1", Price = 10 },
            new TestProduct(Guid.NewGuid()) { Name = "P2", Price = 50 },
            new TestProduct(Guid.NewGuid()) { Name = "P3", Price = 100 }
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
            new TestProduct(Guid.NewGuid()) { Name = "A", Price = 10 },
            new TestProduct(Guid.NewGuid()) { Name = "A", Price = 50 },
            new TestProduct(Guid.NewGuid()) { Name = "B", Price = 10 }
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
        result[0].Price.ShouldBe(10);
    }

    [Fact]
    public async Task ApplyFilters_IsNull_And_IsNotNull_WorkCorrectly()
    {
        // Arrange
        using var context = TestDbContext.Create();
        context.Products.AddRange(
            new TestProduct(Guid.NewGuid()) { Name = "WithDesc", Description = "Exists" },
            new TestProduct(Guid.NewGuid()) { Name = "NoDesc", Description = null }
        );
        await context.SaveChangesAsync();

        // Act & Assert (IsNull)
        var isNullResult = await context
            .Products.ApplyFilters([new Filter { Field = "Description", Operator = FilterOperator.IsNull }])
            .ToListAsync();
        isNullResult.Count.ShouldBe(1);
        isNullResult[0].Name.ShouldBe("NoDesc");

        // Act & Assert (IsNotNull)
        var isNotNullResult = await context
            .Products.ApplyFilters([new Filter { Field = "Description", Operator = FilterOperator.IsNotNull }])
            .ToListAsync();
        isNotNullResult.Count.ShouldBe(1);
        isNotNullResult[0].Name.ShouldBe("WithDesc");
    }

    [Fact]
    public async Task ApplyFilters_DoesNotContain_WorksCorrectly()
    {
        // Arrange
        using var context = TestDbContext.Create();
        context.Products.AddRange(
            new TestProduct(Guid.NewGuid()) { Name = "Apple" },
            new TestProduct(Guid.NewGuid()) { Name = "Banana" }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await context
            .Products.ApplyFilters([
                new Filter
                {
                    Field = "Name",
                    Operator = FilterOperator.DoesNotContain,
                    Value = "p",
                },
            ])
            .ToListAsync();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Banana");
    }
}
