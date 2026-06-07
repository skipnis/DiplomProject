using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Categories.SwapOrder;

public sealed class SwapCategoryOrderHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<SwapCategoryOrderCommand>
{
    public async Task<Result> HandleAsync(SwapCategoryOrderCommand command, CancellationToken ct = default)
    {
        var category = await db.CatalogCategories.FirstOrDefaultAsync(c => c.Id == command.Id, ct);
        if (category is null)
            return Error.NotFound("Catalog.CategoryNotFound", "Category not found");

        var target = await db.CatalogCategories.FirstOrDefaultAsync(c => c.Id == command.TargetId, ct);
        if (target is null)
            return Error.NotFound("Catalog.CategoryNotFound", "Target category not found");

        var tempOrder = category.Order;
        category.SetOrder(target.Order);
        target.SetOrder(tempOrder);

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:categories", token: ct);

        return Result.Success();
    }
}
