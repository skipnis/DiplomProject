using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Gamification.Dtos;

namespace Wishapp.Web.Gamification.Features.GetMyGiftProfile;

public record GetMyGiftProfileQuery(Guid UserId) : IQuery<GiftProfileDto>;
