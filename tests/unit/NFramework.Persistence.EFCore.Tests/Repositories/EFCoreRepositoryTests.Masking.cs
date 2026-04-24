using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NFramework.Persistence.EFCore.Extensions;
using NFramework.Persistence.EFCore.Interceptors;
using Xunit;

namespace NFramework.Persistence.EFCore.Tests.Repositories;

/// <summary>
/// Tests for sensitive data masking integration in <see cref="AuditLoggerInterceptor"/>.
/// Verifies that properties configured via fluent API are masked in audit logs.
/// </summary>
public sealed class SensitiveDataMaskingTests
{
    [Fact]
    public async Task SaveChanges_SensitiveProperty_IsMaskedInLog()
    {
        using TestSensitiveDbContext context = await TestSensitiveDbContext.CreateAsync();
        SensitiveEntity entity = new()
        {
            Id = Guid.NewGuid(),
            Name = "Public Name",
            Secret = "TopSecret123",
        };

        await context.Entities.AddAsync(entity);
        _ = await context.SaveChangesAsync();

        string logOutput = context.CapturedLogOutput;
        Assert.Contains("Public Name", logOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("TopSecret123", logOutput, StringComparison.Ordinal);
        Assert.Contains("************", logOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveChanges_PartiallyMaskedProperty_KeepsVisibleChars()
    {
        using TestSensitiveDbContext context = await TestSensitiveDbContext.CreateAsync();
        SensitiveEntity entity = new()
        {
            Id = Guid.NewGuid(),
            Name = "User",
            PartialSecret = "john@example.com",
        };

        await context.Entities.AddAsync(entity);
        _ = await context.SaveChangesAsync();

        string logOutput = context.CapturedLogOutput;
        Assert.Contains("joh", logOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("john@example.com", logOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveChanges_ModifiedSensitiveProperty_MasksBothValues()
    {
        using TestSensitiveDbContext context = await TestSensitiveDbContext.CreateAsync();
        SensitiveEntity entity = new()
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Secret = "OldSecret",
        };

        await context.Entities.AddAsync(entity);
        _ = await context.SaveChangesAsync();

        entity.Secret = "NewSecret";
        _ = await context.SaveChangesAsync();

        string logOutput = context.CapturedLogOutput;
        Assert.DoesNotContain("OldSecret", logOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("NewSecret", logOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveChanges_NonSensitiveProperty_IsLoggedInPlaintext()
    {
        using TestSensitiveDbContext context = await TestSensitiveDbContext.CreateAsync();
        SensitiveEntity entity = new()
        {
            Id = Guid.NewGuid(),
            Name = "Visible Name",
            Secret = "hidden",
        };

        await context.Entities.AddAsync(entity);
        _ = await context.SaveChangesAsync();

        string logOutput = context.CapturedLogOutput;
        Assert.Contains("Visible Name", logOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveChangesFailed_LogsErrorWithMaskedData()
    {
        // Arrange
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        CapturingLoggerProvider provider = new();
        ILoggerFactory loggerFactory = LoggerFactory.Create(b =>
            b.AddProvider(provider).SetMinimumLevel(LogLevel.Information)
        );
        var options = new DbContextOptionsBuilder<TestSensitiveDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new AuditLoggerInterceptor())
            .UseLoggerFactory(loggerFactory)
            .Options;

        using TestSensitiveDbContext context = new(options, provider, loggerFactory);
        await context.Database.EnsureCreatedAsync();

        SensitiveEntity entity = new()
        {
            Id = Guid.NewGuid(),
            Name = "Error Trigger",
            Secret = "SensitiveFailure",
        };

        // Act
        await context.Entities.AddAsync(entity);

        // Break the connection to force failure
        await connection.CloseAsync();

        // Assert
        await Assert.ThrowsAnyAsync<Exception>(() => context.SaveChangesAsync());

        string logOutput = context.CapturedLogOutput;
        Assert.Contains("SaveChanges failed for context", logOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("SensitiveFailure", logOutput, StringComparison.Ordinal);
        Assert.Contains("************", logOutput, StringComparison.Ordinal);
    }
}

internal sealed class SensitiveEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string PartialSecret { get; set; } = string.Empty;
}

internal sealed class TestSensitiveDbContext : DbContext
{
    private readonly CapturingLoggerProvider _loggerProvider;
    private readonly ILoggerFactory _loggerFactory;

    public DbSet<SensitiveEntity> Entities => Set<SensitiveEntity>();

    public string CapturedLogOutput => _loggerProvider.GetOutput();

    internal TestSensitiveDbContext(
        DbContextOptions<TestSensitiveDbContext> options,
        CapturingLoggerProvider provider,
        ILoggerFactory loggerFactory
    )
        : base(options)
    {
        _loggerProvider = provider;
        _loggerFactory = loggerFactory;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.Entity<SensitiveEntity>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.Secret).IsSensitiveData();
            _ = entity.Property(e => e.PartialSecret).IsSensitiveData(keepStartChars: 3);
        });
    }

    public override void Dispose()
    {
        _loggerFactory.Dispose();
        base.Dispose();
    }

    public static async Task<TestSensitiveDbContext> CreateAsync()
    {
        CapturingLoggerProvider provider = new();
        ILoggerFactory loggerFactory = LoggerFactory.Create(b =>
            b.AddProvider(provider).SetMinimumLevel(LogLevel.Information)
        );
        DbContextOptions<TestSensitiveDbContext> options = new DbContextOptionsBuilder<TestSensitiveDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new AuditLoggerInterceptor())
            .UseLoggerFactory(loggerFactory)
            .Options;

        TestSensitiveDbContext context = new(options, provider, loggerFactory);
        await context.Database.EnsureCreatedAsync();
        return context;
    }
}

internal sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly System.Text.StringBuilder _sb = new();

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(_sb);

    public string GetOutput() => _sb.ToString();

    public void Dispose() { }

    private sealed class CapturingLogger(System.Text.StringBuilder sb) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            ArgumentNullException.ThrowIfNull(formatter);
            _ = sb.AppendLine(formatter(state, exception));
        }
    }
}
