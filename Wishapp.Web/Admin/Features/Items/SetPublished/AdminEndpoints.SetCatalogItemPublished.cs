using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Items.SetPublished;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, BadRequest<Error>, NotFound<Error>>> SetCatalogItemPublished(
        Guid id,
        SetCatalogItemPublishedRequest request,
        ICommandHandler<SetCatalogItemPublishedCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new SetCatalogItemPublishedCommand(id, request.IsPublished), ct);

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
