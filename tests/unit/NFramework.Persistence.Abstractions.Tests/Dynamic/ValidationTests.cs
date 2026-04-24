using NFramework.Persistence.Abstractions.Dynamic;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.Abstractions.Pagination;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Dynamic;

public class ValidationTests
{
    [Fact]
    public void Paging_SizeZero_ShouldThrow()
    {
        Should
            .Throw<ArgumentException>(() => new Paging(0, 0))
            .Message.ShouldContain("Page size must be greater than 0");
    }

    [Fact]
    public void Filter_EmptyField_ShouldThrow()
    {
        var filter = new Filter();
        Should.Throw<ArgumentException>(() => filter.Field = "").Message.ShouldContain("Field name cannot be empty");
    }

    [Fact]
    public void Filter_LogicWithoutFilters_ShouldThrow()
    {
        var filter = new Filter
        {
            Field = "Name",
            Operator = FilterOperator.Equal,
            Logic = FilterLogic.And,
        };
        filter.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(filter)).Any().ShouldBeTrue();
    }

    [Fact]
    public void Order_EmptyField_ShouldThrow()
    {
        var order = new Order();
        Should.Throw<ArgumentException>(() => order.Field = " ").Message.ShouldContain("Field name cannot be empty");
    }

    [Fact]
    public void AuditableEntity_UpdatedAtEarlierThanCreatedAt_ShouldFailValidation()
    {
        var entity = new TestAuditableEntity { CreatedAt = DateTime.UtcNow };
        entity.UpdatedAt = entity.CreatedAt.AddSeconds(-1);

        var results = entity.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(entity)).ToList();
        results.ShouldContain(r =>
            r.ErrorMessage != null && r.ErrorMessage.Contains("UpdatedAt cannot be earlier than CreatedAt")
        );
    }

    [Fact]
    public void SoftDeletableEntity_IsDeletedTrue_ShouldSetDeletedAt()
    {
        var entity = new TestSoftDeletableEntity { IsDeleted = true };
        entity.DeletedAt.ShouldNotBeNull();
    }

    [Fact]
    public void SoftDeletableEntity_DeletedAtNotNull_ShouldSetIsDeleted()
    {
        var entity = new TestSoftDeletableEntity { DeletedAt = DateTime.UtcNow };
        entity.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void SoftDeletableEntity_DeleteAndRestore_ShouldNotStackOverflow()
    {
        var entity = new TestSoftDeletableEntity { IsDeleted = true };

        // Should start deleted
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAt.ShouldNotBeNull();

        // Restore
        entity.IsDeleted = false;
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAt.ShouldBeNull();

        // Direct DeletedAt manipulation
        entity.DeletedAt = DateTime.UtcNow;
        entity.IsDeleted.ShouldBeTrue();

        entity.DeletedAt = null;
        entity.IsDeleted.ShouldBeFalse();
    }

    private sealed class TestAuditableEntity : AuditableEntity<Guid>;

    private sealed class TestSoftDeletableEntity : SoftDeletableEntity<Guid>;
}
