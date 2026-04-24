using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Entities;

public partial class AuditableEntityTests
{
    [Fact]
    public void AuditableEntity_UpdatedAtEarlierThanCreatedAt_ShouldFailValidation()
    {
        var entity = new TestAuditableEntity { CreatedAt = DateTime.UtcNow };
        entity.UpdatedAt = entity.CreatedAt.AddSeconds(-1);

        var errors = entity.Validate().ToList();
        errors.ShouldContain(err => err.Contains("UpdatedAt cannot be earlier than CreatedAt"));
    }
}
