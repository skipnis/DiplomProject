using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.Create;

public sealed class CreateCatalogBadgeDefinitionHandler(ApplicationDbContext db)
    : ICommandHandler<CreateCatalogBadgeDefinitionCommand, int>
{
    public async Task<Result<int>> HandleAsync(
        CreateCatalogBadgeDefinitionCommand command,
        CancellationToken ct = default)
    {
        var definition = CatalogBadgeDefinition.Create(command.Emoji, command.Slug, command.Label, command.Description);
        db.CatalogBadgeDefinitions.Add(definition);
        await db.SaveChangesAsync(ct);
        return definition.Id;
    }
}
