using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Authentication;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Users.Features.RefreshToken;

public sealed class RefreshTokenHandler(
    ApplicationDbContext db,
    ITokenProvider tokenProvider,
    IOptions<JwtOptions> jwtOptions)
    : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<Result<RefreshTokenResponse>> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken ct = default)
    {
        var tokenHash = tokenProvider.HashToken(command.RefreshToken);

        var existing = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (existing is null || !existing.IsActive)
            return Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token is invalid or expired");

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == existing.UserId, ct);

        if (user is null)
            return Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token is invalid or expired");

        existing.Revoke();

        var newRefreshToken = tokenProvider.CreateRefreshToken();
        var newTokenHash = tokenProvider.HashToken(newRefreshToken);

        db.RefreshTokens.Add(UserRefreshToken.Create(user.Id, newTokenHash, _jwt.RefreshTokenExpirationInDays));

        await db.SaveChangesAsync(ct);

        var accessToken = tokenProvider.Create(user);

        return Result.Success(new RefreshTokenResponse(accessToken, newRefreshToken));
    }
}
