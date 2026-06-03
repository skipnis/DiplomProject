namespace Wishapp.Web.Users.Features.GetUserProfile;

public record GetUserProfileResponse(
    Guid Id,
    string DisplayName,
    string? Username,
    string? AvatarUrl,
    string? Bio,
    int ReceivedCount,
    int GiftedCount,
    DateOnly? BirthDate);