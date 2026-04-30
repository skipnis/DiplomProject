using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;

namespace Wishapp.Web.Users.Features.DeleteAvatar;

public sealed class DeleteAvatarHandler(
    ApplicationDbContext db,
    IStorageService storageService)
    : ICommandHandler<DeleteAvatarCommand>
{
    public async Task<Result> HandleAsync(DeleteAvatarCommand command, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([command.UserId], ct);

        if (user is null)
        {
            return Error.NotFound("Users.NotFound", "User not found");
        }

        if (user.AvatarPath is null)
        {
            return Error.NotFound("Avatar.NotFound", "No custom avatar to delete");
        }

        await storageService.DeleteAsync(user.AvatarPath, ct);

        user.AvatarPath = null;

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
