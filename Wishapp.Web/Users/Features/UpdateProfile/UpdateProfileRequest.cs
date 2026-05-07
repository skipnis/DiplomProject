namespace Wishapp.Web.Users.Features.UpdateProfile;

public record UpdateProfileRequest(string DisplayName, string Username, string? Bio, DateOnly? BirthDate, bool ShowFulfilledWishes);