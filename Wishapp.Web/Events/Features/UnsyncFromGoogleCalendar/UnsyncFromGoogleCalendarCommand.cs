using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Events.Features.UnsyncFromGoogleCalendar;

public record UnsyncFromGoogleCalendarCommand(Guid EventId, Guid UserId) : ICommand;
