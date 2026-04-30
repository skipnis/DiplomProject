using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Dtos;

namespace Wishapp.Web.Gamification.Features.GetUserGiftProfile;

public record GetUserGiftProfileQuery(Guid TargetUserId) : IQuery<GiftProfileDto>;
