using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Collections.SetPublished;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, BadRequest<Error>, NotFound<Error>>> SetCollectionPublished(
        Guid id,
        SetCollectionPublishedRequest request,
        ICommandHandler<SetCollectionPublishedCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new SetCollectionPublishedCommand(id, request.IsPublished), ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.BadRequest(result.Error)
            };
        }

        return TypedResults.NoContent();
    }
}
