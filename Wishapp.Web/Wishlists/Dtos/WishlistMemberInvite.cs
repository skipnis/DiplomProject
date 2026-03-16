using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Dtos;

public record WishlistMemberInvite(Guid UserId, WishlistMemberRole Role);