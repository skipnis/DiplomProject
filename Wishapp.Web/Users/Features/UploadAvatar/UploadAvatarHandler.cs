using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Infrastructure.Minio;

namespace Wishapp.Web.Users.Features.UploadAvatar;

public sealed class UploadAvatarHandler(
    ApplicationDbContext db,
    IStorageService storageService)
    : ICommandHandler<UploadAvatarCommand, UploadAvatarResponse>
{
    private const long MaxAvatarSize = 5 * 1024 * 1024;

    public async Task<Result<UploadAvatarResponse>> HandleAsync(
        UploadAvatarCommand command,
        CancellationToken ct = default)
    {
        if (command.File.Length > MaxAvatarSize)
        {
            return Error.Validation("Avatar.TooLarge", "Avatar must be less than 5MB");
        }

        if (!command.File.ContentType.StartsWith("image/"))
        {
            return Error.Validation("Avatar.InvalidType", "File must be an image");
        }

        var user = await db.Users.FindAsync([command.UserId], ct);

        if (user is null)
        {
            return Error.NotFound("Users.NotFound", "User not found");
        }

        if (user.AvatarPath is not null)
        {
            await storageService.DeleteAsync(user.AvatarPath, ct);
        }

        var path = StoragePaths.UserAvatar(command.UserId);

        await using var stream = command.File.OpenReadStream();
        await storageService.UploadAsync(path, stream, command.File.ContentType, command.File.Length, ct);

        user.AvatarPath = path;

        await db.SaveChangesAsync(ct);

        return new UploadAvatarResponse(path);
    }
}
