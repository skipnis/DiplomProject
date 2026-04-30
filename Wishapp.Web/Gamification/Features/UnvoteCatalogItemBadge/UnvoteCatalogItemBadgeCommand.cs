using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Gamification.Features.UnvoteCatalogItemBadge;

public record UnvoteCatalogItemBadgeCommand(Guid UserId, Guid CatalogItemId, int BadgeType) : ICommand;
