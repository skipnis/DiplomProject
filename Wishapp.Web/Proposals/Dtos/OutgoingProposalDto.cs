using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Proposals.Dtos;

public record OutgoingProposalDto(
    Guid Id,
    ProposalSourceType SourceType,
    ProposalStatus Status,
    string? HintMessage,
    string? RecipientComment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReactedAt,
    Guid RecipientId,
    string RecipientDisplayName,
    string? RecipientAvatarUrl,
    Guid? CatalogItemId,
    string? CatalogItemName,
    string? CatalogItemImagePath,
    decimal? CatalogItemPrice,
    Guid? WishlistItemId,
    string? WishlistItemName,
    string? WishlistItemImagePath,
    string? CustomTitle,
    string? CustomDescription,
    string? CustomImagePath);
