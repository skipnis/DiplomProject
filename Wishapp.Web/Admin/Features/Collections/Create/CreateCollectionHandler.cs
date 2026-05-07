using Wishapp.Web.Catalog.Entities;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Collections.Create;

public sealed class CreateCollectionHandler(ApplicationDbContext db)
    : ICommandHandler<CreateCollectionCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateCollectionCommand command,
        CancellationToken ct = default)
    {
        var collection = CatalogCollection.Create(
            command.Name,
            command.Description,
            command.OccasionId,
            command.CoverImagePath,
            command.Order);

        db.CatalogCollections.Add(collection);
        await db.SaveChangesAsync(ct);

        return collection.Id;
    }
}
