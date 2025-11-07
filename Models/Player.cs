using System;
using GameTest.DTOs;

namespace GameTest.Models;

public enum PlayerColor
{
    Red,
    Blue,
    White,
    Orange,
    Brown,
    Green,
    Yellow,
    Purple
}

public class Player
{
    private static int s_nextId = 0;

    public string Id { get; init; }
    public string Name { get; set;  }
    public PlayerColor Color { get; set; }
    public bool IsBot { get;  }

    public Dictionary<ResourceType, int> Resources { get; } = new()
    {
        { ResourceType.Brick, 0 },
        { ResourceType.Wood, 0 },
        { ResourceType.Ore, 0 },
        { ResourceType.Grain, 0 },
        { ResourceType.Wool, 0 }
    };

    public int ResourceCount { get; set; } = 0;

    public Dictionary<DevelopmentCardType, int> DevelopmentCards { get; } = new()
    {
        { DevelopmentCardType.RoadBuilding, 0 },
        { DevelopmentCardType.VictoryPoint, 0 },
        { DevelopmentCardType.Monopoly, 0 },
        { DevelopmentCardType.YearOfPlenty, 0 },
        { DevelopmentCardType.Knight, 0 }
    };

    public int DevelopmentCardCount { get; set; } = 0;

    // Parameterless ctor for serializers
    public Player()
    {
        Id = $"P{Interlocked.Increment(ref s_nextId)}";

        // TODO: Need to assign a unique name
        Name = string.Empty;

        // TODO: Need to assign a color that is not already taken
        Color = PlayerColor.Red;
    }

    public Player(string name, PlayerColor color, bool isBot = false)
    {
        Id = $"P{Interlocked.Increment(ref s_nextId)}";

        // TODO: Need to ensure name and color are unique
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
        Color = color;
        IsBot = isBot;
    }

    public Player(PlayerDTO dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Color = Enum.Parse<PlayerColor>(dto.Color);

        DevelopmentCardCount = dto.DevelopmentCardCount;
        foreach (var kvp in dto.DevelopmentCards)
            DevelopmentCards[kvp.Key] = kvp.Value;

        ResourceCount = dto.ResourceCount;
        foreach (var kvp in dto.Resources)
            Resources[kvp.Key] = kvp.Value;

        IsBot = dto.IsBot;
    }

    public void AssignResources(ResourceType type, int count)
    {
        if (type == ResourceType.Desert)
            throw new ArgumentException("Desert is not a resource that can be earned/owned.");

        Resources[type] += count;
        ResourceCount += count;

    }

    public void RemoveResources(ResourceType type, int count)
    {
        if (!Resources.ContainsKey(type) || Resources[type] < count)
            throw new ArgumentException($"Player doesn't have {count} {type.ToString()}.");

        Resources[type] -= count;
        ResourceCount -= count;
    }

    public void AssignDevelopmentCard(DevelopmentCardType type)
    {
        DevelopmentCards[type]++;
        DevelopmentCardCount++;
    }

    public override string ToString() => $"{Name} ({Id}) - {Color}";
}