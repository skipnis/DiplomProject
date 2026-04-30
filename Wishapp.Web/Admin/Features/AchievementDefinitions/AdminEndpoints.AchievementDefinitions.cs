using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Admin.Features.AchievementDefinitions;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Admin;

public record AchievementDefinitionAdminDto(
    int Id,
    string Name,
    string Description,
    string Emoji,
    AchievementRuleType RuleType,
    int? LinkedBadgeTypeId,
    int Threshold,
    int Order,
    bool IsActive);

public static partial class AdminEndpoints
{
    private static async Task<Ok<List<AchievementDefinitionAdminDto>>> GetAllAchievementDefinitions(
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var items = await db.AchievementDefinitions
            .AsNoTracking()
            .OrderBy(a => a.Order)
            .Select(a => new AchievementDefinitionAdminDto(
                a.Id, a.Name, a.Description, a.Emoji,
                a.RuleType, a.LinkedBadgeTypeId, a.Threshold, a.Order, a.IsActive))
            .ToListAsync(ct);

        return TypedResults.Ok(items);
    }

    private static async Task<Created<int>> CreateAchievementDefinition(
        [FromBody] AchievementDefinitionRequest request,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var definition = AchievementDefinition.Create(
            request.Name, request.Description, request.Emoji,
            request.RuleType, request.LinkedBadgeTypeId,
            request.Threshold, request.Order);

        db.AchievementDefinitions.Add(definition);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/admin/catalog/achievements/{definition.Id}", definition.Id);
    }

    private static async Task<Results<Ok, NotFound>> UpdateAchievementDefinition(
        [FromRoute] int id,
        [FromBody] AchievementDefinitionRequest request,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var definition = await db.AchievementDefinitions.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (definition is null)
            return TypedResults.NotFound();

        definition.Update(
            request.Name, request.Description, request.Emoji,
            request.RuleType, request.LinkedBadgeTypeId,
            request.Threshold, request.Order, request.IsActive);

        await db.SaveChangesAsync(ct);
        return TypedResults.Ok();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAchievementDefinition(
        [FromRoute] int id,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var definition = await db.AchievementDefinitions.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (definition is null)
            return TypedResults.NotFound();

        db.AchievementDefinitions.Remove(definition);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
