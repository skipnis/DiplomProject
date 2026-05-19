using Wishapp.Web.Infrastructure.Authentication;
using Wishapp.Web.Infrastructure.Validation;
using Wishapp.Web.Users.Features.AddBlacklistItem;
using Wishapp.Web.Users.Features.DeleteAvatar;
using Wishapp.Web.Users.Features.DeleteMyAccount;
using Wishapp.Web.Users.Features.RequestDeleteMyAccount;
using Wishapp.Web.Users.Features.UpdateBlacklistItem;
using Wishapp.Web.Users.Features.UpdateProfile;
using Wishapp.Web.Users.Features.SendOtp;
using Wishapp.Web.Users.Features.UploadAvatar;
using Wishapp.Web.Users.Features.VerifyOtp;

namespace Wishapp.Web.Users;

public static partial class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth");

        auth.MapPost("/google", GoogleSignIn);
        auth.MapPost("/email/send-otp", SendOtp)
            .AddEndpointFilter<ValidationFilter<SendOtpCommand>>();
        auth.MapPost("/email/verify-otp", VerifyOtp)
            .AddEndpointFilter<ValidationFilter<VerifyOtpCommand>>();
        auth.MapPost("/refresh", RefreshToken);
        auth.MapPost("/logout", Logout);

        var usersEndpoints = app.MapGroup("/users")
            .RequireAuthorization();

        usersEndpoints.MapGet("/me", GetMyProfile).Produces(401);;

        usersEndpoints.MapPut("/me", UpdateProfile).Produces(401)
            .AddEndpointFilter<ValidationFilter<UpdateProfileRequest>>();

        usersEndpoints.MapGet("/{id:guid}", GetUserProfile).AllowAnonymous();

        usersEndpoints.MapGet("/{id:guid}/fulfilled-wishes", GetUserFulfilledWishes).AllowAnonymous();

        usersEndpoints.MapGet("/search", SearchUsers).Produces(401);;

        usersEndpoints.MapPost("/me/avatar", UploadAvatar).Produces(401)
            .DisableAntiforgery();

        usersEndpoints.MapDelete("/me/avatar", DeleteAvatar).Produces(401);

        usersEndpoints.MapPost("/me/delete-confirmation", RequestAccountDeletion).Produces(401);

        usersEndpoints.MapDelete("/me", DeleteMyAccount).Produces(401);

        usersEndpoints.MapGet("/check-username", CheckUsernameAvailability).Produces(401);

        usersEndpoints.MapGet("/me/blacklist", GetMyBlacklist).Produces(401);

        usersEndpoints.MapGet("/{id:guid}/blacklist", GetUserBlacklist).Produces(401).Produces(403);

        usersEndpoints.MapPost("/me/blacklist", AddBlacklistItem).Produces(401)
            .AddEndpointFilter<ValidationFilter<AddBlacklistItemRequest>>();

        usersEndpoints.MapDelete("/me/blacklist/{itemId:guid}", DeleteBlacklistItem).Produces(401);

        usersEndpoints.MapPut("/me/blacklist/{itemId:guid}", UpdateBlacklistItem).Produces(401)
            .AddEndpointFilter<ValidationFilter<UpdateBlacklistItemRequest>>();

        return app;
    }

    private static void SetAuthCookies(HttpContext httpContext, string accessToken, string refreshToken)
    {
        var isSecure = httpContext.Request.IsHttps;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        };

        httpContext.Response.Cookies.Append(CookieNames.AccessToken, accessToken, cookieOptions);
        httpContext.Response.Cookies.Append(CookieNames.RefreshToken, refreshToken, cookieOptions);
    }

    private static IResult Logout(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(CookieNames.AccessToken);
        httpContext.Response.Cookies.Delete(CookieNames.RefreshToken);
        return Results.Ok();
    }
}
