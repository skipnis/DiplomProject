using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions.SwapOrder;

public record SwapAchievementOrderCommand(int Id, int TargetId) : ICommand;
