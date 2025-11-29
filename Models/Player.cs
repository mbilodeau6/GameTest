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
    public bool IsBot { get; }
    
    public Dictionary<ResourceType, int> Resources { get; } = new()
    {
        { ResourceType.Brick, 0 },
        { ResourceType.Wood, 0 },
        { ResourceType.Ore, 0 },
        { ResourceType.Grain, 0 },
        { ResourceType.Wool, 0 }
    };

    public int ResourceCount { get; set; } = 0;

    // TODO: Remove this after new DevCard objects working
    public Dictionary<DevelopmentCardType, int> DevelopmentCards { get; } = new()
    {
        { DevelopmentCardType.RoadBuilding, 0 },
        { DevelopmentCardType.VictoryPoint, 0 },
        { DevelopmentCardType.Monopoly, 0 },
        { DevelopmentCardType.YearOfPlenty, 0 },
        { DevelopmentCardType.Knight, 0 }
    };

    public int DevelopmentCardCount { get; set; } = 0;
    public List<DevelopmentCardType> DevCardsPurchasedThisRound { get; private set; } = new List<DevelopmentCardType>();
    public List<DevelopmentCardType> DevCardsPlayed { get; private set; } = new List<DevelopmentCardType>();
    public List<DevelopmentCardType> DevCardsReadyToPlay { get; private set; } = new List<DevelopmentCardType>();

    public HashSet<PortType> Ports {get ; private set; } = new HashSet<PortType>();

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
        // TODO: Need to remove when switch to new dev card implementation
        DevelopmentCards[type]++;

        DevCardsPurchasedThisRound.Add(type);
        DevelopmentCardCount++;
    }

    public void MakeNewDevelopmentCardsPlayable()
    {
        foreach(var dc in DevCardsPurchasedThisRound)
            DevCardsReadyToPlay.Add(dc);
        
        DevCardsPurchasedThisRound.Clear();
    }

    public void PlayDevelopmentCard(DevelopmentCardType type)
    {
        if (!DevCardsReadyToPlay.Contains(type))
            throw new InvalidOperationException($"Unexpected Exception. Development Card type {type} not in DevCardsReadyToPlay.");
        else if (type == DevelopmentCardType.VictoryPoint)
            throw new InvalidOperationException("Unexpected Exception. You don't play Victory Points.");
        else if (type == DevelopmentCardType.Knight)
            DevCardsPlayed.Add(DevelopmentCardType.Knight);

        DevCardsReadyToPlay.Remove(type);
    }

    public void AddPort(PortType port)
    {
        Ports.Add(port);
    }

    public override string ToString() => $"{Name} ({Id}) - {Color}";
}