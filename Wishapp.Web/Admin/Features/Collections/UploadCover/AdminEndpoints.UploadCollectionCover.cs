using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Collections.UploadCover;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<Ok<UploadCollectionCoverResponse>, BadRequest<Error>, NotFound<Error>>> UploadCollectionCover(
        Guid id,
        IFormFile file,
        ICommandHandler<UploadCollectionCoverCommand, UploadCollectionCoverResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new UploadCollectionCoverCommand(id, file), ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.BadRequest(result.Error)
            };
        }

        return TypedResults.Ok(result.Value);
    }
}
