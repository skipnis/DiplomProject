using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Events.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Events.Features.CreateEvent;

public sealed class CreateEventHandler(ApplicationDbContext db)
    : ICommandHandler<CreateEventCommand, CreateEventResponse>
{
    public async Task<Result<CreateEventResponse>> HandleAsync(
        CreateEventCommand command,
        CancellationToken ct = default)
    {
        var @event = Event.Create(command.OwnerId, command.Title, command.Description, command.Date);

        db.Events.Add(@event);
        
        await db.SaveChangesAsync(ct);

        return new CreateEventResponse(@event.Id);
    }
}
