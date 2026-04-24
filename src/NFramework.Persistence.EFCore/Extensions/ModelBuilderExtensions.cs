using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NFramework.Persistence.Abstractions.Entities;
using NFramework.Persistence.EFCore.Constants;

namespace NFramework.Persistence.EFCore.Extensions;

/// <summary>
/// Provides <see cref="EntityTypeBuilder{TEntity}"/> extensions for applying
/// NFramework entity conventions explicitly without reflection magic.
/// </summary>
public static class ModelBuilderExtensions
{
    extension<
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicConstructors
                | DynamicallyAccessedMemberTypes.NonPublicConstructors
                | DynamicallyAccessedMemberTypes.PublicFields
                | DynamicallyAccessedMemberTypes.NonPublicFields
                | DynamicallyAccessedMemberTypes.PublicProperties
                | DynamicallyAccessedMemberTypes.NonPublicProperties
                | DynamicallyAccessedMemberTypes.Interfaces
        )]
            TEntity,
        TId
    >(EntityTypeBuilder<TEntity> builder)
        where TEntity : Entity<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Configures the 'Id' as Key and 'RowVersion' as a concurrency token for a base <see cref="Entity{TId}"/>.
        /// </summary>
        public EntityTypeBuilder<TEntity> ConfigureEntity()
        {
            ArgumentNullException.ThrowIfNull(builder);

            _ = builder.HasKey(e => e.Id);
            _ = builder.Property(e => e.RowVersion).IsRowVersion();

            return builder;
        }
    }

    extension<
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicConstructors
                | DynamicallyAccessedMemberTypes.NonPublicConstructors
                | DynamicallyAccessedMemberTypes.PublicFields
                | DynamicallyAccessedMemberTypes.NonPublicFields
                | DynamicallyAccessedMemberTypes.PublicProperties
                | DynamicallyAccessedMemberTypes.NonPublicProperties
                | DynamicallyAccessedMemberTypes.Interfaces
        )]
            TEntity,
        TId
    >(EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Configures timestamps for an <see cref="AuditableEntity{TId}"/>.
        /// </summary>
        public EntityTypeBuilder<TEntity> ConfigureAuditable()
        {
            ArgumentNullException.ThrowIfNull(builder);

            _ = builder.Property(e => e.CreatedAt).IsRequired();
            _ = builder.Property(e => e.UpdatedAt);

            return builder;
        }
    }

    extension<
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicConstructors
                | DynamicallyAccessedMemberTypes.NonPublicConstructors
                | DynamicallyAccessedMemberTypes.PublicFields
                | DynamicallyAccessedMemberTypes.NonPublicFields
                | DynamicallyAccessedMemberTypes.PublicProperties
                | DynamicallyAccessedMemberTypes.NonPublicProperties
                | DynamicallyAccessedMemberTypes.Interfaces
        )]
            TEntity,
        TId
    >(EntityTypeBuilder<TEntity> builder)
        where TEntity : SoftDeletableEntity<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Configures a query filter and deletion tracking for a <see cref="SoftDeletableEntity{TId}"/>.
        /// </summary>
        public EntityTypeBuilder<TEntity> ConfigureSoftDeletable(string isDeletedColumnName = "IsDeleted")
        {
            ArgumentNullException.ThrowIfNull(builder);

            _ = builder.Property(e => e.IsDeleted).HasColumnName(isDeletedColumnName).IsRequired();
            _ = builder.Property(e => e.DeletedAt);

            _ = builder.HasIndex(e => e.IsDeleted).HasFilter($"{isDeletedColumnName} = 0");
            _ = builder.HasQueryFilter(QueryFilters.SoftDeletion, e => !e.IsDeleted);

            return builder;
        }
    }
}
