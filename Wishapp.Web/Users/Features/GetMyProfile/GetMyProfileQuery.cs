using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Users.Features.GetMyProfile;

public record GetMyProfileQuery(Guid UserId) : IQuery<GetMyProfileResponse>;