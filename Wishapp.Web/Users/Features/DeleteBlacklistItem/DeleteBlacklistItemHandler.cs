using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Users.Features.DeleteBlacklistItem;

public sealed class DeleteBlacklistItemHandler(ApplicationDbContext db)
    : ICommandHandler<DeleteBlacklistItemCommand>
{
    public async Task<Result> HandleAsync(
        DeleteBlacklistItemCommand command,
        CancellationToken ct = default)
    {
        var item = await db.BlacklistItems
            .FirstOrDefaultAsync(i => i.Id == command.ItemId && i.UserId == command.UserId, ct);

        if (item is null)
            return Error.NotFound("Blacklist.NotFound", "Blacklist item not found");

        db.BlacklistItems.Remove(item);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
