using Wishapp.Web.Infrastructure.Validation;
using Wishapp.Web.Users.Features.ConnectGoogleCalendar;
using Wishapp.Web.Users.Features.DeleteAvatar;
using Wishapp.Web.Users.Features.DeleteMyAccount;
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

        usersEndpoints.MapGet("/search", SearchUsers).Produces(401);;

        usersEndpoints.MapPost("/me/google-calendar", ConnectGoogleCalendar).Produces(401)
            .AddEndpointFilter<ValidationFilter<ConnectGoogleCalendarRequest>>();

        usersEndpoints.MapDelete("/me/google-calendar", DisconnectGoogleCalendar).Produces(401);

        usersEndpoints.MapPost("/me/avatar", UploadAvatar).Produces(401)
            .DisableAntiforgery();

        usersEndpoints.MapDelete("/me/avatar", DeleteAvatar).Produces(401);

        usersEndpoints.MapDelete("/me", DeleteMyAccount).Produces(401);

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
            Path = "/"
        };

        httpContext.Response.Cookies.Append("access_token", accessToken, cookieOptions);
        httpContext.Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
    }

    private static IResult Logout(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete("access_token");
        httpContext.Response.Cookies.Delete("refresh_token");
        return Results.Ok();
    }
}
