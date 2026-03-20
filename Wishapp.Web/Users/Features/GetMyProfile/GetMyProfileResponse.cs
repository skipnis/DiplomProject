namespace Wishapp.Web.Users.Features.GetMyProfile;

public record GetMyProfileResponse(
    Guid Id,
    string Username,
    string Email,
    string? AvatarUrl,
    string? Bio,
    DateOnly? BirthDate,
    bool IsGoogleCalendarConnected);