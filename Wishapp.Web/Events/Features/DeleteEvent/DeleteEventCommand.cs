using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Events.Features.DeleteEvent;

public record DeleteEventCommand(Guid EventId, Guid UserId) : ICommand;
