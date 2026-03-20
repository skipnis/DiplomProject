using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.ConnectGoogleCalendar;

public record ConnectGoogleCalendarCommand(Guid UserId, string Code) : ICommand;
