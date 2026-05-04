using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.Delete;

public sealed class DeleteCatalogBadgeDefinitionHandler(ApplicationDbContext db)
    : ICommandHandler<DeleteCatalogBadgeDefinitionCommand>
{
    public async Task<Result> HandleAsync(
        DeleteCatalogBadgeDefinitionCommand command,
        CancellationToken ct = default)
    {
        var definition = await db.CatalogBadgeDefinitions.FirstOrDefaultAsync(b => b.Id == command.Id, ct);
        if (definition is null)
            return Error.NotFound("CatalogBadges.NotFound", "Catalog badge definition not found");

        db.CatalogBadgeDefinitions.Remove(definition);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
