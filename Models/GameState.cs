using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using GameTest.DTOs;
using GameTest.Services;

namespace GameTest.Models;

public class GameState
{
    public Guid Id { get; init; }
    public GameSettings Settings { get; private set; }
    public List<Player> Players { get; } = new();
    public List<Tile> Tiles { get; } = new();
    public List<Edge> Edges { get; } = new();
    public List<Vertex> Vertices { get; } = new();
    public Tile RobberTile { get; private set; } = null!;
    public Player? PlayerWithLongestRoad { get; private set; } = null!;
    public Player? PlayerWithLargestArmy { get; private set; } = null!;
    public GameDice Dice { get; private set; } = new GameDice(true);
    public List<EventRecordDTO> EventRecord { get; private set; } = new List<EventRecordDTO>();
    public Stack<PreActionState> UndoState { get; private set; } = new Stack<PreActionState>();
    public int NextEventId { get; private set; } = 0;
    private int NextPlayerId { get; set; } = 1;
    private Bank Bank { get; set; } = new Bank();
    public List<DevelopmentCardType> DevelopmentCards { get; private set; } = new List<DevelopmentCardType>();
    public GamePhase Phase { get; set; }
    public List<Port> Ports {get; } = new();

    private void InitializeDevelopmentCards()
    {
        List<DevelopmentCardType> developmentCards = new List<DevelopmentCardType>();

        for (int i = 0; i < 2; i++)
        {
            developmentCards.Add(DevelopmentCardType.Monopoly);
            developmentCards.Add(DevelopmentCardType.RoadBuilding);
            developmentCards.Add(DevelopmentCardType.YearOfPlenty);
        }

        for (int i = 0; i < 14; i++)
            developmentCards.Add(DevelopmentCardType.Knight);

        for (int i = 0; i < 5; i++)
            developmentCards.Add(DevelopmentCardType.VictoryPoint);

        // Shuffle the development cards
        var rnd = new Random();
        DevelopmentCards = developmentCards.OrderBy(x => rnd.Next()).ToList();
    }

    public GameState(Guid guid, string creator, GameType type = GameType.Default)
    {
        Id = guid;
        Settings = type == GameType.Test ? new GameSettings(type, creator, 2, 5, 6, 3, 2) : new GameSettings(type, creator);

        Phase = new GamePhase(GameStates.SettingUpBoard, Settings.VictoryPointsToWin);

        if (type == GameType.Test)
            Dice = new GameDice(false);

        InitializeDevelopmentCards();
    }

    // ATTENTION: Right now I'm using PlayerId's that are simply the letter "P"
    // with a unique (to the single game instance) integer. This is easier to use
    // for debugging and testing. We may not need anything better as I plan to 
    // switch to a PlayerToken to identify players outside of individual games.
    private int GetIntPortionOfPlayerId(string playerId)
    {
        var cleanedId = playerId.Trim().ToUpper();
        
        if (!cleanedId.StartsWith("P"))
            throw new ArgumentException("Unexpected Error. All PlayerIds are assumed to start with a P.");

        var withoutLeadingP = cleanedId.Substring(1);

        return int.Parse(withoutLeadingP);
    }

    private int GetNextPlayerNumber()
    {
        if (Players == null)
            return 1;

        var largestPlayerNumber = 0;

        foreach(var player in Players)
        {
            var playerIdNumber = GetIntPortionOfPlayerId(player.Id);
            if ( playerIdNumber > largestPlayerNumber)
                largestPlayerNumber = playerIdNumber;
        }

        return ++largestPlayerNumber;
    }

