using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Collections.UpdateItemDescription;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>>> UpdateCollectionItemDescription(
        [FromRoute] Guid id,
        [FromRoute] Guid itemId,
        [FromBody] UpdateCollectionItemDescriptionRequest request,
        ICommandHandler<UpdateCollectionItemDescriptionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new UpdateCollectionItemDescriptionCommand(id, itemId, request.Description), ct);

        if (!result.IsSuccess)
            return TypedResults.NotFound(result.Error);

        return TypedResults.NoContent();
    }
}
