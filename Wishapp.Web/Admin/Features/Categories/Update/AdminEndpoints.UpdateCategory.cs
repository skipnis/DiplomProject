using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Categories.Update;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>>> UpdateCategory(
        [FromRoute] Guid id,
        UpdateCategoryRequest request,
        ICommandHandler<UpdateCategoryCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new UpdateCategoryCommand(id, request.Name, request.Order), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
