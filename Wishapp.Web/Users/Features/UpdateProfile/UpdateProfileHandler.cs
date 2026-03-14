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

        user.Username = command.Username;
        
        user.Bio = command.Bio;
        
        user.BirthDate = command.BirthDate;

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}