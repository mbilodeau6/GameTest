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
    public string Name { get; init;  }
    public PlayerColor Color { get; init; }
    public bool IsBot { get; init; } = false;
    
    public Dictionary<ResourceType, int> Resources { get; } = new()
    {
        { ResourceType.Brick, 0 },
        { ResourceType.Wood, 0 },
        { ResourceType.Ore, 0 },
        { ResourceType.Grain, 0 },
        { ResourceType.Wool, 0 }
    };

    public int ResourceCount { get; set; } = 0;

    public int DevelopmentCardCount { get; set; } = 0;
    public List<DevelopmentCardType> DevCardsPurchasedThisRound { get; private set; } = new List<DevelopmentCardType>();
    public List<DevelopmentCardType> DevCardsPlayed { get; private set; } = new List<DevelopmentCardType>();
    public List<DevelopmentCardType> DevCardsReadyToPlay { get; private set; } = new List<DevelopmentCardType>();

    public int TradeAttemptsThisRound { get; set; } = 0;
    public List<string> AttemptedTradesThisRound { get; private set; } = new List<string>();

    public void ResetTradeAttemptsForRound()
    {
        TradeAttemptsThisRound = 0;
        AttemptedTradesThisRound.Clear();
    }

    public void RecordTradeAttempt(Dictionary<ResourceType, int> offer, Dictionary<ResourceType, int> request)
    {
        TradeAttemptsThisRound++;
        // Create a unique key for this trade to prevent duplicates
        var tradeKey = GetTradeKey(offer, request);
        AttemptedTradesThisRound.Add(tradeKey);
    }

    public bool HasAttemptedTrade(Dictionary<ResourceType, int> offer, Dictionary<ResourceType, int> request)
    {
        var tradeKey = GetTradeKey(offer, request);
        return AttemptedTradesThisRound.Contains(tradeKey);
    }

    private static string GetTradeKey(Dictionary<ResourceType, int> offer, Dictionary<ResourceType, int> request)
    {
        var offerParts = offer.OrderBy(k => k.Key).Select(k => $"{k.Key}:{k.Value}");
        var requestParts = request.OrderBy(k => k.Key).Select(k => $"{k.Key}:{k.Value}");
        return $"O[{string.Join(",", offerParts)}]R[{string.Join(",", requestParts)}]";
    }

    public HashSet<PortType> Ports {get ; private set; } = new HashSet<PortType>();
    public int FullVictoryPoints { get; private set; } = 0; // Includes points from victory dev cards
    public int VisibleVictoryPoints { get; private set; } = 0;

    public static Player CreateTestPlayer(string name, PlayerColor color, bool isBot = false)
    {
        var id = $"P{Interlocked.Increment(ref s_nextId)}";
        return new Player(id, name, color, isBot);
    }

    public Player(string id, string name, PlayerColor color, bool isBot = false)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be empty", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Id = id;
        Name = name;
        Color = color;
        IsBot = isBot;
    }

    public Player(PlayerDTO dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Color = dto.Color;

        DevelopmentCardCount = dto.DevelopmentCardCount;

        if (dto.DevCardsPlayed != null)
            foreach(var dc in dto.DevCardsPlayed)
                DevCardsPlayed.Add(dc);

        if (dto.DevCardsPurchasedThisRound != null)
            foreach(var dc in dto.DevCardsPurchasedThisRound)
                DevCardsPurchasedThisRound.Add(dc);

        if (dto.DevCardsReadyToPlay != null)
            foreach(var dc in dto.DevCardsReadyToPlay)
                DevCardsReadyToPlay.Add(dc);

        ResourceCount = dto.ResourceCount;
        foreach (var kvp in dto.Resources)
            Resources[kvp.Key] = kvp.Value;

        IsBot = dto.IsBot;
        VisibleVictoryPoints = dto.VictoryPoints;

        FullVictoryPoints = dto.FullVictoryPoints ?? 0;
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
        DevCardsPurchasedThisRound.Add(type);
        DevelopmentCardCount++;
    }

    public void MakeNewDevelopmentCardsPlayable()
    {
        foreach(var dc in DevCardsPurchasedThisRound)
            DevCardsReadyToPlay.Add(dc);
        
        DevCardsPurchasedThisRound.Clear();
    }

    public void RetrievePlayedDevelopmentCard(DevelopmentCardType type)
    {
        // TODO: Need to return played dev cards to the bottom of the deck. When I do,
        // need to remove from bottom of deck here.

        DevCardsReadyToPlay.Add(type);
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
        DevelopmentCardCount--;
    }

    public void AddPort(PortType port)
    {
        Ports.Add(port);
    }

    public void SetVictoryPoints(int visibleVictoryPoints, int fullVictoryPoints)
    {
        FullVictoryPoints = fullVictoryPoints;
        VisibleVictoryPoints = visibleVictoryPoints;
    }

    public int CountPlayedKnights()
    {
        return DevCardsPlayed.Count(d => d == DevelopmentCardType.Knight);
    }

    public override string ToString() => $"{Name} ({Id}) - {Color}";
}