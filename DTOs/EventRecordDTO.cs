using System.Text.Json.Serialization;
using GameTest.Models;

public class EventRecordDTO
{
    public int Id { get; set; }
    public string PlayerId { get; init; }
    public EventRecordAction Action { get; private set; }
    public string? VertexId { get; private set; }
    public string? EdgeId {get; private set; }
    public Dictionary<ResourceType, int>? ResourcesUsed {get; private set; }
    public Dictionary<ResourceType, int>? ResourcesReceived {get; private set; }
    public int? DiceRoll {get; private set; }
    public DevelopmentCardType? DevelopmentCard { get; private set; }
    public string? TargetPlayerId { get; private set; }
    public string? TileId { get; private set; }
    public int? EventReversed { get; private set; } // for undo

    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public EventRecordDTO(int id, string playerId, EventRecordAction action,
            string? vertexId, string? edgeId,
            Dictionary<ResourceType, int>? resourcesUsed, Dictionary<ResourceType, int>? resourcesReceived,
            int? diceRoll, DevelopmentCardType? developmentCard, string? targetPlayerId, string? tileId,
            int? eventReversed)
    {
        Id = id;
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
        EventReversed = eventReversed;
    }

    public EventRecordDTO(Player player, EventRecordAction action)
    {
        PlayerId = player.Id;
        Action = action;
    }

    public EventRecordDTO(Player player, EventRecordAction action, GameDice dice) : this(player, action)
    {
        if (action != EventRecordAction.RollDice)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for RollDice.");

        DiceRoll = dice.Die1.Value + dice.Die2.Value;
    }

    public EventRecordDTO(Player player, EventRecordAction action, Vertex vertex) : this(player,action)
    {
        if (action != EventRecordAction.PlaceSettlement && action != EventRecordAction.PlaceFirstSettlement && 
                action != EventRecordAction.UpgradeSettlement)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for PlaceSettlement, PlaceFirstSettlement, or UpgradeSettlement.");

        VertexId = vertex.Id;
    }

    public EventRecordDTO(Player player, EventRecordAction action, Vertex vertex, Dictionary<ResourceType, int> resourcesGained) : this(player,action)
    {
        if (action != EventRecordAction.PlaceSecondSettlement)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for PlaceSecondSettlement.");

        VertexId = vertex.Id;

        ResourcesReceived = new();
        foreach (var resource in resourcesGained)
            ResourcesReceived.Add(resource.Key, resource.Value);
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
        if (action != EventRecordAction.DiscardCards)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for DiscardCards.");

        ResourcesUsed = new Dictionary<ResourceType, int>();

        foreach (var resource in resourcesUsed)
            if (!ResourcesUsed.ContainsKey(resource))
                ResourcesUsed.Add(resource, 1);    
            else
                ResourcesUsed[resource]++;
    }

    public EventRecordDTO(Player player, EventRecordAction action, Player targetPlayer, ResourceType resourceGained) : this(player,action)
    {
        if (action != EventRecordAction.SelectTarget && action != EventRecordAction.StealResource)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for SelectTarget or StealResource.");

        TargetPlayerId = targetPlayer.Id;

        ResourcesReceived = new Dictionary<ResourceType, int>() { {resourceGained, 1 } };
    }

    public EventRecordDTO(Player player, EventRecordAction action, Dictionary<ResourceType, int> resourcesGained) : this(player,action)
    {
        if (action != EventRecordAction.PlayMonopoly && action != EventRecordAction.PlayYearOfPlenty && action != EventRecordAction.ReceivedResources)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for PlayMonopoly, PlayYearOfPlenty or ReceivedResources.");

        ResourcesReceived = new Dictionary<ResourceType, int>();

        foreach (var resource in resourcesGained)
            ResourcesReceived.Add(resource.Key, resource.Value);
    }

    public EventRecordDTO(Player player, EventRecordAction action, DevelopmentCardType devCard) : this(player,action)
    {
        if (action != EventRecordAction.BuyDevelopmentCard)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for BuyDevelopmentCard.");

        DevelopmentCard = devCard;
    }

    public EventRecordDTO(Player player, EventRecordAction action, Dictionary<ResourceType, int> resourcesGained, Dictionary<ResourceType, int> resourcesUsed, Player? targetPlayer = null) : this(player,action)
    {
        if (action != EventRecordAction.TradeWithPlayer && action != EventRecordAction.OfferToTrade && action != EventRecordAction.CounterOffer)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for TradeWithPlayer, OfferToTrade, or CounterOffer.");

        if (targetPlayer != null)
            TargetPlayerId = targetPlayer.Id;

        ResourcesReceived = new Dictionary<ResourceType, int>();

        foreach (var resource in resourcesGained)
            ResourcesReceived.Add(resource.Key, resource.Value);    

        ResourcesUsed = new Dictionary<ResourceType, int>();

        foreach (var resource in resourcesUsed)
            ResourcesUsed.Add(resource.Key, resource.Value);    
    }

    public EventRecordDTO(Player player, EventRecordAction action, Dictionary<ResourceType, int>resourcesGained, Dictionary<ResourceType, int> resourcesUsed) : this(player,action)
    {
        if (action != EventRecordAction.TradeWithBank)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for TradeWithBank.");

        ResourcesReceived = new Dictionary<ResourceType, int>();

        foreach (var resource in resourcesGained)
            ResourcesReceived.Add(resource.Key, resource.Value);    

        ResourcesUsed = new Dictionary<ResourceType, int>();

        foreach (var resource in resourcesUsed)
            ResourcesUsed.Add(resource.Key, resource.Value);    
    }

    public EventRecordDTO(Player player, EventRecordAction action, int eventReversed) : this(player,action)
    {
        if (action != EventRecordAction.Undo)
            throw new InvalidOperationException("Unexpected Exception. Should only be used for Undo.");

        EventReversed = eventReversed;
    }
}
