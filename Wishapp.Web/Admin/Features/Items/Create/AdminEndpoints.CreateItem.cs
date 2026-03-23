using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Items.Create;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<Created<Guid>, NotFound<Error>>> CreateItem(
        CreateCatalogItemCommand command,
        ICommandHandler<CreateCatalogItemCommand, Guid> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            return TypedResults.NotFound(result.Error);
        }

        return TypedResults.Created($"/catalog/items/{result.Value}", result.Value);
    }
}
