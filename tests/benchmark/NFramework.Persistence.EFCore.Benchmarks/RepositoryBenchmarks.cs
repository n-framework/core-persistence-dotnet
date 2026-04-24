using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Dynamic;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.Abstractions.Pagination;
using NFramework.Persistence.Abstractions.Repositories;
using NFramework.Persistence.EFCore.Repositories;

namespace NFramework.Persistence.EFCore.Benchmarks;

internal static class Program
{
    private static void Main()
    {
        BenchmarkRunner.Run<RepositoryBenchmarks>();
    }
}

internal sealed class Product : Entity<Guid>
{
    public string Name { get; set; } = default!;
}

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}

internal sealed class ProductRepository(AppDbContext context)
    : EFCoreRepository<Product, Guid, AppDbContext>(context) { }

[MemoryDiagnoser]
[SuppressMessage(
    "Design",
    "CA1052:Static holder type should be static or NotInheritable",
    Justification = "BenchmarkDotNet requires a non-static class."
)]
[SuppressMessage(
    "Maintainability",
    "CA1515:Consider making types internal",
    Justification = "BenchmarkDotNet requires public benchmark classes."
)]
public class RepositoryBenchmarks : IDisposable
{
    private AppDbContext _context = default!;
    private ProductRepository _repository = default!;
    private readonly Guid _id = Guid.NewGuid();

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "BenchmarkDb")
            .Options;

        _context = new AppDbContext(options);
        _repository = new ProductRepository(_context);

        // Seed 1000 items for pagination tests
        var products = Enumerable
            .Range(1, 1000)
            .Select(i => new Product { Id = i == 1 ? _id : Guid.NewGuid(), Name = $"Product {i}" })
            .ToList();

        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    [Benchmark]
    public async Task GetByIdAsync()
    {
        await _repository.GetByIdAsync(_id);
    }

    [Benchmark]
    public void DynamicQueryTranslation()
    {
        // ... translation logic ...
    }

    [Benchmark]
    public async Task AddAsync()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "New Product" };
        await _repository.AddAsync(product);
    }

    [Benchmark]
    public async Task GetPaginatedListAsync()
    {
        await _repository.GetListAsync(
            new PageableQueryOption<Product>
            {
                Page = new Paging { Index = 5, Size = 20 },
            }
        );
    }

    [Benchmark]
    public async Task GetDynamicListAsync()
    {
        await _repository.GetListByDynamicAsync(
            new PageableDynamicQueryOption
            {
                Page = new Paging { Index = 0, Size = 10 },
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Field = "Name",
                        Operator = FilterOperator.Contains,
                        Value = "Product",
                    },
                },
            }
        );
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
