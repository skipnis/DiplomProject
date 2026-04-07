using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Categories.Update;

public sealed class UpdateCategoryHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> HandleAsync(
        UpdateCategoryCommand command,
        CancellationToken ct = default)
    {
        var category = await db.CatalogCategories
            .FirstOrDefaultAsync(c => c.Id == command.Id, ct);

        if (category is null)
        {
            return Error.NotFound("Catalog.CategoryNotFound", "Category not found");
        }

        var orderTaken = await db.CatalogCategories
            .AnyAsync(c => c.Id != command.Id && c.Order == command.Order, ct);

        if (orderTaken)
        {
            return Error.Conflict("Catalog.CategoryOrderExists", "Category with this order already exists");
        }

        category.Update(command.Name, command.Order);

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:categories", token: ct);

        return Result.Success();
    }
}
