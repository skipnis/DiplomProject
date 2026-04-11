using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Notifications.Features.GetUnreadCount;

public record GetUnreadCountQuery(Guid UserId) : IQuery<int>;
