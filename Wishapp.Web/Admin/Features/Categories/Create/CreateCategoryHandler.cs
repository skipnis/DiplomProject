using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Entities;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Categories.Create;

public sealed class CreateCategoryHandler(ApplicationDbContext db, IFusionCache cache)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateCategoryCommand command,
        CancellationToken ct = default)
    {
        var exists = await db.CatalogCategories
            .AnyAsync(c => c.Name == command.Name || c.Order == command.Order, ct);

        if (exists)
        {
            return Error.Conflict("Catalog.CategoryExists", "Category with this name or order already exists");
        }

        var category = CatalogCategory.Create(command.Name, command.Order);

        db.CatalogCategories.Add(category);

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("catalog:categories", token: ct);

        return category.Id;
    }
}
