using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Proposals.Dtos;

namespace Wishapp.Web.Proposals.Features.GetOutgoing;

public record GetOutgoingProposalsQuery(Guid UserId, PagedRequest Request) : IQuery<PagedResponse<OutgoingProposalDto>>;
