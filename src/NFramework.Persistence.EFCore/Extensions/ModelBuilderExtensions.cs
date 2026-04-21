using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Entities;

namespace NFramework.Persistence.EFCore.Extensions;

/// <summary>
/// Provides <see cref="ModelBuilder"/> extensions for applying
/// NFramework entity conventions to the EF Core model.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Sonar Analyzer",
    "S2325:Methods and properties that don't access instance data should be 'static'",
    Justification = "C# 14 extension block false positive"
)]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Sonar Analyzer",
    "S3398:Move this method inside ''.",
    Justification = "Private static helpers cannot be located inside extension blocks"
)]
public static class ModelBuilderExtensions
{
    extension(ModelBuilder modelBuilder)
    {
        /// <summary>
        /// Applies global query filters for all <see cref="SoftDeletableEntity{TId}"/> types
        /// to exclude soft-deleted entities from normal queries.
        /// </summary>
        public ModelBuilder ApplySoftDeleteFilters()
        {
            foreach (
                Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder
                    .Model.GetEntityTypes()
                    .Where(et => IsSoftDeletableEntity(et.ClrType))
            )
            {
                ApplyFilterForType(modelBuilder, entityType.ClrType);
            }

            return modelBuilder;
        }

        /// <summary>
        /// Configures the <see cref="Entity{TId}.RowVersion"/> property
        /// as a concurrency token for all entity types.
        /// </summary>
        public ModelBuilder ApplyConcurrencyTokens()
        {
            foreach (
                Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder
                    .Model.GetEntityTypes()
                    .Where(et => IsEntityWithRowVersion(et.ClrType))
            )
            {
                _ = modelBuilder.Entity(entityType.ClrType).Property("RowVersion").IsRowVersion();
            }

            return modelBuilder;
        }
    }

    private static void ApplyFilterForType(ModelBuilder modelBuilder, Type entityClrType)
    {
        ParameterExpression parameter = Expression.Parameter(entityClrType, "e");
        System.Reflection.PropertyInfo? isDeletedProperty = entityClrType.GetProperty(
            nameof(SoftDeletableEntity<>.IsDeleted)
        );

        if (isDeletedProperty == null)
        {
            return;
        }

        MemberExpression propertyAccess = Expression.Property(parameter, isDeletedProperty);
        UnaryExpression notDeleted = Expression.Not(propertyAccess);
        LambdaExpression lambda = Expression.Lambda(notDeleted, parameter);

        _ = modelBuilder.Entity(entityClrType).HasQueryFilter(lambda);
    }

    private static bool IsSoftDeletableEntity(Type type)
    {
        Type? current = type.BaseType;
        while (current != null)
        {
            if (
                current.IsGenericType
                && current.GetGenericTypeDefinition().FullName
                    == "NFramework.Persistence.Abstractions.Entities.SoftDeletableEntity`1"
            )
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static bool IsEntityWithRowVersion(Type type)
    {
        Type? current = type.BaseType;
        while (current != null)
        {
            if (
                current.IsGenericType
                && current.GetGenericTypeDefinition().FullName
                    == "NFramework.Persistence.Abstractions.Entities.Entity`1"
            )
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }
}
