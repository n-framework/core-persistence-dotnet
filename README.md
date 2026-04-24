# NFramework.Persistence

NFramework.Persistence is the core data access library of NFramework, a compile-time-first application framework and workspace standard for building clean architecture services.

## Overview

This package provides a robust persistence layer built on top of Entity Framework Core, designed for:

- **Clean Architecture**: Strong separation between abstractions and implementation.
- **Native AOT**: Zero reflection, compile-time service registration.
- **Microservice Ready**: Efficient pagination, dynamic querying, and bulk operations.
- **High Stability**: Strong validation, concurrency control, and explicit error handling.

## Key Features

- **Repository Abstractions**: Clean interfaces for CRUD, paging, and dynamic querying.
- **Soft Delete**: Built-in support for safe data removal with automatic global filters.
- **Concurrency Control**: Optimistic concurrency using row versions and timestamps.
- **Dynamic Queries**: Type-safe runtime query building with filters and sorting.
- **Bulk Operations**: High-performance batch processing with automatic chunking.
- **Pagination**: Flexible pagination results with comprehensive metadata.
