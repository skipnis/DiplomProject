using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions;
using Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.Create;
using Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.Delete;
using Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.GetAll;
using Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.Update;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Features.GetFulfilledBadgeDefinitions;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Ok<List<FulfilledBadgeDefinitionDto>>> GetAllFulfilledBadgeDefinitions(
        IQueryHandler<GetAllFulfilledBadgeDefinitionsQuery, List<FulfilledBadgeDefinitionDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetAllFulfilledBadgeDefinitionsQuery(), ct);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Created<int>> CreateFulfilledBadgeDefinition(
        [FromBody] FulfilledBadgeDefinitionRequest request,
        ICommandHandler<CreateFulfilledBadgeDefinitionCommand, int> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new CreateFulfilledBadgeDefinitionCommand(request.Emoji, request.Slug, request.Label, request.Description), ct);

        return TypedResults.Created($"/admin/catalog/fulfilled-badge-definitions/{result.Value}", result.Value);
    }

    private static async Task<Results<Ok, NotFound<Error>>> UpdateFulfilledBadgeDefinition(
        [FromRoute] int id,
        [FromBody] FulfilledBadgeDefinitionRequest request,
        ICommandHandler<UpdateFulfilledBadgeDefinitionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new UpdateFulfilledBadgeDefinitionCommand(id, request.Emoji, request.Slug, request.Label, request.Description, request.IsActive), ct);

        if (!result.IsSuccess)
            return TypedResults.NotFound(result.Error);

        return TypedResults.Ok();
    }

    private static async Task<Results<NoContent, NotFound<Error>>> DeleteFulfilledBadgeDefinition(
        [FromRoute] int id,
        ICommandHandler<DeleteFulfilledBadgeDefinitionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new DeleteFulfilledBadgeDefinitionCommand(id), ct);

        if (!result.IsSuccess)
            return TypedResults.NotFound(result.Error);

        return TypedResults.NoContent();
    }
}
