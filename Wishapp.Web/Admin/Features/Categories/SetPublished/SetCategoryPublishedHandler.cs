using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Categories.SetPublished;

public sealed class SetCategoryPublishedHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<SetCategoryPublishedCommand>
{
    public async Task<Result> HandleAsync(SetCategoryPublishedCommand command, CancellationToken ct = default)
    {
        var category = await db.CatalogCategories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, ct);

        if (category is null)
            return Error.NotFound("Catalog.CategoryNotFound", "Category not found");

        category.SetPublished(command.IsPublished);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:categories", token: ct);

        return Result.Success();
    }
}
