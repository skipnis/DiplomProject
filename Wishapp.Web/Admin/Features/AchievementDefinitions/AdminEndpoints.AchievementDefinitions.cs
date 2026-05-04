using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.AchievementDefinitions;
using Wishapp.Web.Admin.Features.AchievementDefinitions.Create;
using Wishapp.Web.Admin.Features.AchievementDefinitions.Delete;
using Wishapp.Web.Admin.Features.AchievementDefinitions.GetAll;
using Wishapp.Web.Admin.Features.AchievementDefinitions.Update;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Ok<List<AchievementDefinitionAdminDto>>> GetAllAchievementDefinitions(
        IQueryHandler<GetAllAchievementDefinitionsQuery, List<AchievementDefinitionAdminDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetAllAchievementDefinitionsQuery(), ct);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Created<int>> CreateAchievementDefinition(
        [FromBody] AchievementDefinitionRequest request,
        ICommandHandler<CreateAchievementDefinitionCommand, int> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new CreateAchievementDefinitionCommand(
                request.Name, request.Description, request.Emoji,
                request.RuleType, request.LinkedBadgeTypeId,
                request.Threshold, request.Order), ct);

        return TypedResults.Created($"/admin/catalog/achievements/{result.Value}", result.Value);
    }

    private static async Task<Results<Ok, NotFound<Error>>> UpdateAchievementDefinition(
        [FromRoute] int id,
        [FromBody] AchievementDefinitionRequest request,
        ICommandHandler<UpdateAchievementDefinitionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new UpdateAchievementDefinitionCommand(
                id, request.Name, request.Description, request.Emoji,
                request.RuleType, request.LinkedBadgeTypeId,
                request.Threshold, request.Order, request.IsActive), ct);

        if (!result.IsSuccess)
            return TypedResults.NotFound(result.Error);

        return TypedResults.Ok();
    }

    private static async Task<Results<NoContent, NotFound<Error>>> DeleteAchievementDefinition(
        [FromRoute] int id,
        ICommandHandler<DeleteAchievementDefinitionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new DeleteAchievementDefinitionCommand(id), ct);

        if (!result.IsSuccess)
            return TypedResults.NotFound(result.Error);

        return TypedResults.NoContent();
    }
}
