using Microsoft.AspNetCore.Authorization;
using Wishapp.Web.Infrastructure.Authorization.Requirements;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Authorization.Handlers;

public sealed class WishlistMemberAuthorizationHandler
    : AuthorizationHandler<WishlistMemberRequirement, WishlistAccessContext>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WishlistMemberRequirement requirement,
        WishlistAccessContext resource)
    {
        var userIdResult = context.User.TryGetUserId();

        if (userIdResult.IsFailure)
        {
            context.Fail();
            
            return Task.CompletedTask;
        }

        var member = resource.Members
            .FirstOrDefault(m => m.UserId == userIdResult.Value);

        if (member is not null && member.Role >= requirement.MinimumRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}