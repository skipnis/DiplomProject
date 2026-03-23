using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Categories.Create;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<Created<Guid>, Conflict<Error>>> CreateCategory(
        CreateCategoryCommand command,
        ICommandHandler<CreateCategoryCommand, Guid> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            return TypedResults.Conflict(result.Error);
        }

        return TypedResults.Created($"/catalog/categories/{result.Value}", result.Value);
    }
}
