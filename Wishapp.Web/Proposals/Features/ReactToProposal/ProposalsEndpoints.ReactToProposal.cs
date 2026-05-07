using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Proposals;

public static partial class ProposalsEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>, Conflict<Error>, ForbidHttpResult, UnauthorizedHttpResult>> ReactToProposal(
        Guid id,
        [FromBody] Features.ReactToProposal.ReactToProposalRequest request,
        ClaimsPrincipal user,
        ICommandHandler<Features.ReactToProposal.ReactToProposalCommand> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var command = new Features.ReactToProposal.ReactToProposalCommand(
            id,
            userIdResult.Value,
            request.Status,
            request.Comment);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                ErrorType.Conflict => TypedResults.Conflict(result.Error),
                _ => TypedResults.Forbid()
            };
        }

        return TypedResults.NoContent();
    }
}
