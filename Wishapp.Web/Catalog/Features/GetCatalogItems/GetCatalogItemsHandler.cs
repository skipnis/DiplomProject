using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Catalog.Features.GetCatalogItems;

public sealed class GetCatalogItemsHandler(ApplicationDbContext db, IGamificationApi gamification)
    : IQueryHandler<GetCatalogItemsQuery, PagedResponse<CatalogItemSummaryDto>>
{
    public async Task<Result<PagedResponse<CatalogItemSummaryDto>>> HandleAsync(
        GetCatalogItemsQuery query,
        CancellationToken ct = default)
    {
        var hasOccasionFilter = query.Filter.OccasionIds is { Length: > 0 };

        var itemQuery = db.CatalogItems
            .AsNoTracking()
            .Include(i => i.Category)
            .Where(i => i.IsPublished)
            .WhereIf(query.Filter.CategoryId.HasValue, i => i.CategoryId == query.Filter.CategoryId!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(query.Filter.Search),
                i => EF.Property<NpgsqlTsVector>(i, "SearchVector")
                    .Matches(EF.Functions.WebSearchToTsQuery("russian", query.Filter.Search!)))
            .WhereIf(query.Filter.MinPrice.HasValue, i => i.Price >= query.Filter.MinPrice!.Value)
            .WhereIf(query.Filter.MaxPrice.HasValue, i => i.Price <= query.Filter.MaxPrice!.Value)
            .WhereIf(hasOccasionFilter,
                i => i.Occasions.Any(o => query.Filter.OccasionIds!.Contains(o.OccasionId)))
            .OrderByDescending(i => i.WishCount)
            .ThenBy(i => i.Name)
            .Select(i => new
            {
                i.Id, i.Name, i.Price, i.Currency,
                i.ImagePath, i.Url, i.CategoryId, CategoryName = i.Category.Name,
                i.WishCount,
            });

        var totalCount = await itemQuery.CountAsync(ct);
        var rawItems = await itemQuery
            .Skip((query.Request.Page - 1) * query.Request.PageSize)
            .Take(query.Request.PageSize)
            .ToListAsync(ct);

        if (rawItems.Count == 0)
            return new PagedResponse<CatalogItemSummaryDto>([], query.Request.Page, query.Request.PageSize, totalCount);

        var itemIds = rawItems.Select(i => i.Id).ToList();
        var badgesByItemId = await gamification.GetBadgesForItemsAsync(itemIds, query.UserId, ct);

        var items = rawItems
            .Select(i => new CatalogItemSummaryDto(
                i.Id, i.Name,
                i.Price, i.Currency?.ToString(),
                i.ImagePath, i.Url,
                i.CategoryId, i.CategoryName,
                i.WishCount, null,
                badgesByItemId.GetValueOrDefault(i.Id, [])))
            .ToList();

        return new PagedResponse<CatalogItemSummaryDto>(items, query.Request.Page, query.Request.PageSize, totalCount);
    }
}
