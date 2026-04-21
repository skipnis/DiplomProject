using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Categories.SetPublished;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, BadRequest<Error>, NotFound<Error>>> SetCategoryPublished(
        Guid id,
        SetCategoryPublishedRequest request,
        ICommandHandler<SetCategoryPublishedCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new SetCategoryPublishedCommand(id, request.IsPublished), ct);

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
