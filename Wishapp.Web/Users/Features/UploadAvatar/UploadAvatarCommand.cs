using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.UploadAvatar;

public record UploadAvatarCommand(Guid UserId, IFormFile File) : ICommand<UploadAvatarResponse>;
