using NFramework.Persistence.EFCore.Repositories;

namespace NFramework.Persistence.EFCore.Tests.Helpers;

/// <summary>
/// Concrete test repository for <see cref="TestProduct"/>.
/// </summary>
internal sealed class TestProductRepository(TestDbContext context)
    : EFCoreRepository<TestProduct, Guid, TestDbContext>(context);

/// <summary>
/// Concrete test repository for <see cref="TestCategory"/>.
/// </summary>
internal sealed class TestCategoryRepository(TestDbContext context)
    : EFCoreRepository<TestCategory, int, TestDbContext>(context);
