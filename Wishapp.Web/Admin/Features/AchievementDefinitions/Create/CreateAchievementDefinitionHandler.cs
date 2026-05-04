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
        var definition = AchievementDefinition.Create(
            command.Name, command.Description, command.Emoji,
            command.RuleType, command.LinkedBadgeTypeId,
            command.Threshold, command.Order);

        db.AchievementDefinitions.Add(definition);
        await db.SaveChangesAsync(ct);
        return definition.Id;
    }
}
