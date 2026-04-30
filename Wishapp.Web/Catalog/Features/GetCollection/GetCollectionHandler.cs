using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Catalog.Features.GetCollection;

public sealed class GetCollectionHandler(ApplicationDbContext db)
    : IQueryHandler<GetCollectionQuery, CatalogCollectionDto>
{
    public async Task<Result<CatalogCollectionDto>> HandleAsync(
        GetCollectionQuery query,
        CancellationToken ct = default)
    {
        var raw = await db.CatalogCollections
            .AsNoTracking()
            .Where(c => c.Id == query.Id && c.IsPublished)
            .Select(c => new
            {
                c.Id, c.Name, c.Description, c.Occasion, c.CoverImagePath, c.Order,
                Items = c.Items
                    .Where(i => i.CatalogItem.IsPublished)
                    .Select(i => new
                    {
                        i.CatalogItem.Id, i.CatalogItem.Name, i.CatalogItem.Description,
                        i.CatalogItem.Price, i.CatalogItem.Currency,
                        i.CatalogItem.ImagePath, i.CatalogItem.Url,
                        i.CatalogItem.CategoryId, CategoryName = i.CatalogItem.Category.Name,
                        i.CatalogItem.IsPublished, i.CatalogItem.CreatedAt, i.CatalogItem.UpdatedAt,
                        i.CatalogItem.WishCount,
                        ItemDescription = i.Description
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (raw is null)
            return Error.NotFound("Catalog.CollectionNotFound", "Collection not found");

        var items = raw.Items
            .Select(i => new CatalogItemDto(
                i.Id, i.Name, i.Description,
                i.Price, i.Currency?.ToString(),
                i.ImagePath, i.Url,
                i.CategoryId, i.CategoryName,
                i.IsPublished, i.CreatedAt, i.UpdatedAt,
                i.WishCount, i.ItemDescription, []))
            .ToList();

        return new CatalogCollectionDto(
            raw.Id, raw.Name, raw.Description,
            raw.Occasion, raw.CoverImagePath, raw.Order, items);
    }
}
