using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Proposals.Features.ReactToProposal;

public record ReactToProposalRequest(ProposalStatus Status, string? Comment);
