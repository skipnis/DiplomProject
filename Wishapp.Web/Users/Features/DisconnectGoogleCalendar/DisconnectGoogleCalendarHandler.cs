using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Users.Features.DisconnectGoogleCalendar;

public sealed class DisconnectGoogleCalendarHandler(ApplicationDbContext db)
    : ICommandHandler<DisconnectGoogleCalendarCommand>
{
    private const string Provider = "google";
    private const string Scope = "calendar";

    public async Task<Result> HandleAsync(DisconnectGoogleCalendarCommand command, CancellationToken ct = default)
    {
        var token = await db.UserExternalTokens
            .FirstOrDefaultAsync(t => t.UserId == command.UserId && t.Provider == Provider && t.Scope == Scope, ct);

        if (token is null)
        {
            return Result.Success();
        }

        db.UserExternalTokens.Remove(token);
        
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
