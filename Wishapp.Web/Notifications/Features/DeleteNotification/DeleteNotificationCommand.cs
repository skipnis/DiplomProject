using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Notifications.Features.DeleteNotification;

public record DeleteNotificationCommand(Guid NotificationId, Guid UserId) : ICommand;
