using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.Delete;

public sealed class DeleteFulfilledBadgeDefinitionHandler(ApplicationDbContext db)
    : ICommandHandler<DeleteFulfilledBadgeDefinitionCommand>
{
    public async Task<Result> HandleAsync(
        DeleteFulfilledBadgeDefinitionCommand command,
        CancellationToken ct = default)
    {
        var definition = await db.FulfilledWishBadgeDefinitions.FirstOrDefaultAsync(b => b.Id == command.Id, ct);
        if (definition is null)
            return Error.NotFound("FulfilledBadges.NotFound", "Fulfilled badge definition not found");

        db.FulfilledWishBadgeDefinitions.Remove(definition);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
