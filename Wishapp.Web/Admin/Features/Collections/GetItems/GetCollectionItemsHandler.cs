using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Collections.GetItems;

public sealed class GetCollectionItemsHandler(ApplicationDbContext db)
    : IQueryHandler<GetCollectionItemsQuery, List<CatalogItemDto>>
{
    public async Task<Result<List<CatalogItemDto>>> HandleAsync(
        GetCollectionItemsQuery query,
        CancellationToken ct = default)
    {
        var rawItems = await db.CatalogCollectionItems
            .AsNoTracking()
            .Where(i => i.CollectionId == query.CollectionId)
            .Select(i => new
            {
                i.CatalogItem.Id, i.CatalogItem.Name, i.CatalogItem.Description,
                i.CatalogItem.Price, i.CatalogItem.Currency,
                i.CatalogItem.ImagePath, i.CatalogItem.Url,
                i.CatalogItem.CategoryId, CategoryName = i.CatalogItem.Category.Name,
                i.CatalogItem.IsPublished, i.CatalogItem.CreatedAt, i.CatalogItem.UpdatedAt,
                i.CatalogItem.WishCount,
                CollectionItemDescription = i.Description
            })
            .ToListAsync(ct);

        return rawItems
            .Select(i => new CatalogItemDto(
                i.Id, i.Name, i.Description,
                i.Price, i.Currency?.ToString(),
                i.ImagePath, i.Url,
                i.CategoryId, i.CategoryName,
                i.IsPublished, i.CreatedAt, i.UpdatedAt,
                i.WishCount, i.CollectionItemDescription, [], []))
            .ToList();
    }
}
