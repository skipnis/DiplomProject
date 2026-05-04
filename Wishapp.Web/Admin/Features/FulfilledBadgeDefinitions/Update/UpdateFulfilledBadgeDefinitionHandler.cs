using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.Update;

public sealed class UpdateFulfilledBadgeDefinitionHandler(ApplicationDbContext db)
    : ICommandHandler<UpdateFulfilledBadgeDefinitionCommand>
{
    public async Task<Result> HandleAsync(
        UpdateFulfilledBadgeDefinitionCommand command,
        CancellationToken ct = default)
    {
        var definition = await db.FulfilledWishBadgeDefinitions.FirstOrDefaultAsync(b => b.Id == command.Id, ct);
        if (definition is null)
            return Error.NotFound("FulfilledBadges.NotFound", "Fulfilled badge definition not found");

        definition.Update(command.Emoji, command.Slug, command.Label, command.Description, command.IsActive);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
