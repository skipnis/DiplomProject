using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Proposals.Dtos;

public record IncomingProposalDto(
    Guid Id,
    ProposalSourceType SourceType,
    string SenderAlias,
    string? HintMessage,
    bool IsViewedByRecipient,
    ProposalStatus Status,
    string? RecipientComment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReactedAt,
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
