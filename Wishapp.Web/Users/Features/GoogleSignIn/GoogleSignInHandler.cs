using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Users.Features.GoogleSignIn;

public sealed class GoogleSignInHandler(
    ApplicationDbContext db,
    IGoogleAuthService googleAuthService,
    ITokenProvider tokenProvider)
    : ICommandHandler<GoogleSignInCommand, GoogleSignInResponse>
{
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
            
            await db.SaveChangesAsync(ct);
        }
        else
        {
            user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == identity.UserId, ct) ?? throw new InvalidOperationException();
        }
        
        var token = tokenProvider.Create(user);

        return Result.Success(new GoogleSignInResponse(token));
    }
}