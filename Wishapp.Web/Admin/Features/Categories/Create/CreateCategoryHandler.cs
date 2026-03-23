using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Entities;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Categories.Create;

public sealed class CreateCategoryHandler(ApplicationDbContext db)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateCategoryCommand command,
        CancellationToken ct = default)
    {
        var exists = await db.CatalogCategories
            .AnyAsync(c => c.Name == command.Name, ct);

        if (exists)
        {
            return Error.Conflict("Catalog.CategoryExists", "Category with this name already exists");
        }

        var category = CatalogCategory.Create(command.Name, command.Order);

        db.CatalogCategories.Add(category);

        await db.SaveChangesAsync(ct);

        return category.Id;
    }
}
