using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;
using Wishapp.Web.Proposals.Features.UploadProposalImage;

namespace Wishapp.Web.Proposals;

public static partial class ProposalsEndpoints
{
    private static async Task<Results<Ok<UploadProposalImageResponse>, NotFound<Error>, BadRequest<Error>, ForbidHttpResult, UnauthorizedHttpResult>> UploadImage(
        Guid id,
        [FromForm] UploadProposalImageRequest request,
        ClaimsPrincipal user,
        ICommandHandler<UploadProposalImageCommand, UploadProposalImageResponse> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();

        if (userIdResult.IsFailure)
            return TypedResults.Unauthorized();

        var result = await handler.HandleAsync(
            new UploadProposalImageCommand(id, userIdResult.Value, request.File), ct);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                ErrorType.Forbidden => TypedResults.Forbid(),
                _ => TypedResults.BadRequest(result.Error)
            };
        }

        return TypedResults.Ok(result.Value);
    }
}
