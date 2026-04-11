using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Notifications.Features.MarkAllAsRead;

public record MarkAllAsReadCommand(Guid UserId) : ICommand;
