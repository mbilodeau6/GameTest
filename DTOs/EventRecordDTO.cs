using System.Text.Json.Serialization;
using GameTest.Models;

public class EventRecordDTO
{
    public string PlayerId { get; init; }
    public string Action { get; private set; }
    public string? VertexId { get; private set; }
    public string? EdgeId {get; private set; }
    public Dictionary<string, int>? ResourcesUsed {get; private set; }
    public Dictionary<string, int>? ResourcesReceived {get; private set; }
    public int? DiceRoll {get; private set; }
    public string? DevelopmentCard { get; private set; }
    public string? TargetPlayerId { get; private set; }
    public string? TileId { get; private set; }

    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public EventRecordDTO(string playerId, string action, 
            string? vertexId, string? edgeId,
            Dictionary<string, int>? resourcesUsed, Dictionary<string, int>? resourcesReceived, 
            int? diceRoll, string? developmentCard, string? targetPlayerId, string? tileId)
    {
        PlayerId = playerId;
        Action = action;
        VertexId = vertexId;
        EdgeId = edgeId;
        DiceRoll = diceRoll;
        DevelopmentCard = developmentCard;
        TargetPlayerId = targetPlayerId;
        TileId = tileId;
        ResourcesUsed = resourcesUsed;
        ResourcesReceived = resourcesReceived;

    }

    public EventRecordDTO(Player player, EventRecordAction action)
    {
        PlayerId = player.Id;
        Action = action.ToString();
    }

    public EventRecordDTO(Player player, EventRecordAction action, GameDice dice) : this(player, action)
    {
        if (action != EventRecordAction.RollDice)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for RollDice.");

        DiceRoll = dice.Die1.Value + dice.Die2.Value;
    }

    public EventRecordDTO(Player player, EventRecordAction action, Vertex vertex) : this(player,action)
    {
        if (action != EventRecordAction.PlaceSettlement && action != EventRecordAction.UpgradeSettlement)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for PlaceSettlement or UpgradeSettlement.");

        VertexId = vertex.Id;
    }

    public EventRecordDTO(Player player, EventRecordAction action, Edge edge) : this(player,action)
    {
        if (action != EventRecordAction.PlaceRoad)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for PlaceRoad.");

        EdgeId = edge.Id;
    }

    public EventRecordDTO(Player player, EventRecordAction action, Tile tile) : this(player,action)
    {
        if (action != EventRecordAction.PlaceRobber && action != EventRecordAction.PlayKnight)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for PlaceRobber or PlaceKnight.");

        TileId = tile.Id;
    }


    public EventRecordDTO(Player player, EventRecordAction action, List<ResourceType> resourcesUsed) : this(player,action)
    {
        if (action != EventRecordAction.SevenDiscard)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for SevenDiscard.");

        ResourcesUsed = new Dictionary<string, int>();

        foreach (var resource in resourcesUsed)
            if (!ResourcesUsed.ContainsKey(resource.ToString()))
                ResourcesUsed.Add(resource.ToString(), 1);    
            else
                ResourcesUsed[resource.ToString()]++;
    }

    public EventRecordDTO(Player player, EventRecordAction action, Player targetPlayer, ResourceType resourceGained) : this(player,action)
    {
        if (action != EventRecordAction.SelectRobberTarget && action != EventRecordAction.StealResource)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for SelectRobberTarget or StealResource.");

        TargetPlayerId = targetPlayer.Id;

        ResourcesReceived = new Dictionary<string, int>() { {resourceGained.ToString(), 1 } };
    }

    public EventRecordDTO(Player player, EventRecordAction action, Dictionary<ResourceType, int> resourcesGained) : this(player,action)
    {
        if (action != EventRecordAction.PlayMonoploy && action != EventRecordAction.PlayYearOfPlenty)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for PlayMonopoly or PlayYearOfPlenty.");

        ResourcesReceived = new Dictionary<string, int>();

        foreach (var resource in resourcesGained)
            ResourcesReceived.Add(resource.Key.ToString(), resource.Value);
    }

    public EventRecordDTO(Player player, EventRecordAction action, DevelopmentCardType devCard) : this(player,action)
    {
        if (action != EventRecordAction.BuyDevelopmentCard)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for BuyDevelopmentCard.");

        DevelopmentCard = devCard.ToString();
    }

    public EventRecordDTO(Player player, EventRecordAction action, Player targetPlayer, Dictionary<ResourceType, int> resourcesGained, Dictionary<ResourceType, int> resourcesUsed) : this(player,action)
    {
        if (action != EventRecordAction.TradeWithPlayer)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for TradeWithPlayer.");

        TargetPlayerId = targetPlayer.Id;

        ResourcesReceived = new Dictionary<string, int>();

        foreach (var resource in resourcesGained)
            ResourcesReceived.Add(resource.Key.ToString(), resource.Value);    

        ResourcesUsed = new Dictionary<string, int>();

        foreach (var resource in resourcesUsed)
            ResourcesUsed.Add(resource.Key.ToString(), resource.Value);    
    }

    public EventRecordDTO(Player player, EventRecordAction action, Dictionary<ResourceType, int>resourcesGained, Dictionary<ResourceType, int> resourcesUsed) : this(player,action)
    {
        if (action != EventRecordAction.TradeWithBank)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for TradeWithBank.");

        ResourcesReceived = new Dictionary<string, int>();

        foreach (var resource in resourcesGained)
            ResourcesReceived.Add(resource.Key.ToString(), resource.Value);    

        ResourcesUsed = new Dictionary<string, int>();

        foreach (var resource in resourcesUsed)
            ResourcesUsed.Add(resource.Key.ToString(), resource.Value);    

    }
}
