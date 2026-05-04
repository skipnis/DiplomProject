using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.Create;

public sealed class CreateFulfilledBadgeDefinitionHandler(ApplicationDbContext db)
    : ICommandHandler<CreateFulfilledBadgeDefinitionCommand, int>
{
    public async Task<Result<int>> HandleAsync(
        CreateFulfilledBadgeDefinitionCommand command,
        CancellationToken ct = default)
    {
        var definition = FulfilledWishBadgeDefinition.Create(command.Emoji, command.Slug, command.Label, command.Description);
        db.FulfilledWishBadgeDefinitions.Add(definition);
        await db.SaveChangesAsync(ct);
        return definition.Id;
    }
}
