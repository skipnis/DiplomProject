using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Catalog.Features.GetCatalogItems;

public sealed class GetCatalogItemsHandler(ApplicationDbContext db)
    : IQueryHandler<GetCatalogItemsQuery, PagedResponse<CatalogItemDto>>
{
    public async Task<Result<PagedResponse<CatalogItemDto>>> HandleAsync(
        GetCatalogItemsQuery query,
        CancellationToken ct = default)
    {
        var result = await db.CatalogItems
            .AsNoTracking()
            .Include(i => i.Category)
            .Where(i => i.IsPublished)
            .WhereIf(query.Filter.CategoryId.HasValue, i => i.CategoryId == query.Filter.CategoryId!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(query.Filter.Search),
                i => EF.Property<NpgsqlTsVector>(i, "SearchVector")
                    .Matches(EF.Functions.WebSearchToTsQuery("russian", query.Filter.Search!)))
            .WhereIf(query.Filter.MinPrice.HasValue, i => i.Price >= query.Filter.MinPrice!.Value)
            .WhereIf(query.Filter.MaxPrice.HasValue, i => i.Price <= query.Filter.MaxPrice!.Value)
            .OrderBy(i => i.Name)
            .Select(i => new CatalogItemDto(
                i.Id, i.Name, i.Description,
                i.Price, i.Currency != null ? i.Currency.ToString() : null,
                i.ImagePath, i.Url,
                i.CategoryId, i.Category.Name,
                i.IsPublished, i.CreatedAt, i.UpdatedAt,
                i.Ratings.Any() ? i.Ratings.Average(r => (double)r.Value) : (double?)null,
                i.Ratings.Count,
                query.UserId.HasValue ? i.Ratings.Where(r => r.UserId == query.UserId.Value).Select(r => (int?)r.Value).FirstOrDefault() : null,
                i.WishCount,
                null))
            .ToPagedResponseAsync(query.Request, ct);

        return result;
    }
}
