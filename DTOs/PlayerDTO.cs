using GameTest.Models;
using System.Text.Json.Serialization;

namespace GameTest.DTOs;

public class PlayerDTO
{
    public string Id { get; }
    public string Name { get; }
    public PlayerColor Color { get; }

    public bool IsBot { get;  }
    public Dictionary<ResourceType, int> Resources { get; } = new()
    {
        { ResourceType.Brick, 0 },
        { ResourceType.Wood, 0 },
        { ResourceType.Ore, 0 },
        { ResourceType.Grain, 0 },
        { ResourceType.Wool, 0 }
    };

    public int ResourceCount { get; } = 0;

    public List<DevelopmentCardType> DevCardsPurchasedThisRound { get; } = new();
    public List<DevelopmentCardType> DevCardsPlayed { get; } = new();
    public List<DevelopmentCardType> DevCardsReadyToPlay { get; } = new();

    public int DevelopmentCardCount { get; } = 0;

    public int VictoryPoints { get; }

    // JsonConstructor parameters must match the JSON property names (case-insensitive).
    [JsonConstructor]
    public PlayerDTO(string id, string name, PlayerColor color, bool isBot = false,
        Dictionary<ResourceType, int>? resources = null, int resourceCount = 0,
        int developmentCardCount = 0, List<DevelopmentCardType>? devCardsPurchasedThisRound = null,
        List<DevelopmentCardType>? devCardsPlayed = null, List<DevelopmentCardType>? devCardsReadyToPlay = null,
        int victoryPoints = 0)
    {
        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        Color = color;
        IsBot = isBot;

        if (resources != null && resources.Count > 0)
            foreach (var kvp in resources)
                Resources[kvp.Key] = kvp.Value;

        ResourceCount = resourceCount;
        DevelopmentCardCount = developmentCardCount;
        DevCardsPurchasedThisRound = devCardsPurchasedThisRound;
        DevCardsPlayed = devCardsPlayed;
        DevCardsReadyToPlay = devCardsReadyToPlay;
        VictoryPoints = victoryPoints;
    }

    public PlayerDTO(Player player, bool countsOnly = true)
    {
        Id = player.Id;
        Name = player.Name;
        Color = player.Color;
        IsBot = player.IsBot;

        if (!countsOnly && player.DevCardsPurchasedThisRound != null)
            foreach (var dc in player.DevCardsPurchasedThisRound)
                DevCardsPurchasedThisRound.Add(dc);

        if (!countsOnly && player.DevCardsReadyToPlay != null)
            foreach (var dc in player.DevCardsReadyToPlay)
                DevCardsReadyToPlay.Add(dc);

        if (player.DevCardsPlayed != null)
            foreach (var dc in player.DevCardsPlayed)
                DevCardsPlayed.Add(dc);

        DevelopmentCardCount = player.DevelopmentCardCount;

        if (!countsOnly && player.Resources != null)
            Resources = new Dictionary<ResourceType, int>(player.Resources);

        ResourceCount = player.ResourceCount;
        VictoryPoints = player.VictoryPoints;
    }
}