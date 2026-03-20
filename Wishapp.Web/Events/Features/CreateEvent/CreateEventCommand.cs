using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Events.Features.CreateEvent;

public record CreateEventCommand(
    Guid OwnerId,
    string Title,
    string? Description,
    DateOnly Date) : ICommand<CreateEventResponse>;
