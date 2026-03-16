using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Authorization;

public record WishlistMemberInfo(Guid UserId, WishlistMemberRole Role);