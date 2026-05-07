using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Proposals.Features.ReactToProposal;

public record ReactToProposalCommand(
    Guid ProposalId,
    Guid UserId,
    ProposalStatus Status,
    string? Comment) : ICommand;
