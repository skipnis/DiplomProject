using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Users.Features.UpdateBlacklistItem;

public sealed class UpdateBlacklistItemHandler(ApplicationDbContext db)
    : ICommandHandler<UpdateBlacklistItemCommand>
{
    public async Task<Result> HandleAsync(
        UpdateBlacklistItemCommand command,
        CancellationToken ct = default)
    {
        var item = await db.BlacklistItems
            .FirstOrDefaultAsync(i => i.Id == command.ItemId && i.UserId == command.UserId, ct);

        if (item is null)
            return Error.NotFound("Blacklist.NotFound", "Blacklist item not found");

        item.UpdateTitle(command.Title);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
