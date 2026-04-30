using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.DeleteAvatar;

public record DeleteAvatarCommand(Guid UserId) : ICommand;
