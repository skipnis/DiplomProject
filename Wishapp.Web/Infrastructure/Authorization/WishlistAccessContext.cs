using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Authorization;

public record WishlistAccessContext(
    Guid WishlistId,
    Guid OwnerId,
    WishlistVisibility Visibility,
    IReadOnlyCollection<WishlistMemberInfo> Members,
    bool AreFriends = false);