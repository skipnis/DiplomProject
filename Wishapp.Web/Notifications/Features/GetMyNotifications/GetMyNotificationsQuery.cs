using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Notifications.Dtos;

namespace Wishapp.Web.Notifications.Features.GetMyNotifications;

public record GetMyNotificationsQuery(Guid UserId, PagedRequest Request) : IQuery<PagedResponse<NotificationDto>>;
