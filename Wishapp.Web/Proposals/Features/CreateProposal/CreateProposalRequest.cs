using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Proposals.Features.CreateProposal;

public record CreateProposalRequest(
    Guid RecipientId,
    ProposalSourceType SourceType,
    Guid? CatalogItemId,
    Guid? WishlistItemId,
    string? CustomTitle,
    string? CustomDescription,
    string? HintMessage,
    string? SenderAlias);
