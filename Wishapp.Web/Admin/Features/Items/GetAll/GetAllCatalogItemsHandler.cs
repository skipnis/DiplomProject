using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Items.GetAll;

public sealed class GetAllCatalogItemsHandler(ApplicationDbContext db)
    : IQueryHandler<GetAllCatalogItemsQuery, PagedResponse<CatalogItemDto>>
{
    public async Task<Result<PagedResponse<CatalogItemDto>>> HandleAsync(
        GetAllCatalogItemsQuery query,
        CancellationToken ct = default)
    {
        var itemQuery = db.CatalogItems
            .AsNoTracking()
            .Include(i => i.Category)
            .WhereIf(query.Filter.CategoryId.HasValue, i => i.CategoryId == query.Filter.CategoryId!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(query.Filter.Search),
                i => EF.Functions.ILike(i.Name, $"%{query.Filter.Search}%"))
            .WhereIf(query.Filter.MinPrice.HasValue, i => i.Price >= query.Filter.MinPrice!.Value)
            .WhereIf(query.Filter.MaxPrice.HasValue, i => i.Price <= query.Filter.MaxPrice!.Value)
            .OrderBy(i => i.Name)
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
                i.WishCount, null, [],
                occasionsLookup.GetValueOrDefault(i.Id, [])))
            .ToList();

        return new PagedResponse<CatalogItemDto>(items, query.Request.Page, query.Request.PageSize, totalCount);
    }
}
