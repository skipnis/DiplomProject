using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions.Delete;

public record DeleteAchievementDefinitionCommand(int Id) : ICommand;
