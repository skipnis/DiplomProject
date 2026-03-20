using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.DisconnectGoogleCalendar;

public record DisconnectGoogleCalendarCommand(Guid UserId) : ICommand;
