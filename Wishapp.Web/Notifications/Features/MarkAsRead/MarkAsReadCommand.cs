using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Notifications.Features.MarkAsRead;

public record MarkAsReadCommand(Guid NotificationId, Guid UserId) : ICommand;
