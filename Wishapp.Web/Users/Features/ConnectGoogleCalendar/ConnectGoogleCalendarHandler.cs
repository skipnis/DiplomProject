using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.GoogleCalendar;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Users.Features.ConnectGoogleCalendar;

public sealed class ConnectGoogleCalendarHandler(
    ApplicationDbContext db,
    IGoogleCalendarService calendarService)
    : ICommandHandler<ConnectGoogleCalendarCommand>
{
    private const string Provider = "google";
    private const string Scope = "calendar";

    public async Task<Result> HandleAsync(ConnectGoogleCalendarCommand command, CancellationToken ct = default)
    {
        var exchangeResult = await calendarService.ExchangeCodeAsync(command.Code, ct);

        if (exchangeResult.IsFailure)
        {
            return exchangeResult.Error;
        }

        var existing = await db.UserExternalTokens
            .FirstOrDefaultAsync(t => t.UserId == command.UserId && t.Provider == Provider && t.Scope == Scope, ct);

        if (existing is not null)
        {
            existing.UpdateRefreshToken(exchangeResult.Value);
        }
        else
        {
            db.UserExternalTokens.Add(
                UserExternalToken.Create(command.UserId, Provider, Scope, exchangeResult.Value));
        }

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
