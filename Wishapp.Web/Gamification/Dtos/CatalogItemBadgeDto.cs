namespace Wishapp.Web.Gamification.Dtos;

public record CatalogItemBadgeDto(int BadgeType, string Emoji, string Slug, string Label, int VoteCount, bool MyVote);
