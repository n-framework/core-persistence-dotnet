# Error Recovery Strategies

## Overview

This document outlines standard strategies for handling and recovering from persistence-related failures when using NFramework Persistence Abstractions. Consumers of these abstractions should implement these patterns to ensure system resilience.

## 1. Optimistic Concurrency Conflicts

- **Scenario**: A concurrency exception (e.g., `DbUpdateConcurrencyException`) is thrown during an `UpdateAsync` or `DeleteAsync` operation due to a `RowVersion` mismatch.
- **Recovery Strategy**:

    1. **Catch**: Intercept the concurrency exception at the service or command handler level.
    2. **Reload**: Fetch the latest version of the entity from the database using `GetByIdAsync`.
    3. **Resolve**:
        - **Client Wins**: Overwrite the database values with the current state (requires updating the `RowVersion` to the reloaded one).
        - **Database Wins**: Discard local changes and notify the user.
        - **Merge**: Intelligently merge non-conflicting fields.
    4. **Retry**: If "Client Wins" or "Merge" is chosen, re-attempt the operation.

## 2. Unique Constraint Violations

- **Scenario**: A database operation fails because of a duplicate key in a unique index.
- **Recovery Strategy**:

    1. **Prevent**: Perform a "pre-flight" check (e.g., `AnyAsync`) before attempting the write.
    2. **Handle**: Catch implementation-specific exceptions and map them to domain-level errors (e.g., `AlreadyExistsException`).
    3. **Feedback**: Provide clear feedback to the user about which field caused the violation.

## 3. Transient Failures & Deadlocks

- **Scenario**: Temporary network issues, database failovers, or deadlocks.
- **Recovery Strategy**:

    1. **Retry Policy**: Implementations should use an execution strategy (like Polly in EfCore) with exponential backoff.
    2. **Transaction Scope**: Ensure operations are idempotent if they might be retried after a partial failure.

## 4. Null References & Deleted Entities

- **Scenario**: Attempting to update an entity that no longer exists (e.g., concurrent hard delete).
- **Recovery Strategy**: Handle `GetByIdAsync` returning `null` gracefully by returning a `NotFound` result instead of allowing a `NullReferenceException` to propagate.
