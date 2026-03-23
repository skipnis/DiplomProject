using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Catalog.Features.GetCatalogItem;

public sealed class GetCatalogItemHandler(ApplicationDbContext db)
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
            .Select(i => new CatalogItemDto(
                i.Id, i.Name, i.Description,
                i.Price, i.Currency != null ? i.Currency.ToString() : null,
                i.ImagePath, i.Url,
                i.CategoryId, i.Category.Name,
                i.IsPublished, i.CreatedAt, i.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        if (item is null)
        {
            return Error.NotFound("Catalog.NotFound", "Catalog item not found");
        }

        return item;
    }
}
