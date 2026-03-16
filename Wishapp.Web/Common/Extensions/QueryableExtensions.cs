using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Common.Extensions;

public static class QueryableExtensions
{
    extension<T>(IQueryable<T> query)
    {
        public IQueryable<T> WhereIf(bool condition,
            Expression<Func<T, bool>> predicate) =>
            condition ? query.Where(predicate) : query;

        private IQueryable<T> ApplyPaging(PagedRequest request) =>
            query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);
        
        public async Task<PagedResponse<T>> ToPagedResponseAsync(
            PagedRequest request,
            CancellationToken ct = default)
        {
            var totalCount = await query.CountAsync(ct);

            var items = await query
                .ApplyPaging(request)
                .ToListAsync(ct);

            return new PagedResponse<T>(
                items,
                request.Page,
                request.PageSize,
                totalCount);
        }
    }
}