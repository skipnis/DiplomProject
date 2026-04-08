using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    string DisplayName,
    string Username,
    string? Bio,
    DateOnly? BirthDate) : ICommand;