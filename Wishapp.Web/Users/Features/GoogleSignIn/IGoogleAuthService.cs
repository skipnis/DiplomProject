using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Users.Features.GoogleSignIn;

public interface IGoogleAuthService
{
    Task<Result<GoogleUserInfo>> ValidateTokenAsync(string idToken, CancellationToken ct = default);
}