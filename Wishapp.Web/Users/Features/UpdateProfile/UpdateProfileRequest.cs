namespace Wishapp.Web.Users.Features.UpdateProfile;

public record UpdateProfileRequest(string Username, string? Bio, DateOnly? BirthDate);