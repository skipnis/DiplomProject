using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Members.UpdateMemberRole;

public record UpdateMemberRoleRequest(WishlistMemberRole Role, string? CustomRoleName);