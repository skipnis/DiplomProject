using System.Text.Json;
using Wishapp.Web.Notifications.Entities;

namespace Wishapp.Web.Notifications.Dtos;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    JsonElement Payload,
    bool IsRead,
    DateTimeOffset CreatedAt);
