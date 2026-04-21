using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Items.BatchImport;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<Ok<List<BatchImportItemResult>>, BadRequest<Error>, NotFound<Error>>> BatchImportCatalogItems(
        BatchImportCatalogItemsCommand command,
        ICommandHandler<BatchImportCatalogItemsCommand, List<BatchImportItemResult>> handler,
        CancellationToken ct)
    {
        if (command.Urls is not { Count: > 0 })
            return TypedResults.BadRequest(Error.Validation("Urls.Empty", "At least one URL must be provided"));

        if (command.Urls.Count > 50)
            return TypedResults.BadRequest(Error.Validation("Urls.TooMany", "Maximum 50 URLs per batch"));

        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type == ErrorType.NotFound
                ? TypedResults.NotFound(result.Error)
                : TypedResults.BadRequest(result.Error);
        }

        return TypedResults.Ok(result.Value);
    }
}
