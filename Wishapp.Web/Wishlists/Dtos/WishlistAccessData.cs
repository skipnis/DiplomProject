using Wishapp.Web.Infrastructure.Authorization;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Dtos;

public record WishlistAccessData(
    Guid OwnerId,
    WishlistVisibility Visibility,
    IReadOnlyCollection<WishlistMemberInfo> Members,
    bool IsSystem,
    bool IsSurpriseModeEnabled);
