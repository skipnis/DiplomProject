using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Proposals.Dtos;

namespace Wishapp.Web.Proposals;

public static partial class ProposalsEndpoints
{
    private static async Task<Results<Ok<ProposalDetailDto>, NotFound<Error>, ForbidHttpResult, UnauthorizedHttpResult>> GetProposalDetail(
        Guid id,
        ClaimsPrincipal user,
        ICommandHandler<Features.GetProposalDetail.GetProposalDetailCommand, ProposalDetailDto> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new Features.GetProposalDetail.GetProposalDetailCommand(id, userIdResult.Value), ct);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.Forbid()
            };
        }

        return TypedResults.Ok(result.Value);
    }
}
