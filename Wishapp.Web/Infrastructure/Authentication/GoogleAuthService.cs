using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Users.Features.GoogleSignIn;

namespace Wishapp.Web.Infrastructure.Authentication;

internal sealed class GoogleAuthService(
    ILogger<GoogleAuthService> logger,
    IOptions<GoogleOptions> options) : IGoogleAuthService
{
    public async Task<Result<GoogleUserInfo>> ValidateTokenAsync(string idToken, CancellationToken ct = default)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [ options.Value.ClientId ]
        };
        
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            
            return new GoogleUserInfo(
                payload.Subject, 
                payload.Email,
                payload.Name,
                payload.Picture);
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning("Google token validation failed: {Message}", ex.Message);
            
            return Error.Validation("Auth.InvalidToken", "Google token is invalid");
        }
    }
}