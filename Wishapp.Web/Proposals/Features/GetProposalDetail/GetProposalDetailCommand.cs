using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Proposals.Dtos;

namespace Wishapp.Web.Proposals.Features.GetProposalDetail;

public record GetProposalDetailCommand(Guid ProposalId, Guid UserId) : ICommand<ProposalDetailDto>;
