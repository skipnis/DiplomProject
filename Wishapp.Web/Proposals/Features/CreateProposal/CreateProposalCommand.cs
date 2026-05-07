using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Proposals.Features.CreateProposal;

public record CreateProposalCommand(
    Guid SenderId,
    Guid RecipientId,
    ProposalSourceType SourceType,
    Guid? CatalogItemId,
    Guid? WishlistItemId,
    string? CustomTitle,
    string? CustomDescription,
    string? HintMessage,
    string? SenderAlias) : ICommand<Guid>;
