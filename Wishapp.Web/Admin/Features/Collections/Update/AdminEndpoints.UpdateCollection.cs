using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Collections.Update;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>>> UpdateCollection(
        [FromRoute] Guid id,
        UpdateCollectionRequest request,
        ICommandHandler<UpdateCollectionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new UpdateCollectionCommand(
                id,
                request.Name,
                request.Description,
                request.OccasionId,
                request.CoverImagePath,
                request.IsPublished), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
