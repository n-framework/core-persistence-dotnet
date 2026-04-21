using Microsoft.EntityFrameworkCore;
using NFramework.Persistence.Abstractions.Pagination;

namespace NFramework.Persistence.EFCore.Extensions;

/// <summary>
/// Provides <see cref="IQueryable{T}"/> pagination extensions
/// that map <see cref="Paging"/> descriptors to Skip/Take operations.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Sonar Analyzer",
    "S2325:Methods and properties that don't access instance data should be 'static'",
    Justification = "C# 14 extension block false positive"
)]
public static class PaginationExtensions
{
    extension<T>(IQueryable<T> source)
    {
        /// <summary>
        /// Applies pagination and returns a <see cref="PaginatedList{T}"/>.
        /// </summary>
        public async Task<PaginatedList<T>> ToPaginatedListAsync(
            Paging paging,
            CancellationToken cancellationToken = default
        )
        {
            int totalCount = await source.CountAsync(cancellationToken).ConfigureAwait(false);
            int totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / paging.Size);

            List<T> items = await source
                .Skip((int)(paging.Index * paging.Size))
                .Take((int)paging.Size)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            PagingMeta meta = new(paging, totalCount, totalPages);
            return new PaginatedList<T>(items, meta);
        }
    }
}
