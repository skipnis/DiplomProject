using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Authentication;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Users.Entities;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Users.Features.GoogleSignIn;

public sealed class GoogleSignInHandler(
    ApplicationDbContext db,
    IGoogleAuthService googleAuthService,
    ITokenProvider tokenProvider,
    IWishlistsApi wishlistsApi,
    IOptions<JwtOptions> jwtOptions)
    : ICommandHandler<GoogleSignInCommand, GoogleSignInResponse>
{
    private readonly JwtOptions _jwt = jwtOptions.Value;
    public async Task<Result<GoogleSignInResponse>> HandleAsync(
        GoogleSignInCommand command,
        CancellationToken ct = default)
    {
        var result = await googleAuthService.ValidateTokenAsync(command.IdToken, ct);
        
        if (result.IsFailure) return result.Error;

        var identity = await db.AuthIdentities
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.Provider == AuthProvider.Google &&
                a.ProviderKey == result.Value.Subject, ct);

        User user;

        if (identity is null)
        {
            user = User.Create(result.Value.Name, result.Value.Email, result.Value.Picture);
            
            var authIdentity = AuthIdentity.Create(user.Id, AuthProvider.Google, result.Value.Subject);

            db.Users.Add(user);

            db.AuthIdentities.Add(authIdentity);

            await wishlistsApi.CreateSystemWishlistsAsync(user.Id, ct);

            await db.SaveChangesAsync(ct);
        }
        else
        {
            user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == identity.UserId, ct) ?? throw new InvalidOperationException();
        }
        
        var accessToken = tokenProvider.Create(user);
        var refreshToken = tokenProvider.CreateRefreshToken();
        var tokenHash = tokenProvider.HashToken(refreshToken);

        db.RefreshTokens.Add(UserRefreshToken.Create(user.Id, tokenHash, _jwt.RefreshTokenExpirationInDays));
        await db.SaveChangesAsync(ct);

        return Result.Success(new GoogleSignInResponse(accessToken, refreshToken));
    }
}