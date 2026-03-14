using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.GetUserProfile;

public record GetUserProfileQuery(Guid TargetUserId) : IQuery<GetUserProfileResponse>;