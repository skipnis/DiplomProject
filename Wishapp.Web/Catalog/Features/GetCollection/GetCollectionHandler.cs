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
        var collection = await db.CatalogCollections
            .AsNoTracking()
            .Where(c => c.Id == query.Id && c.IsPublished)
            .Select(c => new CatalogCollectionDto(
                c.Id,
                c.Name,
                c.Description,
                c.Occasion,
                c.CoverImagePath,
                c.Order,
                c.Items
                    .Where(i => i.CatalogItem.IsPublished)
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
                        i.CatalogItem.UpdatedAt,
                        i.CatalogItem.Ratings.Any() ? i.CatalogItem.Ratings.Average(r => (double)r.Value) : (double?)null,
                        i.CatalogItem.Ratings.Count,
                        null,
                        i.CatalogItem.WishCount,
                        i.Description))
                    .ToList()))
            .FirstOrDefaultAsync(ct);

        if (collection is null)
            return Error.NotFound("Catalog.CollectionNotFound", "Collection not found");

        return collection;
    }
}
