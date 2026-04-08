namespace Wishapp.Web.Users.Features.GetMyProfile;

public record GetMyProfileResponse(
    Guid Id,
    string DisplayName,
    string? Username,
    string Email,
    string? AvatarUrl,
    string? Bio,
    DateOnly? BirthDate,
    bool IsGoogleCalendarConnected,
    bool IsOnboarded);