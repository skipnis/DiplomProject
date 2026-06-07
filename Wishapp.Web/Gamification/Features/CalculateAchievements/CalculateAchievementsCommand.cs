using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Gamification.Features.CalculateAchievements;

public sealed record CalculateAchievementsCommand(Guid UserId) : ICommand;
