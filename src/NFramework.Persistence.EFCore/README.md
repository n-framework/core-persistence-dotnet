# NFramework.Persistence.EFCore

1. High-Performance Repository Layer
1. Native AOT & Trimmability Validation
1. Dynamic Filtering & Abstraction

## Overview

This package provides the concrete implementation of `IAsyncRepository` using EF Core. It includes advanced features like automatic timestamps, soft deletion via interceptors, and dynamic query translation.

## Key Features

- **Explicit Registration**: Designed for full transparency and Native AOT compatibility.
- **Interceptors**:
  - `TimestampsInterceptor`: Automatic `CreatedAt`/`UpdatedAt` management.
  - `SoftDeletionInterceptor`: Recursive cascade soft-delete support.
- **Dynamic Linq**: Real-time translation of `Filter` models to optimized SQL.
- **Bulk Operations**: High-performance batching for Add, Update, and Delete actions.

## Quick Start

1. Register your repository in the Infrastructure layer:

```csharp
public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<AppDbContext>(options => 
        options.UseSqlServer(configuration.GetConnectionString("Default")));

    services.AddScoped<IProductRepository, ProductRepository>();
    return services;
}
```

1. Leverage the `EFCoreRepository` base:

```csharp
public class ProductRepository : EFCoreRepository<Product, Guid, AppDbContext>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }
}
```
