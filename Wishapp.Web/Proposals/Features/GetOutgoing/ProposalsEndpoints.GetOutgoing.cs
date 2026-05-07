using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Proposals.Dtos;

namespace Wishapp.Web.Proposals;

public static partial class ProposalsEndpoints
{
    private static async Task<Results<Ok<PagedResponse<OutgoingProposalDto>>, UnauthorizedHttpResult>> GetOutgoing(
        [AsParameters] PagedRequest request,
        ClaimsPrincipal user,
        [FromServices] IQueryHandler<Features.GetOutgoing.GetOutgoingProposalsQuery, PagedResponse<OutgoingProposalDto>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new Features.GetOutgoing.GetOutgoingProposalsQuery(userIdResult.Value, request), ct);

        return TypedResults.Ok(result.Value);
    }
}
