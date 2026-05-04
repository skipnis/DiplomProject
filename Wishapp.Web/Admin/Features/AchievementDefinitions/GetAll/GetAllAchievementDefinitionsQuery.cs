using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions.GetAll;

public record GetAllAchievementDefinitionsQuery : IQuery<List<AchievementDefinitionAdminDto>>;
