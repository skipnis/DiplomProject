namespace Wishapp.Web.Gamification;

public sealed class GiftLevelOptions
{
    public List<GiftLevelDefinition> Levels { get; init; } = [];

    public (int Level, string LevelName, int NextLevelThreshold) Calculate(int giftsGiven)
    {
        for (var i = Levels.Count - 1; i >= 0; i--)
        {
            if (giftsGiven >= Levels[i].Threshold)
            {
                var nextThreshold = i + 1 < Levels.Count ? Levels[i + 1].Threshold : int.MaxValue;
                return (i + 1, Levels[i].Name, nextThreshold);
            }
        }
        return (1, Levels.Count > 0 ? Levels[0].Name : string.Empty, Levels.Count > 1 ? Levels[1].Threshold : int.MaxValue);
    }
}

public sealed class GiftLevelDefinition
{
    public string Name { get; init; } = string.Empty;
    public int Threshold { get; init; }
}
