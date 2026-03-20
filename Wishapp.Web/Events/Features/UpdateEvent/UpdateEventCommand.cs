using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Events.Features.UpdateEvent;

public record UpdateEventCommand(
    Guid EventId,
    Guid UserId,
    string Title,
    string? Description,
    DateOnly Date) : ICommand;