    public GameState(GameStateDTO dto)
    {
        Id = Guid.Parse(dto.Id);

        Player? currentPlayer = null;
        Player? endPlayer = null;

        foreach (var playerDto in dto.Players)
        {
            var player = new Player(playerDto);
            Players.Add(player);

            if (dto.Phase != null && player.Id == dto.Phase.CurrentPlayerId)
                currentPlayer = player;

            if (dto.Phase != null && player.Id == dto.Phase.EndPlayerId)
                endPlayer = player;
        }

        NextPlayerId = GetNextPlayerNumber();

        foreach (var tileDto in dto.Tiles)
        {
            Tile tile = new Tile(tileDto);
            Tiles.Add(new Tile(tileDto));
            if (tile.Id == dto.RobberTileId)
                RobberTile = tile;
        }

        Settings = new GameSettings(dto.Settings);

        foreach (var edgeDto in dto.Edges)
            Edges.Add(new Edge(edgeDto, Players, Tiles));

        foreach (var vertexDto in dto.Vertices)
            Vertices.Add(new Vertex(vertexDto, Players, Tiles));

        foreach (var portDto in dto.Ports)
            Ports.Add(new Port(portDto, Vertices));

        foreach (var er in dto.EventRecord)
            EventRecord.Add(er);

        NextEventId = dto.NextEventId;

        foreach (var dc in dto.DevelopmentCards)
            DevelopmentCards.Add(dc);

        Dice = dto.Dice;

        if (dto.Phase != null)
            Phase = new GamePhase(this, dto.Phase);
        else
            Phase = new GamePhase(GameStates.SettingUpBoard);

        if (dto.HasLargestArmyPlayerId != null)
            PlayerWithLargestArmy = Players.First(p => p.Id == dto.HasLargestArmyPlayerId);

        if (dto.HasLongestRoadPlayerId != null)
            PlayerWithLongestRoad = Players.First(p => p.Id == dto.HasLongestRoadPlayerId);

        UndoState = new Stack<PreActionState>(dto.UndoState);

        if (dto.Bank == null)
            Bank = new Bank();
        else
            Bank = new Bank(dto.Bank);
    }

    public string GetNewPlayerId()
    {
        string playerId = $"P{NextPlayerId}";
        NextPlayerId++;

        return playerId;
    }

    public void AddPlayer(Player player)
    {
        Players.Add(player);
    }

    public void AddTile(Tile tile)
    {
        Tiles.Add(tile);
    }

    public void AddEdge(Edge edge)
    {
        Edges.Add(edge);
    }

    public void AddVertex(Vertex vertex)
    {
        Vertices.Add(vertex);
    }

    public void AddPort(Port port)
    {
        Ports.Add(port);
    }

    public void SetRobberTile(Tile tile)
    {
        if (RobberTile != null && tile.Id.Equals(RobberTile.Id))
            throw new ArgumentException("Unexpected Exception. Robber is already on the specified tile.");

        if (!Tiles.Any(t => t.Id == tile.Id))
            throw new ArgumentException("Unexpected Exception. The specified tile does not exist in the game.");

        RobberTile = tile;
    }

    public void PlaceRobberOnDesert()
    {
        var desertTile = Tiles.FirstOrDefault(t => t.Resource == ResourceType.Desert);
        if (desertTile == null)
            throw new InvalidOperationException("Unexpected Exception. No desert tile found in the game.");

        RobberTile = desertTile;
    }
    
    public void SetDiceForTesting(GameDice dice)
    {
       Dice = dice;
    }

    public void AssignLongestRoadToPlayer(Player player)
    {
        PlayerWithLongestRoad = player;
    }

    public void ClearLongestRoadPlayer()
    {
        PlayerWithLongestRoad = null;      
    }

    public void AssignLargestArmyToPlayer(Player player)
    {
        PlayerWithLargestArmy = player;
    }

    public void ClearLargestArmyPlayer()
    {
        PlayerWithLargestArmy = null;      
    }

    public Tile GetTileAt(int x, int y)
       => Tiles.First(t => t.X == x && t.Y == y);

