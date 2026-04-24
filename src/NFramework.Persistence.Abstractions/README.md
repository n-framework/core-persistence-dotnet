# NFramework.Persistence.Abstractions

Zero-dependency persistence abstractions for Clean Architecture applications in .NET.

## Overview

This package defines the core contracts and base classes used by the NFramework persistence layer. It is designed to be completely independent of any specific database technology, making it ideal for use in Domain and Application layers.

## Key Features

- **Entity Bases**: `Entity<TId>`, `AuditableEntity<TId>`, and `SoftDeletableEntity<TId>`.
- **Repository Patterns**: `IAsyncRepository` and `IPaginate<T>` for consistent data access.
- **Dynamic Queries**: Type-safe `Filter` and `Order` models for complex search scenarios.
- **Native AOT Ready**: Zero reflection at heart, fully compatible with trimmable and AOT-compiled applications.

## Usage

Inherit from the base entity classes to define your domain models:

```csharp
public class Product : AuditableEntity<Guid>
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}
```

Define your repository interfaces in the Application layer:

```csharp
public interface IProductRepository : IAsyncRepository<Product, Guid>
{
}
```
