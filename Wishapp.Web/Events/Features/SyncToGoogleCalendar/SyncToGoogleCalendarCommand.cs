using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Events.Features.SyncToGoogleCalendar;

public record SyncToGoogleCalendarCommand(Guid EventId, Guid UserId) : ICommand;
