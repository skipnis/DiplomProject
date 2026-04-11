using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Notifications.Features.MarkAllAsRead;

public sealed class MarkAllAsReadHandler(ApplicationDbContext db)
    : ICommandHandler<MarkAllAsReadCommand>
{
    public async Task<Result> HandleAsync(MarkAllAsReadCommand command, CancellationToken ct = default)
    {
        await db.Notifications
            .Where(n => n.UserId == command.UserId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);

        return Result.Success();
    }
}
