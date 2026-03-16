using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Wishlists.Features.Members.RemoveMember;

public sealed class RemoveMemberHandler(ApplicationDbContext db)
    : ICommandHandler<RemoveMemberCommand>
{
    public async Task<Result> HandleAsync(
        RemoveMemberCommand command,
        CancellationToken ct = default)
    {
        var wishlist = await db.Wishlists
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == command.WishlistId, ct);

        if (wishlist is null)
        {
            return Error.NotFound("Wishlists.NotFound", "Wishlist not found");
        }

        var result = wishlist.RemoveMember(command.UserId);

        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}