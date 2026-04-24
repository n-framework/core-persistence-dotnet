using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.Abstractions.Repositories;
using Shouldly;

namespace NFramework.Persistence.Abstractions.Tests.Repositories;

public class RepositoryContractTests
{
    [Fact]
    public void IReadRepository_ShouldDefineStandardMethods()
    {
        var type = typeof(IReadRepository<TestEntity, Guid>);
        _ = new TestEntity(); // Instantiated to resolve CA1812

        type.GetMethod("GetByIdAsync").ShouldNotBeNull();
        type.GetMethod("GetAllAsync").ShouldNotBeNull();
        type.GetMethod("GetListAsync").ShouldNotBeNull();
        type.GetMethod("AnyAsync").ShouldNotBeNull();
        type.GetMethod("CountAsync").ShouldNotBeNull();
    }

    [Fact]
    public void IWriteRepository_ShouldDefineStandardMethods()
    {
        var type = typeof(IWriteRepository<TestEntity, Guid>);

        type.GetMethod("AddAsync").ShouldNotBeNull();
        type.GetMethod("UpdateAsync").ShouldNotBeNull();
        type.GetMethod("UpsertAsync").ShouldNotBeNull();
        type.GetMethod("DeleteAsync").ShouldNotBeNull();
        type.GetMethod("BulkAddAsync").ShouldNotBeNull();
        type.GetMethod("BulkUpdateAsync").ShouldNotBeNull();
        type.GetMethod("BulkDeleteAsync").ShouldNotBeNull();
    }

    [Fact]
    public void IDynamicReadRepository_ShouldDefineStandardMethods()
    {
        var type = typeof(IDynamicReadRepository<TestEntity, Guid>);

        type.GetMethod("GetByDynamicAsync").ShouldNotBeNull();
        type.GetMethod("GetAllByDynamicAsync").ShouldNotBeNull();
        type.GetMethod("GetListByDynamicAsync").ShouldNotBeNull();
    }

    private sealed class TestEntity : Entity<Guid>;
}
