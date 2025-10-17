using GameTest.Models;
using System.Text.Json.Serialization;

namespace GameTest.DTOs;

public class PlayerDTO
{
    public string Id { get; }
    public string Name { get; }
    public string Color { get; }
    public Dictionary<ResourceType, int> Resources { get; } = new()
    {
        { ResourceType.Brick, 0 },
        { ResourceType.Wood, 0 },
        { ResourceType.Ore, 0 },
        { ResourceType.Grain, 0 },
        { ResourceType.Wool, 0 }
    };

    public int ResourceCount { get; } = 0;

    public Dictionary<DevelopmentCardType, int> DevelopmentCards { get; } = new()
    {
        { DevelopmentCardType.RoadBuilding, 0 },
        { DevelopmentCardType.VictoryPoint, 0 },
        { DevelopmentCardType.Monopoly, 0 },
        { DevelopmentCardType.YearOfPlenty, 0 },
        { DevelopmentCardType.Knight, 0 }
    };

    public int DevelopmentCardCount { get; } = 0;


    // JsonConstructor parameters must match the JSON property names (case-insensitive).
    [JsonConstructor]
    public PlayerDTO(string id, string name, string color)
    {
        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        Color = color ?? string.Empty;
    }

    public PlayerDTO(Player player, bool? countsOnly = true)
    {
        Id = player.Id;
        Name = player.Name;
        Color = player.Color.ToString();

        if (countsOnly ?? true || player.DevelopmentCards != null)
            DevelopmentCards = new Dictionary<DevelopmentCardType, int>(player.DevelopmentCards);

        DevelopmentCardCount = player.DevelopmentCardCount;

        if (countsOnly ?? true || player.Resources != null)
            Resources = new Dictionary<ResourceType, int>(player.Resources);

        ResourceCount = player.ResourceCount;
    }
}