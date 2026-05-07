using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Proposals.Dtos;

namespace Wishapp.Web.Proposals.Features.GetIncoming;

public record GetIncomingProposalsQuery(Guid UserId, PagedRequest Request) : IQuery<PagedResponse<IncomingProposalDto>>;
