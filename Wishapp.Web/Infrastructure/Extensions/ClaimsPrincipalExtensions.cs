using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Result<Guid> TryGetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return sub is null
            ? Error.Unauthorized("Auth.InvalidToken", "User ID claim not found")
            : Result.Success(Guid.Parse(sub));
    }
}