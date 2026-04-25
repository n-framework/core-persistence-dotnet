using NFramework.Persistence.EFCore.Repositories;

namespace NFramework.Persistence.EFCore.Tests.Helpers;

/// <summary>
/// SQLite-backed repository for concurrency testing.
/// </summary>
internal sealed class SqliteTestProductRepository(SqliteTestDbContext context)
    : EFCoreRepository<TestProduct, Guid, SqliteTestDbContext>(context);
