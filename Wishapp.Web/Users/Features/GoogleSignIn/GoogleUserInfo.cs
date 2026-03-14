namespace Wishapp.Web.Users.Features.GoogleSignIn;

public record GoogleUserInfo(
    string Subject,
    string Email,
    string Name,
    string? Picture);