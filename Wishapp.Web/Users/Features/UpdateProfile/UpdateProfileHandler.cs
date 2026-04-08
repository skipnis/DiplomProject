using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Users.Features.UpdateProfile;

public sealed class UpdateProfileHandler(ApplicationDbContext db)
    : ICommandHandler<UpdateProfileCommand>
{
    public async Task<Result> HandleAsync(
        UpdateProfileCommand command,
        CancellationToken ct = default)
    {
        var user = await db.Users
            .FindAsync([command.UserId], ct);

        if (user is null)
        {
            return Error.NotFound("Users.NotFound", "User not found");
        }

        var usernameTaken = await db.Users
            .AnyAsync(u => u.Username == command.Username && u.Id != command.UserId, ct);

        if (usernameTaken)
        {
            return Error.Conflict("Users.UsernameAlreadyTaken", "Username is already taken");
        }

        user.DisplayName = command.DisplayName;
        user.Username = command.Username;
        user.Bio = command.Bio;
        user.BirthDate = command.BirthDate;
        user.IsOnboarded = true;

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}