    public Vertex GetVertexFromTileInfo(Tile t1, Tile? t2, Tile? t3, VertexDirection? dir)
    {
        if (t1 != null && t2 != null && t3 != null)
            return Vertices.First(v => v.Tiles.Contains(t1) && v.Tiles.Contains(t2) && v.Tiles.Contains(t3));

        if (t1 != null && t2 != null && t3 == null)
            return Vertices.First(v => v.Tiles.Count() == 2 && v.Tiles.Contains(t1) && v.Tiles.Contains(t2));

        if (t1 != null && t2 == null && t3 != null)
            return Vertices.First(v => v.Tiles.Count() == 2 && v.Tiles.Contains(t1) && v.Tiles.Contains(t3));

        return Vertices.First(v => v.Tiles.Count() == 1 && v.Tiles.Contains(t1) && v.Direction == dir);
    }

    public Edge GetEdgeFromTileInfo(Tile t1, Tile? t2, HexDirection? dir)
    {
        if (t2 != null)
            return Edges.First(e => e.Tiles.Contains(t1) && e.Tiles.Contains(t2));

        return Edges.First(e => e.Tiles.Contains(t1) && e.Direction == dir);
    }

    public int CountSettlementsForPlayer(Player player)
    {
        if (player == null) 
            throw new InvalidOperationException("Player should not be null");

        return Vertices.Count(v => v.Owner != null && v.Owner.Id == player.Id && v.Building == BuildingType.Settlement);
    }

    public int CountCitiesForPlayer(Player player)
    {
        if (player == null) 
            throw new InvalidOperationException("Player should not be null");

        return Vertices.Count(v => v.Owner != null && v.Owner.Id == player.Id && v.Building == BuildingType.City);
    }

    public int CountRoadsForPlayer(Player player)
    {
        if (player == null) 
            throw new InvalidOperationException("Player should not be null");

        return Edges.Count(v => v.Owner != null && v.Owner.Id == player.Id);
    }

    public int GetLongestRoadLength(int lengthSoFar, Player player, Edge edge, List<string> visitedEdgeIds, List<string> visitedVertexIds)
    {
        if (edge.Owner == null || (edge.Owner != null && edge.Owner.Id != player.Id) || visitedEdgeIds.Contains(edge.Id))
            return lengthSoFar;

        visitedEdgeIds.Add(edge.Id);
        lengthSoFar++;

        var newLongest = lengthSoFar;

        foreach (var vertex in edge.Vertices)
        {
            if ((vertex.Owner != null && vertex.Owner.Id != player.Id) || visitedVertexIds.Contains(vertex.Id))
                continue;

            visitedVertexIds.Add(vertex.Id);

            foreach (var edge2 in vertex.Edges)
            {
                var newLength = GetLongestRoadLength(lengthSoFar, player, edge2, new List<string>(visitedEdgeIds), new List<string>(visitedVertexIds));
                if (newLength > newLongest)
                    newLongest = newLength;
            }
        }

        return newLongest;
    }

    public int GetLongestRoadLength(Player player)
    {
        var longestSoFar = 0;

        foreach(var edge in Edges.Where(e => e.Owner != null && e.Owner.Id == player.Id)) {
            var length = GetLongestRoadLength(0, player, edge, new List<string>(), new List<string>());
            if (length > longestSoFar)
                longestSoFar = length;
        }

        return longestSoFar;
    }

    public void UpdatePlayerVictoryPoints()
    {
        foreach(var player in Players)
            UpdatePlayerVictoryPoints(player);
    }

    public void UpdatePlayerVictoryPoints(Player player)
    {
        int victoryPoints = CountSettlementsForPlayer(player) + (CountCitiesForPlayer(player) * 2);

        if (PlayerWithLargestArmy != null && PlayerWithLargestArmy.Id == player.Id)
            victoryPoints += 2;

        if (PlayerWithLongestRoad != null && PlayerWithLongestRoad.Id == player.Id)
            victoryPoints += 2;

        player.SetVictoryPoints(victoryPoints, victoryPoints + GamePlayHelpers.CountVictoryPointDevCardsForPlayer(player));
    }

    public bool UnusedRoadAvailable(Player player)
    {
        return CountRoadsForPlayer(player) < Settings.RoadsPerPlayer;
    }

