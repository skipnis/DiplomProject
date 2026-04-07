using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Categories.Delete;

public sealed class DeleteCategoryHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> HandleAsync(
        DeleteCategoryCommand command,
        CancellationToken ct = default)
    {
        var category = await db.CatalogCategories
            .FirstOrDefaultAsync(c => c.Id == command.Id, ct);

        if (category is null)
        {
            return Error.NotFound("Catalog.CategoryNotFound", "Category not found");
        }

        var hasItems = await db.CatalogItems
            .AnyAsync(i => i.CategoryId == command.Id, ct);

        if (hasItems)
        {
            return Error.Conflict("Catalog.CategoryHasItems", "Cannot delete category with existing items");
        }

        db.CatalogCategories.Remove(category);

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:categories", token: ct);

        return Result.Success();
    }
}
