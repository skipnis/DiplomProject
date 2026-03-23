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
        var items = await db.CatalogCollectionItems
            .AsNoTracking()
            .Where(i => i.CollectionId == query.CollectionId)
            .Select(i => new CatalogItemDto(
                i.CatalogItem.Id,
                i.CatalogItem.Name,
                i.CatalogItem.Description,
                i.CatalogItem.Price,
                i.CatalogItem.Currency != null ? i.CatalogItem.Currency.ToString() : null,
                i.CatalogItem.ImagePath,
                i.CatalogItem.Url,
                i.CatalogItem.CategoryId,
                i.CatalogItem.Category.Name,
                i.CatalogItem.IsPublished,
                i.CatalogItem.CreatedAt,
                i.CatalogItem.UpdatedAt))
            .ToListAsync(ct);

        return items;
    }
}
