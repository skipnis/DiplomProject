using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Collections.SetPublished;

public sealed class SetCollectionPublishedHandler(ApplicationDbContext db)
    : ICommandHandler<SetCollectionPublishedCommand>
{
    public async Task<Result> HandleAsync(SetCollectionPublishedCommand command, CancellationToken ct = default)
    {
        var collection = await db.CatalogCollections.FirstOrDefaultAsync(c => c.Id == command.CollectionId, ct);

        if (collection is null)
            return Error.NotFound("Catalog.CollectionNotFound", "Collection not found");

        collection.SetPublished(command.IsPublished);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
