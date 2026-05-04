using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.Update;

public sealed class UpdateCatalogBadgeDefinitionHandler(ApplicationDbContext db)
    : ICommandHandler<UpdateCatalogBadgeDefinitionCommand>
{
    public async Task<Result> HandleAsync(
        UpdateCatalogBadgeDefinitionCommand command,
        CancellationToken ct = default)
    {
        var definition = await db.CatalogBadgeDefinitions.FirstOrDefaultAsync(b => b.Id == command.Id, ct);
        if (definition is null)
            return Error.NotFound("CatalogBadges.NotFound", "Catalog badge definition not found");

        definition.Update(command.Emoji, command.Slug, command.Label, command.Description, command.IsActive);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
