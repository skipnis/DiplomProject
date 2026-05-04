using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions.GetAll;

public sealed class GetAllAchievementDefinitionsHandler(ApplicationDbContext db)
    : IQueryHandler<GetAllAchievementDefinitionsQuery, List<AchievementDefinitionAdminDto>>
{
    public async Task<Result<List<AchievementDefinitionAdminDto>>> HandleAsync(
        GetAllAchievementDefinitionsQuery query,
        CancellationToken ct = default)
    {
        var items = await db.AchievementDefinitions
            .AsNoTracking()
            .OrderBy(a => a.Order)
            .Select(a => new AchievementDefinitionAdminDto(
                a.Id, a.Name, a.Description, a.Emoji,
                a.RuleType, a.LinkedBadgeTypeId, a.Threshold, a.Order, a.IsActive))
            .ToListAsync(ct);

        return items;
    }
}
