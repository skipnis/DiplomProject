using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Wishapp.Web.Friendships;
using Wishapp.Web.Infrastructure.Authorization.Requirements;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Authorization.Handlers;

public sealed class WishlistFriendAuthorizationHandler
    : AuthorizationHandler<WishlistFriendRequirement, WishlistAccessContext>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WishlistFriendRequirement requirement,
        WishlistAccessContext resource)
    {
        var userIdResult = context.User.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            context.Fail();
            
            return Task.CompletedTask;
        }

        if (resource.AreFriends)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}