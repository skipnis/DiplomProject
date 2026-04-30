using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Gamification.Features.VoteCatalogItemBadge;

public record VoteCatalogItemBadgeCommand(Guid UserId, Guid CatalogItemId, int BadgeType) : ICommand;
