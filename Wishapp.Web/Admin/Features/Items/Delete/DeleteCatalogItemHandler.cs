using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Items.Delete;

public sealed class DeleteCatalogItemHandler(ApplicationDbContext db)
    : ICommandHandler<DeleteCatalogItemCommand>
{
    public async Task<Result> HandleAsync(
        DeleteCatalogItemCommand command,
        CancellationToken ct = default)
    {
        var item = await db.CatalogItems
            .FirstOrDefaultAsync(i => i.Id == command.Id, ct);

        if (item is null)
        {
            return Error.NotFound("Catalog.ItemNotFound", "Catalog item not found");
        }

        db.CatalogItems.Remove(item);

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
