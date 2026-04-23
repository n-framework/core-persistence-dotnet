using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.EFCore.Interceptors;

namespace NFramework.Persistence.EFCore.Tests.Unit.Helpers;

/// <summary>
/// SQLite-backed test DbContext for concurrency testing.
/// Unlike InMemory, SQLite enforces RowVersion concurrency tokens.
/// </summary>
internal sealed class SqliteTestDbContext : DbContext
{
    private readonly SqliteConnection _connection;

    private SqliteTestDbContext(DbContextOptions<SqliteTestDbContext> options, SqliteConnection connection)
        : base(options)
    {
        _connection = connection;
    }

    /// <summary>Products table.</summary>
    public DbSet<TestProduct> Products => Set<TestProduct>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<TestProduct>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity
                .Property(e => e.RowVersion)
                .IsRequired()
                .IsConcurrencyToken()
                .HasDefaultValue(Array.Empty<byte>());
            _ = entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    /// <summary>
    /// Creates a new SQLite-backed test context with an open in-memory connection.
    /// The connection remains open for the lifetime of the context to keep the database alive.
    /// </summary>
    public static async Task<SqliteTestDbContext> CreateAsync()
    {
        SqliteConnection connection = new("DataSource=:memory:");
        await connection.OpenAsync();

        DbContextOptions<SqliteTestDbContext> options = new DbContextOptionsBuilder<SqliteTestDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new SoftDeletionInterceptor(), new AuditableInterceptor())
            .Options;

        SqliteTestDbContext context = new(options, connection);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _connection.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }
}
