using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Catalog.Features.GetCatalogItem;

public sealed class GetCatalogItemHandler(ApplicationDbContext db, IGamificationApi gamification)
    : IQueryHandler<GetCatalogItemQuery, CatalogItemDto>
{
    public async Task<Result<CatalogItemDto>> HandleAsync(
        GetCatalogItemQuery query,
        CancellationToken ct = default)
    {
        var item = await db.CatalogItems
            .AsNoTracking()
            .Include(i => i.Category)
            .Where(i => i.Id == query.Id && i.IsPublished)
            .Select(i => new
            {
                i.Id, i.Name, i.Description, i.Price, i.Currency,
                i.ImagePath, i.Url, i.CategoryId, CategoryName = i.Category.Name,
                i.IsPublished, i.CreatedAt, i.UpdatedAt, i.WishCount,
            })
            .FirstOrDefaultAsync(ct);

        if (item is null)
            return Error.NotFound("Catalog.NotFound", "Catalog item not found");

        var badgesByItemId = await gamification.GetBadgesForItemsAsync([item.Id], query.UserId, ct);

        var occasions = await db.CatalogItemOccasions
            .AsNoTracking()
            .Where(o => o.CatalogItemId == item.Id)
            .Select(o => new OccasionDto(o.Occasion.Id, o.Occasion.Key, o.Occasion.Label, o.Occasion.Order))
            .ToListAsync(ct);

        return new CatalogItemDto(
            item.Id, item.Name, item.Description,
            item.Price, item.Currency?.ToString(),
            item.ImagePath, item.Url,
            item.CategoryId, item.CategoryName,
            item.IsPublished, item.CreatedAt, item.UpdatedAt,
            item.WishCount, null,
            badgesByItemId.GetValueOrDefault(item.Id, []),
            occasions);
    }
}