    public bool UnusedSettlementAvailable(Player player)
    {
        return CountSettlementsForPlayer(player) < Settings.SettlementsPerPlayer;
    }

    public bool UnusedCityAvailable(Player player)
    {
        return CountCitiesForPlayer(player) < Settings.CitiesPerPlayer;
    }

    public int AddEventRecord(EventRecordDTO eventRecord)
    {
        eventRecord.Id = NextEventId++;
        EventRecord.Add(eventRecord);

        return eventRecord.Id;
    }

    public PreActionState GetPreActionStat()
    {
        return new PreActionState(-1, Phase, PlayerWithLongestRoad, PlayerWithLargestArmy);
    }

    // Accept optional eventRecordId override because many caller won't have the eventRecordId
    // before the action is preformed (which is too late to store/create the preActionState)
    public void PushUndoState(PreActionState preActionState, int? eventRecordId = null)
    {
        if (preActionState.EventRecordId < 0 && (eventRecordId == null || eventRecordId < 0))
            throw new InvalidOperationException("Unexpected Error. The EventRecordId must be set to a valid value.");
        
        if (eventRecordId != null && eventRecordId >= 0)
            preActionState.SetEventRecordId((int) eventRecordId);

        UndoState.Push(preActionState);
    }

    public void ClearUndoState()
    {
        UndoState.Clear();
    }

    public int GetBankResourceCount(ResourceType resource)
    {
        return Bank.GetResourceCount(resource);
    }

    public Dictionary<ResourceType, int> GetBankResources()
    {
        return Bank.GetBankResources();
    }

    public void AssignResourcesToPlayer(Player player, ResourceType resourceType, int resourceCount)
    {
        Bank.WithdrawResources(resourceType, resourceCount);
        player.AssignResources(resourceType, resourceCount);
    }

    public void WithdrawResourcesToBuildRoad(Player player)
    {
        if (GamePlayHelpers.HasResourcesToBuildRoad(player))
        {
            player.RemoveResources(ResourceType.Wood, 1);
            player.RemoveResources(ResourceType.Brick, 1);
            Bank.ReturnResources(ResourceType.Wood, 1);
            Bank.ReturnResources(ResourceType.Brick, 1);
        }
        else
            throw new InvalidOperationException("Player does not have required resources to build road.");
    }

    public void WithdrawResourcesToBuildSettlement(Player player)
    {
        if (!GamePlayHelpers.HasResourcesToBuildSettlement(player))
            throw new InvalidOperationException("Player does not have required resources to build settlement.");

        player.RemoveResources(ResourceType.Wood, 1);
        player.RemoveResources(ResourceType.Brick, 1);
        player.RemoveResources(ResourceType.Wool, 1);
        player.RemoveResources(ResourceType.Grain, 1);
        Bank.ReturnResources(ResourceType.Wood, 1);
        Bank.ReturnResources(ResourceType.Brick, 1);
        Bank.ReturnResources(ResourceType.Wool, 1);
        Bank.ReturnResources(ResourceType.Grain, 1);
    }

    public void WithdrawResourcesToBuildCity(Player player)
    {
        if (GamePlayHelpers.HasResourcesToBuildCity(player))
        {
            player.RemoveResources(ResourceType.Ore, 3);
            player.RemoveResources(ResourceType.Grain, 2);
            Bank.ReturnResources(ResourceType.Ore, 3);
            Bank.ReturnResources(ResourceType.Grain, 2);
        }
        else
            throw new InvalidOperationException("Player does not have required resources to build city.");
    }

    public void WithdrawResourcesToBuyDevCard(Player player)
    {
        if (GamePlayHelpers.HasResourcesToBuyDevCard(player))
        {
            player.RemoveResources(ResourceType.Ore, 1);
            player.RemoveResources(ResourceType.Wool, 1);
            player.RemoveResources(ResourceType.Grain, 1);
            Bank.ReturnResources(ResourceType.Ore, 1);
            Bank.ReturnResources(ResourceType.Wool, 1);
            Bank.ReturnResources(ResourceType.Grain, 1);
        }
    }

