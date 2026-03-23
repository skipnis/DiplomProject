using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Items.SetPublished;

public sealed class SetCatalogItemPublishedHandler(ApplicationDbContext db)
    : ICommandHandler<SetCatalogItemPublishedCommand>
{
    public async Task<Result> HandleAsync(SetCatalogItemPublishedCommand command, CancellationToken ct = default)
    {
        var item = await db.CatalogItems.FirstOrDefaultAsync(i => i.Id == command.ItemId, ct);

        if (item is null)
            return Error.NotFound("Catalog.ItemNotFound", "Item not found");

        item.SetPublished(command.IsPublished);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
