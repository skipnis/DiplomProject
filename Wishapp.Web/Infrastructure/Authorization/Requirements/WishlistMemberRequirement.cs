using Microsoft.AspNetCore.Authorization;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Authorization.Requirements;

public sealed class WishlistMemberRequirement(WishlistMemberRole minimumRole) 
    : IAuthorizationRequirement
{
    public WishlistMemberRole MinimumRole { get; } = minimumRole;
}