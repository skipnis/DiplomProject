using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Events.Features.SyncAllEvents;

public record SyncAllEventsCommand(Guid UserId) : ICommand;
