using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Proposals.Dtos;

public record ProposalDetailDto(
    Guid Id,
    ProposalSourceType SourceType,
    string SenderAlias,
    string? HintMessage,
    bool IsViewedByRecipient,
    ProposalStatus Status,
    string? RecipientComment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReactedAt,
    bool IsOwnProposal,
    Guid? RecipientId,
    string? RecipientDisplayName,
    string? RecipientAvatarUrl,
    Guid? CatalogItemId,
    string? CatalogItemName,
    string? CatalogItemImagePath,
    decimal? CatalogItemPrice,
    string? CatalogItemUrl,
    Guid? WishlistItemId,
    string? WishlistItemName,
    string? WishlistItemImagePath,
    string? WishlistItemDescription,
    string? CustomTitle,
    string? CustomDescription,
    string? CustomImagePath);
