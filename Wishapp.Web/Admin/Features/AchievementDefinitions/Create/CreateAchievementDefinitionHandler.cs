using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions.Create;

public sealed class CreateAchievementDefinitionHandler(ApplicationDbContext db)
    : ICommandHandler<CreateAchievementDefinitionCommand, int>
{
    public async Task<Result<int>> HandleAsync(
        CreateAchievementDefinitionCommand command,
        CancellationToken ct = default)
    {
        var nextOrder = await db.AchievementDefinitions.AnyAsync(ct)
            ? await db.AchievementDefinitions.MaxAsync(a => a.Order, ct) + 1
            : 1;

        var definition = AchievementDefinition.Create(
            command.Name, command.Description, command.Emoji,
            command.RuleType, command.LinkedBadgeTypeId,
            command.Threshold, nextOrder);

        db.AchievementDefinitions.Add(definition);
        await db.SaveChangesAsync(ct);
        return definition.Id;
    }
}
