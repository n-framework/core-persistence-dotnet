using System.ComponentModel.DataAnnotations;
using NFramework.Persistence.Abstractions.Dynamic;
using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Tests.Helpers;
using Shouldly;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Extensions;

public class DynamicQueryErrorTests
{
    [Fact]
    public async Task GetByDynamicAsync_WithInvalidFieldName_ShouldThrowArgumentException()
    {
        using var context = TestDbContext.Create();
        var repo = new TestProductRepository(context);

        // Name with invalid characters (SQL Injection attempt)
        var options = new DynamicQueryOption(
            [
                new Filter
                {
                    Field = "Name; DROP TABLE Products;--",
                    Operator = FilterOperator.Equal,
                    Value = "test",
                },
            ],
            null
        );

        var ex = await Should.ThrowAsync<ArgumentException>(async () => await repo.GetByDynamicAsync(options));

        ex.Message.ShouldContain("Invalid or unsafe field name");
    }

    [Fact]
    public void FilterValidation_InOperator_WithoutIEnumerable_ShouldFail()
    {
        var filter = new Filter
        {
            Field = "Name",
            Operator = FilterOperator.In,
            Value = 123,
        };

        var validationContext = new ValidationContext(filter);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(filter, validationContext, results, true);

        isValid.ShouldBeFalse();
        results.ShouldContain(r => r.ErrorMessage != null && r.ErrorMessage.Contains("requires an IEnumerable value"));
    }

    [Fact]
    public void FilterValidation_IsNullOperator_WithValue_ShouldFail()
    {
        var filter = new Filter
        {
            Field = "Name",
            Operator = FilterOperator.IsNull,
            Value = "test",
        };

        var validationContext = new ValidationContext(filter);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(filter, validationContext, results, true);

        isValid.ShouldBeFalse();
        results.ShouldContain(r => r.ErrorMessage != null && r.ErrorMessage.Contains("does not expect a value"));
    }

    [Fact]
    public void FilterValidation_EqualOperator_WithoutValue_ShouldFail()
    {
        var filter = new Filter
        {
            Field = "Name",
            Operator = FilterOperator.Equal,
            Value = null,
        };

        var validationContext = new ValidationContext(filter);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(filter, validationContext, results, true);

        isValid.ShouldBeFalse();
        results.ShouldContain(r => r.ErrorMessage != null && r.ErrorMessage.Contains("requires a comparison value"));
    }
}
