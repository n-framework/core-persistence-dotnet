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
        _ = new TestEntity(Guid.NewGuid()); // Instantiated to resolve CA1812
        var methods = type.GetMethods();

        methods.ShouldContain(static m => m.Name == "GetByIdAsync");
        methods.ShouldContain(static m => m.Name == "GetAsync");
        methods.ShouldContain(static m => m.Name == "GetSelectAsync");
        methods.ShouldContain(static m => m.Name == "GetAllAsync");
        methods.ShouldContain(static m => m.Name == "GetAllSelectAsync");
        methods.ShouldContain(static m => m.Name == "GetListAsync");
        methods.ShouldContain(static m => m.Name == "GetListSelectAsync");
        methods.ShouldContain(static m => m.Name == "AnyAsync");
        methods.ShouldContain(static m => m.Name == "CountAsync");
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

    private sealed class TestEntity(Guid id) : Entity<Guid>(id);
}
