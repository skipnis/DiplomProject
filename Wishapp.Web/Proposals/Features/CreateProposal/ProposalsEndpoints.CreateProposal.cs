using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Proposals;

public static partial class ProposalsEndpoints
{
    private static async Task<Results<Created<Guid>, NotFound<Error>, BadRequest<Error>, ForbidHttpResult, UnauthorizedHttpResult>> CreateProposal(
        [FromBody] Features.CreateProposal.CreateProposalRequest request,
        ClaimsPrincipal user,
        ICommandHandler<Features.CreateProposal.CreateProposalCommand, Guid> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var command = new Features.CreateProposal.CreateProposalCommand(
            userIdResult.Value,
            request.RecipientId,
            request.SourceType,
            request.CatalogItemId,
            request.WishlistItemId,
            request.CustomTitle,
            request.CustomDescription,
            request.HintMessage,
            request.SenderAlias);

        var result = await handler.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                ErrorType.Validation => TypedResults.BadRequest(result.Error),
                _ => TypedResults.Forbid()
            };
        }

        return TypedResults.Created($"/proposals/{result.Value}", result.Value);
    }
}