    public void AssignResourcesToPlayers(Dictionary<Player, Dictionary<ResourceType, int>> resources)
    {
        // First, find out if the bank has enough resources to pay everyone. Skip assignment for resources that the bank is short
        var resourcesNeeded = new Dictionary<ResourceType, int>();

        foreach (var kvpPlayer in resources)
        {
            foreach (var kvpResource in kvpPlayer.Value)
            {
                if (!resourcesNeeded.ContainsKey(kvpResource.Key))
                    resourcesNeeded.Add(kvpResource.Key, 0);

                resourcesNeeded[kvpResource.Key] += kvpResource.Value;
            }
        }

        var resourceShortages = new List<ResourceType>();
        foreach (var kvpResource in resourcesNeeded)
            if (GetBankResourceCount(kvpResource.Key) < kvpResource.Value)
                resourceShortages.Add(kvpResource.Key);

        // Now assign resources to players and log events
        foreach (var kvpPlayer in resources)
        {
            var gained = new Dictionary<ResourceType, int>();
            foreach (var kvpResource in kvpPlayer.Value)
            { 
                if (resourceShortages.Contains(kvpResource.Key))
                    continue;

                kvpPlayer.Key.AssignResources(kvpResource.Key, kvpResource.Value);
                Bank.WithdrawResources(kvpResource.Key, kvpResource.Value);
                gained.Add(kvpResource.Key, kvpResource.Value);
            }

            AddEventRecord(new EventRecordDTO(kvpPlayer.Key, EventRecordAction.ReceivedResources, gained));
        }
    }

    public void RemoveResourcesFromPlayer(Player player, Dictionary<ResourceType, int> resources)
    {
        foreach (var kvp in resources)
        {
            player.RemoveResources(kvp.Key, kvp.Value);
            Bank.ReturnResources(kvp.Key, kvp.Value);
        }
    }

    public void RemoveResourcesFromPlayer(Player player, ResourceType resourceType, int resourceCount)
    {
        player.RemoveResources(resourceType, resourceCount);
        Bank.ReturnResources(resourceType, resourceCount);
    }

    public Dictionary<Player, Dictionary<ResourceType, int>> GetResourcesEarnedOnLastRoll()
    {
        var resourcesEarned = new Dictionary<Player, Dictionary<ResourceType, int>>();

        if (Dice.GetCombinedValue() == 7)
            return resourcesEarned;

        var matchingTiles = Tiles.FindAll(t => t.DiceNumber == Dice.GetCombinedValue() && t.Id != RobberTile.Id);

        foreach (var tile in matchingTiles)
        {
            foreach (var vertex in Vertices)
            {
                if (vertex.Tiles.Contains(tile) && vertex.Building != null && vertex.Owner != null)
                {
                    if (!resourcesEarned.ContainsKey(vertex.Owner))
                        resourcesEarned.Add(vertex.Owner, new Dictionary<ResourceType, int>());

                    var victoryPoints = GamePlayHelpers.GetVictoryPointsForBuild(vertex.Building);

                    if (!resourcesEarned[vertex.Owner].ContainsKey(tile.Resource))
                        resourcesEarned[vertex.Owner].Add(tile.Resource, victoryPoints);
                    else
                        resourcesEarned[vertex.Owner][tile.Resource] += victoryPoints;
                }
            }
        }

        return resourcesEarned;
    }

    public void AssignResourcesBasedOnLastDiceRoll()
    {
        var resources = GetResourcesEarnedOnLastRoll();
        AssignResourcesToPlayers(resources);
    }

    public ResponseDTO TradeWithBank(GameState gs, Player player, Dictionary<ResourceType, int> offer, Dictionary<ResourceType, int> request)
    {
        return Bank.TradeWithBank(gs, player, offer, request);  
    }
}