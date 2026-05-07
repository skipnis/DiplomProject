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
    : IQueryHandler<GetCatalogItemsQuery, PagedResponse<CatalogItemDto>>
{
    public async Task<Result<PagedResponse<CatalogItemDto>>> HandleAsync(
        GetCatalogItemsQuery query,
        CancellationToken ct = default)
    {
        var hasOccasionFilter = query.Filter.OccasionIds is { Count: > 0 };

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
                i.Id, i.Name, i.Description, i.Price, i.Currency,
                i.ImagePath, i.Url, i.CategoryId, CategoryName = i.Category.Name,
                i.IsPublished, i.CreatedAt, i.UpdatedAt, i.WishCount,
            });

        var totalCount = await itemQuery.CountAsync(ct);
        var rawItems = await itemQuery
            .Skip((query.Request.Page - 1) * query.Request.PageSize)
            .Take(query.Request.PageSize)
            .ToListAsync(ct);

        if (rawItems.Count == 0)
            return new PagedResponse<CatalogItemDto>([], query.Request.Page, query.Request.PageSize, totalCount);

        var itemIds = rawItems.Select(i => i.Id).ToList();

        var badgesByItemId = await gamification.GetBadgesForItemsAsync(itemIds, query.UserId, ct);

        var occasionsByItemId = await db.CatalogItemOccasions
            .AsNoTracking()
            .Where(o => itemIds.Contains(o.CatalogItemId))
            .Select(o => new { o.CatalogItemId, o.Occasion.Id, o.Occasion.Key, o.Occasion.Label, o.Occasion.Order })
            .ToListAsync(ct);

        var occasionsLookup = occasionsByItemId
            .GroupBy(o => o.CatalogItemId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<OccasionDto>)group
                    .Select(o => new OccasionDto(o.Id, o.Key, o.Label, o.Order))
                    .ToList());

        var items = rawItems
            .Select(i => new CatalogItemDto(
                i.Id, i.Name, i.Description,
                i.Price, i.Currency?.ToString(),
                i.ImagePath, i.Url,
                i.CategoryId, i.CategoryName,
                i.IsPublished, i.CreatedAt, i.UpdatedAt,
                i.WishCount, null,
                badgesByItemId.GetValueOrDefault(i.Id, []),
                occasionsLookup.GetValueOrDefault(i.Id, [])))
            .ToList();

        return new PagedResponse<CatalogItemDto>(items, query.Request.Page, query.Request.PageSize, totalCount);
    }
}
