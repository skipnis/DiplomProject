using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Catalog.Api;

internal sealed class CatalogApi(ApplicationDbContext db) : ICatalogApi
{
    public async Task<CatalogItemData?> GetCatalogItemDataAsync(Guid id, CancellationToken ct = default)
    {
        return await db.CatalogItems
            .AsNoTracking()
            .Where(i => i.Id == id && i.IsPublished)
            .Select(i => new CatalogItemData(i.Name, i.Description, i.Price, i.Currency, i.ImagePath, i.Url))
            .FirstOrDefaultAsync(ct);
    }

    public async Task IncrementWishCountAsync(Guid id, CancellationToken ct = default)
    {
        var item = await db.CatalogItems.FindAsync([id], ct);
        item?.IncrementWishCount();
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> ItemExistsAsync(Guid id, CancellationToken ct = default) =>
        db.CatalogItems.AnyAsync(item => item.Id == id, ct);
}
