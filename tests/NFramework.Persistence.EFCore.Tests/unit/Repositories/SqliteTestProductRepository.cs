using NFramework.Persistence.EFCore.Repositories;
using NFramework.Persistence.EFCore.Tests.Unit.Helpers;

namespace NFramework.Persistence.EFCore.Tests.Unit.Repositories;

/// <summary>
/// SQLite-backed repository for concurrency testing.
/// </summary>
internal sealed class SqliteTestProductRepository(SqliteTestDbContext context)
    : EFCoreRepository<TestProduct, Guid, SqliteTestDbContext>(context);
