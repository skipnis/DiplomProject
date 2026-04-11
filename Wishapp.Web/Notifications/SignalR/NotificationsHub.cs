using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Wishapp.Web.Notifications.SignalR;

[Authorize]
public sealed class NotificationsHub : Hub;
