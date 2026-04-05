using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Authentication;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Users.Entities;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Users.Features.VerifyOtp;

public sealed class VerifyOtpHandler(
    ApplicationDbContext db,
    ITokenProvider tokenProvider,
    IWishlistsApi wishlistsApi,
    IOptions<JwtOptions> jwtOptions)
    : ICommandHandler<VerifyOtpCommand, VerifyOtpResponse>
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    private static readonly Error InvalidCode =
        Error.Unauthorized("Otp.InvalidCode", "Invalid or expired code.");

    public async Task<Result<VerifyOtpResponse>> HandleAsync(
        VerifyOtpCommand command,
        CancellationToken ct = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var codeHash = HashCode(command.Code);

        var otp = await db.EmailOtps
            .Where(o => o.Email == email && !o.UsedAt.HasValue)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otp is null || otp.IsExpired)
            return InvalidCode;

        if (otp.CodeHash != codeHash)
        {
            otp.AttemptCount++;
            await db.SaveChangesAsync(ct);
            return InvalidCode;
        }

        if (!otp.IsValid)
            return InvalidCode;

        otp.UsedAt = DateTime.UtcNow;

        var identity = await db.AuthIdentities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Provider == AuthProvider.Email && a.ProviderKey == email, ct);

        User user;

        if (identity is null)
        {
            var existingUser = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, ct);

            if (existingUser is not null)
            {
                user = existingUser;
                db.AuthIdentities.Add(AuthIdentity.Create(user.Id, AuthProvider.Email, email));
            }
            else
            {
                var username = email.Split('@')[0];
                user = User.Create(username, email, null);

                db.Users.Add(user);
                db.AuthIdentities.Add(AuthIdentity.Create(user.Id, AuthProvider.Email, email));

                await wishlistsApi.CreateSystemWishlistsAsync(user.Id, ct);
            }
        }
        else
        {
            user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == identity.UserId, ct)
                ?? throw new InvalidOperationException();
        }

        var accessToken = tokenProvider.Create(user);
        var refreshToken = tokenProvider.CreateRefreshToken();
        var tokenHash = tokenProvider.HashToken(refreshToken);

        db.RefreshTokens.Add(UserRefreshToken.Create(user.Id, tokenHash, _jwt.RefreshTokenExpirationInDays));
        await db.SaveChangesAsync(ct);

        return Result.Success(new VerifyOtpResponse(accessToken, refreshToken));
    }

    private static string HashCode(string code)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(hash);
    }
}
