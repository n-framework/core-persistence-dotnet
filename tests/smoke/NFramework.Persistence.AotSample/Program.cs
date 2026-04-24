using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NFramework.Persistence.Abstractions.Dynamic;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Extensions;
using NFramework.Persistence.EFCore.Repositories;

namespace NFramework.Persistence.AotSample;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var runner = new AotRunner();
        await runner.RunAsync(args);
    }
}

// Define a simple entity for AOT testing
internal sealed class AotProduct : Entity<Guid>
{
    [Obsolete("Only for ORM use", true)]
#pragma warning disable CS0618
    public AotProduct() { }
#pragma warning restore CS0618

    public AotProduct(Guid id)
        : base(id) { }

    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}

// Define the DbContext
[SuppressMessage(
    "Performance",
    "CA1812: Avoid uninstantiated internal classes",
    Justification = "Instantiated by DI via ActivatorUtilities."
)]
internal sealed class AotDbContext(DbContextOptions<AotDbContext> options) : DbContext(options)
{
    public DbSet<AotProduct> Products => Set<AotProduct>();
}

// Define the Repository (Explicit Implementation for AOT)
internal interface IAotProductRepository
    : IReadRepository<AotProduct, Guid>,
        IWriteRepository<AotProduct, Guid>,
        IDynamicReadRepository<AotProduct, Guid>,
        IUnitOfWork { }

[SuppressMessage("Performance", "CA1812: Avoid uninstantiated internal classes", Justification = "Instantiated by DI.")]
internal sealed class AotProductRepository(AotDbContext context)
    : EFCoreRepository<AotProduct, Guid, AotDbContext>(context),
        IAotProductRepository { }

internal sealed class AotRunner
{
    public async Task RunAsync(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Register DbContext (Explicitly)
        builder.Services.AddDbContext<AotDbContext>(options =>
        {
            options.UseSqlite("Data Source=aot_sample.db");
        });

        // Register Repository (Explicitly - NO MAGIC)
        builder.Services.AddScoped<IAotProductRepository, AotProductRepository>();

        using var host = builder.Build();

        // 1. Apply Migrations (Phase 12 Verification)
        await host.ApplyMigrationsAsync<AotDbContext>();

        using (var scope = host.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IAotProductRepository>();

            // 2. Basic CRUD
            var product = new AotProduct(Guid.NewGuid()) { Name = "AOT Product", Price = 99.99m };
            await repo.AddAsync(product);
            await repo.SaveChangesAsync();
            Console.WriteLine($"[AOT] Product created with ID: {product.Id}");

            // 3. Dynamic Query (Phase 7 Verification in AOT context)
            var options = new DynamicQueryOption(
                Filters:
                [
                    new Filter
                    {
                        Field = "Name",
                        Operator = FilterOperator.Contains,
                        Value = "AOT",
                    },
                ]
            );
            var results = await repo.GetAllByDynamicAsync(options);
            Console.WriteLine($"[AOT] Dynamic query found {results.Count} items.");

            if (results.Any())
            {
                Console.WriteLine($"[AOT] First item name: {results[0].Name}");
            }
        }

        Console.WriteLine("[AOT] Native AOT validation successful!");
    }
}
