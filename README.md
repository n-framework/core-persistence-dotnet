# NFramework.Persistence

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

The core data access library for NFramework .NET services. Built on Entity Framework Core, it provides repository abstractions, dynamic querying, and bulk operations designed for Native AOT through compile-time service registration instead of runtime reflection.

---

## Overview

- **Clean Architecture**: Strong separation between abstractions and implementation.
- **Native AOT Ready**: Zero reflection, compile-time service registration.
- **Microservice Ready**: Efficient pagination, dynamic querying, and bulk operations.
- **High Stability**: Strong validation, concurrency control, and explicit error handling.

---

## Key Features

- **Repository Abstractions**: Clean interfaces for CRUD, paging, and dynamic querying.
- **Soft Delete**: Built-in support for safe data removal with automatic global filters.
- **Concurrency Control**: Optimistic concurrency using row versions and timestamps.
- **Dynamic Queries**: Type-safe runtime query building with filters and sorting.
- **Bulk Operations**: High-performance batch processing with automatic chunking.
- **Pagination**: Flexible pagination results with comprehensive metadata.

---

## Projects

| Project | Description |
| --- | --- |
| `NFramework.Persistence.Abstractions` | Repository, paging, dynamic query, and entity contracts. Minimal dependencies. |
| `NFramework.Persistence.EFCore` | Entity Framework Core implementation of the persistence abstractions. |

---

## Build

```bash
make build
```

Or directly:

```bash
dotnet build NFramework.Persistence.slnx
```

## Test

```bash
make test
```

Or directly:

```bash
dotnet test NFramework.Persistence.slnx
```

## Format & Lint

```bash
make format
make lint
```

## Setup

```bash
make setup
```

---

## License

This project is licensed under the **Apache License 2.0** - see the [LICENSE](LICENSE) file for details.
