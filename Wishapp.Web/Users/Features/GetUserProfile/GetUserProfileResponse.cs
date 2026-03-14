namespace Wishapp.Web.Users.Features.GetUserProfile;

public record GetUserProfileResponse(
    Guid Id,
    string Username,
    string? AvatarUrl,
    string? Bio);