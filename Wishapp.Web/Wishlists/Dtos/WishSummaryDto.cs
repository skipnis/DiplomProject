using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Dtos;

public record WishSummaryDto(
    Guid Id,
    string Name,
    decimal? Price,
    Currency? Currency,
    WishPriority Priority,
    string? ImagePath,
    bool IsFulfilled,
    bool IsReserved,
    bool HasGiftBadges);
