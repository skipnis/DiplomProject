using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Events.Features.UpdateEvent;

public sealed class UpdateEventHandler(ApplicationDbContext db)
    : ICommandHandler<UpdateEventCommand>
{
    public async Task<Result> HandleAsync(
        UpdateEventCommand command,
        CancellationToken ct = default)
    {
        var @event = await db.Events
            .FirstOrDefaultAsync(e => e.Id == command.EventId, ct);

        if (@event is null)
        {
            return Error.NotFound("Events.NotFound", "Event not found");
        }

        if (@event.OwnerId != command.UserId)
        {
            return Error.Forbidden("Events.Forbidden", "Access denied");
        }

        @event.Update(command.Title, command.Description, command.Date);

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
