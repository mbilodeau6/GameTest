using GameTest.DTOs;
using GameTest.Models;

namespace GameTest.Services;

public static class GamePlayHelpers
{
    private static readonly Random _random = new();

    public static int GetVictoryPointsForBuild(BuildingType? type)
    {
        if (type == BuildingType.Settlement)
            return 1;

        if (type == BuildingType.City)
            return 2;

        return 0;
    }

    public static Dictionary<Player, Dictionary<ResourceType, int>> GetResourcesEarnedOnLastRoll(GameState gs)
    {
        var resourcesEarned = new Dictionary<Player, Dictionary<ResourceType, int>>();

        if (gs.Dice.GetCombinedValue() == 7)
            return resourcesEarned;

        var matchingTiles = gs.Tiles.FindAll(t => t.DiceNumber == gs.Dice.GetCombinedValue() && t.Id != gs.RobberTile.Id);

        foreach (var tile in matchingTiles)
        {
            foreach (var vertex in gs.Vertices)
            {
                if (vertex.Tiles.Contains(tile) && vertex.Building != null && vertex.Owner != null)
                {
                    if (!resourcesEarned.ContainsKey(vertex.Owner))
                        resourcesEarned.Add(vertex.Owner, new Dictionary<ResourceType, int>());

                    var victoryPoints = GetVictoryPointsForBuild(vertex.Building);

                    if (!resourcesEarned[vertex.Owner].ContainsKey(tile.Resource))
                        resourcesEarned[vertex.Owner].Add(tile.Resource, victoryPoints);
                    else
                        resourcesEarned[vertex.Owner][tile.Resource] += victoryPoints;
                }
            }
        }

        return resourcesEarned;
    }

    public static void AssignResourcesToPlayers(GameState gameState, Dictionary<Player, Dictionary<ResourceType, int>> resources)
    {
        foreach (var kvpPlayer in resources)
        {
            foreach (var kvpResource in kvpPlayer.Value)
            {
                kvpPlayer.Key.AssignResources(kvpResource.Key, kvpResource.Value);
            }
        }
    }

    public static void AssignResourcesBasedOnLastDiceRoll(GameState gameState)
    {
        var resources = GetResourcesEarnedOnLastRoll(gameState);
        AssignResourcesToPlayers(gameState, resources);
    }

    public static bool HasResourcesToBuildRoad(Player player)
    {
        return player.Resources.ContainsKey(ResourceType.Wood) && player.Resources[ResourceType.Wood] >= 1 &&
               player.Resources.ContainsKey(ResourceType.Brick) && player.Resources[ResourceType.Brick] >= 1;
    }

    public static void WithdrawResourcesToBuildRoad(Player player)
    {
        if (HasResourcesToBuildRoad(player))
        {
            player.RemoveResources(ResourceType.Wood, 1);
            player.RemoveResources(ResourceType.Brick, 1);
        }
        else
            throw new InvalidOperationException("Player does not have required resources to build road.");
    }

    public static bool HasResourcesToBuildSettlement(Player player)
    {
        return player.Resources.ContainsKey(ResourceType.Wood) && player.Resources[ResourceType.Wood] >= 1 &&
               player.Resources.ContainsKey(ResourceType.Brick) && player.Resources[ResourceType.Brick] >= 1 &&
               player.Resources.ContainsKey(ResourceType.Wool) && player.Resources[ResourceType.Wool] >= 1 &&
               player.Resources.ContainsKey(ResourceType.Grain) && player.Resources[ResourceType.Grain] >= 1;
    }

    public static void WithdrawResourcesToBuildSettlement(Player player)
    {
        if (!HasResourcesToBuildSettlement(player))
            throw new InvalidOperationException("Player does not have required resources to build settlement.");

        player.RemoveResources(ResourceType.Wood, 1);
        player.RemoveResources(ResourceType.Brick, 1);
        player.RemoveResources(ResourceType.Wool, 1);
        player.RemoveResources(ResourceType.Grain, 1);
    }

    public static bool HasResourcesToBuildCity(Player player)
    {
        return player.Resources.ContainsKey(ResourceType.Grain) && player.Resources[ResourceType.Grain] >= 2 &&
               player.Resources.ContainsKey(ResourceType.Ore) && player.Resources[ResourceType.Ore] >= 3;
    }

    public static void WithdrawResourcesToBuildCity(Player player)
    {
        if (HasResourcesToBuildCity(player))
        {
            player.RemoveResources(ResourceType.Ore, 3);
            player.RemoveResources(ResourceType.Grain, 2);
        }
        else
            throw new InvalidOperationException("Player does not have required resources to build city.");
    }

    public static bool HasResourcesToBuyDevCard(Player player)
    {
        return (player.Resources.ContainsKey(ResourceType.Ore) && player.Resources[ResourceType.Ore] >= 1 &&
            player.Resources.ContainsKey(ResourceType.Grain) && player.Resources[ResourceType.Grain] >= 1 &&
            player.Resources.ContainsKey(ResourceType.Wool) && player.Resources[ResourceType.Wool] >= 1);
    }

    public static void WithdrawResourcesToBuyDevCard(Player player)
    {
        if (HasResourcesToBuyDevCard(player))
        {
            player.RemoveResources(ResourceType.Ore, 1);
            player.RemoveResources(ResourceType.Wool, 1);
            player.RemoveResources(ResourceType.Grain, 1);
        }
    }

    public static int CountVictoryPointDevCardsForPlayer(Player player)
    {
        return player.DevCardsPurchasedThisRound.Count(d => d == DevelopmentCardType.VictoryPoint) 
            + player.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.VictoryPoint);
    }

    public static void EndTurn(Player player, GameState gameState, bool skipGameLoop = false)
    {
        // Verify EndTurn is only called in appropriate circumstances. Expect callers to protects
        // against these scenarios.
        if (gameState.Phase.CurrentPlayer == null)
            throw new InvalidOperationException("Can not end turn without a CurrentPlayer.");

        if (gameState.Phase.PhaseState != GameStates.BuildOrTrade)
            throw new InvalidOperationException("Can not end turn on any phase but BuildOrTrade.");

        if (player.Id != gameState.Phase.CurrentPlayer.Id)
            throw new InvalidOperationException("Can not end the turn for another player.");

        player.MakeNewDevelopmentCardsPlayable();

        // TODO: Shouldn't have GetNextPlayer exposed here.
        gameState.Phase.CurrentPlayer = gameState.Phase.GetNextPlayer(gameState.Phase.CurrentPlayer, gameState.Players);
        gameState.Phase.PhaseState = GameStates.RollOrUseDevCard;
        gameState.Phase.SetWaitingForRoll();

        if (!skipGameLoop)
            GameLoop(gameState);
    }

    private static Edge GetEdgeFromEdgeId(GameState gs, string edgeId)
    {
        return gs.Edges.First(e => e.Id == edgeId);
    }

    private static Vertex GetVertexFromVertexId(GameState gs, string vertexId)
    {
        return gs.Vertices.First(e => e.Id == vertexId);
    }

    private static Tile GetTileFromTileId(GameState gs, string tileId)
    {
        return gs.Tiles.First(t => t.Id == tileId);
    }

    public static Player GetPlayerFromPlayerId(GameState gs, string playerId)
    {
        return gs.Players.First(p => p.Id == playerId);
    }

    public static bool IsPlayerSetupPhase(GameState gs)
    {
        return gs.Phase.PhaseState == GameStates.PlaceFirstSettlement ||
            gs.Phase.PhaseState == GameStates.PlaceFirstRoad ||
            gs.Phase.PhaseState == GameStates.PlaceSecondSettlement ||
            gs.Phase.PhaseState == GameStates.PlaceSecondRoad;
    }

    public static void StartGame(GameState gameState)
    {
        GameLoop(gameState);
    }

    private static bool BuildRoadPhase(GameState gs)
    {
        return gs.Phase.PhaseState == GameStates.PlaceFirstRoad ||
            gs.Phase.PhaseState == GameStates.PlaceSecondRoad ||
            gs.Phase.PhaseState == GameStates.BuildOrTrade ||
            gs.Phase.PhaseState == GameStates.FirstDevCardRoad ||
            gs.Phase.PhaseState == GameStates.SecondDevCardRoad;
    }

    private static bool BuildSettlementPhase(GameState gs)
    {
        return gs.Phase.PhaseState == GameStates.PlaceFirstSettlement ||
            gs.Phase.PhaseState == GameStates.PlaceSecondSettlement ||
            gs.Phase.PhaseState == GameStates.BuildOrTrade;
    }

    public static Vertex FindSettlementWithNoRoads(GameState gs, Player player)
    {
        List<Vertex> verticesWithoutRoads = new List<Vertex>();
        foreach(var vertex in gs.Vertices.FindAll(v => v.Owner != null && v.Owner.Id == player.Id))
            if (vertex.Edges.All(e => e.Owner == null))
                verticesWithoutRoads.Add(vertex);

        if (verticesWithoutRoads.Count > 1)
            throw new InvalidOperationException("Invalid state. A valid game can not have two or more settlements without any roads.");

        if (verticesWithoutRoads.Count == 1)
            return verticesWithoutRoads.First();
        else
            throw new InvalidOperationException("Unexpected Error. FindSettlementWithNoRoads() could not find a vertex without a road.");
    }



    private static bool BuildCityPhase(GameState gs)
    {
        return gs.Phase.PhaseState == GameStates.BuildOrTrade;
    }

    public static void BuildRoad(GameState gs, Player player, Edge edge)
    {
        if (gs.Phase.PhaseState == GameStates.BuildOrTrade)
            WithdrawResourcesToBuildRoad(player);

        if (gs.Phase.PhaseState == GameStates.PlaceSecondRoad)
        {
            var targetVertex =  FindSettlementWithNoRoads(gs, player);
            if (!edge.Vertices.Any(v => v.Id == targetVertex.Id))
            throw new InvalidOperationException($"Unexpected Error. Attempting to place second road away from second settlement. GameId: {gs.Id}; EdgeId: {edge.Id}; Player: {player.Id}");
        }

        edge.BuildRoad(player);
        gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.PlaceRoad, edge));
    }

    public static ResponseDTO BuildRoadRequestFromUser(GameState gs, string playerId, string edgeId)
    {
        if (!BuildRoadPhase(gs) || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: BuildRoad; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != playerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (!UnusedRoadAvailable(gs, player))
            return new ResponseDTO(false, 1030, $"GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);


        var edge = gs.Edges.FirstOrDefault(e => e.Id == edgeId);
        if (edge == null)
            return new ResponseDTO(false, 1013, $"GameId: {gs.Id}; EdgeId: {edgeId}", null as GameStateDTO);

        if (!IsEdgeAdjacentToPlayerBuild(gs, edge, player))
            return new ResponseDTO(false, 1015, $"GameId: {gs.Id}; EdgeId: {edgeId}; Player: {playerId}", null as GameStateDTO);

        if (gs.Phase.PhaseState == GameStates.PlaceSecondRoad)
        {
            var targetVertex =  FindSettlementWithNoRoads(gs, player);
            if (!edge.Vertices.Any(v => v.Id == targetVertex.Id))
                return new ResponseDTO(false, 1042, $"GameId: {gs.Id}; EdgeId: {edgeId}; Player: {playerId}", null as GameStateDTO);
        }

        if (edge.Owner != null)
            return new ResponseDTO(false, 1016, $"GameId: {gs.Id}; EdgeId: {edgeId}", null as GameStateDTO);

        if (gs.Phase.PhaseState == GameStates.BuildOrTrade && !HasResourcesToBuildRoad(player))
            return new ResponseDTO(false, 1017, $"Action: BuildRoad; GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        BuildRoad(gs, player, edge);

        GameLoop(gs);

        return new ResponseDTO(true, 0, string.Empty, gs);
    }

    public static void BuildSettlement(GameState gs, Player player, Vertex vertex)
    {
        if (gs.Phase.PhaseState == GameStates.BuildOrTrade)
            WithdrawResourcesToBuildSettlement(player);
        else if (gs.Phase.PhaseState == GameStates.PlaceSecondSettlement)
            foreach (var tile in vertex.Tiles)
                if (tile.Resource != ResourceType.Desert)
                    player.AssignResources(tile.Resource, 1);

        vertex.BuildSettlement(player);
        gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.PlaceSettlement, vertex));
        MarkBlockedVertices(gs, vertex);
        PopulatePlayerPorts(gs);
        gs.UpdatePlayerVictoryPoints(player);
    }

    // TODO: Return a GameResult type that can indicate success/failure and include messages.
    // Right now, an empty string indicates success.
    public static ResponseDTO BuildSettlementRequestFromUser(GameState gs, string playerId, string vertexId)
    {
        if (!BuildSettlementPhase(gs) || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: BankSettlement; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != playerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (!UnusedSettlementAvailable(gs, player))
            return new ResponseDTO(false, 1029, $"GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);
        if (vertex == null)
            return new ResponseDTO(false, 1014, $"GameId: {gs.Id}; VertexId: {vertexId}", null as GameStateDTO);

        if (vertex.Building == BuildingType.Settlement)
            return new ResponseDTO(false, 1018, $"GameId: {gs.Id}; VertexId: {vertexId}", null as GameStateDTO);

        if (vertex.Building == BuildingType.City)
            return new ResponseDTO(false, 1019, $"GameId: {gs.Id}; VertexId: {vertexId}", null as GameStateDTO);

        if (vertex.Building == BuildingType.Blocked)
            return new ResponseDTO(false, 1022, $"GameId: {gs.Id}; VertexId: {vertexId}", null as GameStateDTO);

        if (gs.Phase.PhaseState == GameStates.BuildOrTrade && !IsVertexAdjacentToPlayerRoad(gs, vertex, player))
            return new ResponseDTO(false, 1023, $"GameId: {gs.Id}; VertexId: {vertexId}", null as GameStateDTO);

        if (gs.Phase.PhaseState == GameStates.BuildOrTrade && !HasResourcesToBuildSettlement(player))
            return new ResponseDTO(false, 1017, $"Action: BuildSettlement; GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        BuildSettlement(gs, player, vertex);

        GameLoop(gs);

        return new ResponseDTO(true, 0, string.Empty, gs);
    }

    public static void UpgradeToCity(GameState gs, Player player, Vertex vertex)
    {
        if (BuildCityPhase(gs))
        {
            WithdrawResourcesToBuildCity(player);
            vertex.UpgradeToCity();
            gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.UpgradeSettlement, vertex));
            gs.UpdatePlayerVictoryPoints(player);
        }
    }


    public static ResponseDTO UpgradeToCityRequestFromUser(GameState gs, string playerId, string vertexId)
    {
        if (!BuildCityPhase(gs) || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: BuildCity; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != playerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (!UnusedCityAvailable(gs, player))
            return new ResponseDTO(false, 1028, $"GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);
        if (vertex == null)
            return new ResponseDTO(false, 1014, $"GameId: {gs.Id}; VertexId: {vertexId}", null as GameStateDTO);

        if (vertex.Building == null || vertex.Owner == null)
            return new ResponseDTO(false, 1020, $"GameId: {gs.Id}; VertexId: {vertexId}", null as GameStateDTO);

        if (vertex.Building == BuildingType.City)
            return new ResponseDTO(false, 1019, $"GameId: {gs.Id}; VertexId: {vertexId}", null as GameStateDTO);

        if (vertex.Owner.Id != playerId)
            return new ResponseDTO(false, 1019, $"GameId: {gs.Id}; VertexId: {vertexId}; OwnerId: {vertex.Owner.Id}", null as GameStateDTO);

        if (!HasResourcesToBuildCity(player))
            return new ResponseDTO(false, 1017, $"Action: BuildCity; GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        UpgradeToCity(gs, player, vertex);

        GameLoop(gs);

        return new ResponseDTO(true, 0, string.Empty, gs);
    }

    public static void GameLoop(GameState gs)
    {
        int loopCounter = 0; // Failsafe to prevent infinite loops

        gs.Phase = gs.Phase.GetNextPhase(gs.Players, 
            gs.CountSettlementsForPlayer(gs.Phase.CurrentPlayer), gs.CountRoadsForPlayer(gs.Phase.CurrentPlayer), 
            gs.Dice.GetCombinedValue(), gs.RobberTile);

        if (gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException("Shouldn't call GameLoop before current player set.");

        if (gs.Phase.CurrentPlayer.IsBot)
        {
            var bot = new BotAI(gs);

            while (gs.Phase.CurrentPlayer.IsBot)
            {
                if (loopCounter++ > 100)
                    throw new InvalidOperationException("GameLoop appears to be stuck in an infinite loop");

                BotMove move;

                // TODO: Would a swtich be more appropriate than if/elseif?
                if (IsPlayerSetupPhase(gs))
                {
                    move = bot.GetSetUpMove();
                }
                else if (gs.Phase.PhaseState == GameStates.RollOrUseDevCard)
                {
                    move = bot.GetPreRollMove();
                }
                else if (gs.Phase.PhaseState == GameStates.BuildOrTrade)
                {
                    move = bot.GetBuildMove();
                }
                else if (gs.Phase.PhaseState == GameStates.PlaceRobber)
                {
                    move = bot.GetRobberMove();
                }
                else
                {
                    // TODO: Other states not implemented yet.
                    break;
                }

                if (move.EdgeMove != null)
                {
                    var edge = GetEdgeFromEdgeId(gs, move.EdgeMove.Id);
                    BuildRoad(gs, gs.Phase.CurrentPlayer, edge);
                }

                if (move.VertexMove != null)
                {
                    var vertex = GetVertexFromVertexId(gs, move.VertexMove.Id);

                    if (move.VertexMove.Building == BuildingType.Settlement.ToString())
                        BuildSettlement(gs, gs.Phase.CurrentPlayer, vertex);
                    else
                        UpgradeToCity(gs, gs.Phase.CurrentPlayer, vertex);
                }

                if (move.BankTrade != null)
                {
                    BankTradeFromUser(gs, move.BankTrade);
                }

                if (move.RollDice)
                    GamePlayHelpers.RollDice(gs, true);

                if (move.EndTurn)
                    GamePlayHelpers.EndTurn(gs.Phase.CurrentPlayer, gs, true);

                if (move.TileMove != null && gs.Phase.PhaseState == GameStates.PlaceRobber)
                {
                    var tile = GetTileFromTileId(gs, move.TileMove.Id);
                    GamePlayHelpers.PlaceRobber(gs, gs.Phase.CurrentPlayer, tile);
                }

                gs.Phase = gs.Phase.GetNextPhase(gs.Players, 
                    gs.CountSettlementsForPlayer(gs.Phase.CurrentPlayer), gs.CountRoadsForPlayer(gs.Phase.CurrentPlayer), 
                    gs.Dice.GetCombinedValue(), gs.RobberTile);
            }
        }
    }

    public static void RollDice(GameState gs, bool skipGameLoop = false)
    {
        if (gs.Phase.PhaseState != GameStates.RollOrUseDevCard || gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException($"Unexpected Exception. Roll called when game in {gs.Phase.PhaseState}. Player: {gs.Phase.CurrentPlayer}.");

        gs.Dice.Roll();
        gs.Phase.ClearWaitingForRoll();
        GamePlayHelpers.AssignResourcesBasedOnLastDiceRoll(gs);
        gs.EventRecord.Add(new EventRecordDTO(gs.Phase.CurrentPlayer, EventRecordAction.RollDice, gs.Dice));

        if (!skipGameLoop)
            GameLoop(gs);
    }

    public static void MarkBlockedVertices(GameState gs, Vertex vertex)
    {
        foreach (var linkedEdge in vertex.Edges)
            foreach (var linkedVertex in linkedEdge.Vertices)
                if (linkedVertex.Id != vertex.Id && linkedVertex.Building == null)
                    linkedVertex.MarkBlocked();
    }

    public static void MarkBlockedVertices(GameState gs)
    {
        BoardCreationHelpers.LinkEdgesAndVertices(gs);

        foreach (var vertex in gs.Vertices)
        {
            if (vertex.Building != null && HasBuilding(vertex))
                MarkBlockedVertices(gs, vertex);
        }
    }

    public static GameState LoadAndPrepareGameStateDTO(GameStateDTO dto)
    {
        var gs = new GameState(dto);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);
        MarkBlockedVertices(gs);
        PopulatePlayerPorts(gs);
        foreach(var player in gs.Players)
            gs.UpdatePlayerVictoryPoints(player);

        return gs;
    }

    public static bool HasBuilding(Vertex vertex)
    {
        return vertex.Building == BuildingType.Settlement || vertex.Building == BuildingType.City;
    }

    public static bool IsEdgeAdjacentToPlayerBuild(GameState gs, Edge edge, Player player)
    {
        foreach (var vertex in edge.Vertices)
        {
            if (HasBuilding(vertex) && vertex.Owner != null && vertex.Owner.Id == player.Id)
            {
                return true;
            }

            if (!HasBuilding(vertex) && vertex.Edges.Any(e => e.Owner != null && e.Owner.Id == player.Id))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsVertexAdjacentToPlayerRoad(GameState gs, Vertex vertex, Player player)
    {
        foreach (var edge in vertex.Edges)
        {
            if (edge.Owner != null && edge.Owner.Id == player.Id)
            {
                return true;
            }
        }

        return false;
    }

    public static ResponseDTO BankTrade(GameState gs, TradeRequest request)
    {
        Bank bank = new Bank();
        var response = bank.TradeWithBank(gs, request.Player, request.Offer, request.Request);

        if (response.Success)
            response.GameState.EventRecord.Add(new EventRecordDTO(request.Player, EventRecordAction.TradeWithBank, request.Request, request.Offer ));

        return response;
    }
    
    public static ResponseDTO BankTradeFromUser(GameState gs, TradeRequestDTO request)
    {
        if (gs.Phase.PhaseState != GameStates.BuildOrTrade || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: BankTrade; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != request.PlayerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        
        return BankTrade(gs, new TradeRequest(gs, request));
    }

    public static bool UnusedRoadAvailable(GameState gs, Player player)
    {
        return gs.CountRoadsForPlayer(player) < gs.Settings.RoadsPerPlayer;
    }

    public static bool UnusedSettlementAvailable(GameState gs, Player player)
    {
        return gs.CountSettlementsForPlayer(player) < gs.Settings.SettlementsPerPlayer;
    }

    public static bool UnusedCityAvailable(GameState gs, Player player)
    {
        return gs.CountCitiesForPlayer(player) < gs.Settings.CitiesPerPlayer;
    }

    public static void PopulatePlayerPorts(GameState gs)
    {
        foreach(var port in gs.Ports)
            foreach(var vertex in port.Vertices)
                if (vertex.Owner != null)
                    vertex.Owner.AddPort(port.Type);
    }

    public static void PlaceRobber(GameState gs, Player player, Tile tile)
    {
        // Move Robber
        gs.SetRobberTile(tile);
        gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.PlaceRobber, tile));

        // Steal resource from player with building on the target tile
        // TODO: If there are multiple players on the tile, need to ask the user which player to steal from
        // if more than one player has resources
        foreach(var vertex in gs.Vertices)
        {
            if (vertex.Tiles.Any(t => t.Id == tile.Id) && vertex.Owner != null && vertex.Owner.Id != player.Id)
            {
                List<ResourceType> targetResources = new List<ResourceType>();
                foreach (var pair in vertex.Owner.Resources)
                    for (int i = 0; i < pair.Value; i++ )
                        targetResources.Add(pair.Key);

                if (targetResources.Count > 0)
                {
                    var resourceToSteal = targetResources[_random.Next(targetResources.Count)];
                    vertex.Owner.RemoveResources(resourceToSteal, 1);
                    player.AssignResources(resourceToSteal, 1);

                    gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.StealResource, vertex.Owner, resourceToSteal));
                    break;
                }
            }
        }
    }

    public static ResponseDTO PlaceRobberForUser(GameState gs, string playerId, string tileId)
    {
        if (gs.Phase.PhaseState != GameStates.PlaceRobber || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: PlaceRobber; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != playerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var tile = gs.Tiles.FirstOrDefault(t => t.Id == tileId);
        if (tile == null)
            return new ResponseDTO(false, 1032, $"GameId: {gs.Id}; TileId: {tileId}", null as GameStateDTO);
        
        if (gs.Phase.OriginalRobberTile == null || gs.Phase.OriginalRobberTile.Id == tile.Id)
            return new ResponseDTO(false, 1033, $"GameId: {gs.Id}; Player: {player.Id}; OriginalTile: {gs.Phase.OriginalRobberTile.Id}; NewTile: {tile.Id}", null as GameStateDTO);

        PlaceRobber(gs, player, tile);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null, gs);
    }

    public static void BuyDevCard(GameState gs, Player player)
    {
        if (gs.Phase.PhaseState != GameStates.BuildOrTrade || gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException($"Unexpected Error. BuyDevCard not valid in {gs.Phase.PhaseState} state.");

        if (gs.Phase.CurrentPlayer.Id != player.Id)
            throw new InvalidOperationException($"Unexpected Error. It is not player {player.Id}'s turn.");

        if (gs.DevelopmentCards.Count == 0)
            throw new InvalidOperationException("Unexpected Error. All development cards have been used.");

        if (!HasResourcesToBuyDevCard(player))
            throw new InvalidOperationException("Unexpected Error. Player doesn't have resources to buy dev card.");

        player.AssignDevelopmentCard(gs.DevelopmentCards[0]);
        gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.BuyDevelopmentCard, gs.DevelopmentCards[0]));
        gs.DevelopmentCards.RemoveAt(0);
        WithdrawResourcesToBuyDevCard(player);
        gs.UpdatePlayerVictoryPoints(player);
    }
    public static ResponseDTO BuyDevCardFromUser(GameState gs, string playerId)
    {
        if (gs.Phase.PhaseState != GameStates.BuildOrTrade || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: BuyDevCard; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != playerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (!HasResourcesToBuyDevCard(player))
            return new ResponseDTO(false, 1017, $"Action: BuyDevCard; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}", null as GameStateDTO);

        BuyDevCard(gs, player);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null, gs);
    }

    private static void StandardPlayDevCardValidation(GameState gs, Player player, DevelopmentCardType devCard)
    {
        if ((gs.Phase.PhaseState != GameStates.RollOrUseDevCard && gs.Phase.PhaseState != GameStates.BuildOrTrade) || gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException($"Unexpected Error. PlayDevCard not valid in {gs.Phase.PhaseState} state.");

        if (player == null)
            throw new ArgumentNullException("player");

        if (gs.Phase.CurrentPlayer.Id != player.Id)
            throw new InvalidOperationException($"Unexpected Error. It is not player {player.Id}'s turn.");

        if (!player.DevCardsReadyToPlay.Contains(devCard))
            throw new InvalidOperationException($"Unexpected Error. Player does not have a {devCard} to play.");
    }

    private static ResponseDTO StandardPlayDevCardValidationForUserRequest(GameState gs, PlayDevCardRequest request, DevelopmentCardType targetType)
    {
        if ((gs.Phase.PhaseState != GameStates.RollOrUseDevCard && gs.Phase.PhaseState != GameStates.BuildOrTrade) || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: Play{targetType}DevCard; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (request.DevCardType != targetType.ToString())
            return new ResponseDTO(false, 9999, $"Requested: {request.DevCardType}; Called: {targetType}; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != request.PlayerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (request.SelectedResources != null)
        {
            var requestedResource = ResourceType.Desert;

            foreach (var resource in request.SelectedResources)
            {
                try {
                    requestedResource = Enum.Parse<ResourceType>(resource);
                }
                catch (ArgumentException)
                {
                    // convert exception into failure  resources
                    return new ResponseDTO(false, 1038, $"Action: Play{targetType}DevCard; RequestedResource: {resource}; GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);
                }

                if (requestedResource == ResourceType.Desert)
                    return new ResponseDTO(false, 1038, $"Action: Play{targetType}DevCard; RequestedResource: {resource}; GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);
            }
        }

        if (!player.DevCardsReadyToPlay.Contains(targetType))
            return new ResponseDTO(false, 1039, $"Action: Play{targetType}DevCard; GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        return new ResponseDTO(true, 0, null, gs);
    }

    public static void PlayMonopolyDevCard(GameState gs, Player player, ResourceType requestedResource)
    {
        StandardPlayDevCardValidation(gs, player, DevelopmentCardType.Monopoly);

        if (requestedResource == ResourceType.Desert)
            throw new InvalidOperationException($"Unexpected Error. Desert is not a valid resource to request in PlayMonopolyDevCard.");

        var resourcesReceived = 0;

        foreach(var opponent in gs.Players)
        {
            if (opponent.Id == player.Id || !opponent.Resources.ContainsKey(requestedResource))
                continue;

            var opponentCount = opponent.Resources[requestedResource];
            opponent.RemoveResources(requestedResource, opponentCount);
            player.AssignResources(requestedResource, opponentCount);
            resourcesReceived += opponentCount;
        }

        player.PlayDevelopmentCard(DevelopmentCardType.Monopoly);
        gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.PlayMonoploy, new Dictionary<ResourceType, int>() { {requestedResource, resourcesReceived} }));
    }

    public static ResponseDTO PlayMonopolyDevCardFromUser(GameState gs, PlayDevCardRequest request)
    {
        if (request.SelectedResources == null || request.SelectedResources.Count != 1)
            return new ResponseDTO(false, 1037, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        var response = StandardPlayDevCardValidationForUserRequest(gs, request, DevelopmentCardType.Monopoly);

        if (!response.Success)
            return response;

        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        var requestedResource = Enum.Parse<ResourceType>(request.SelectedResources[0]);

        PlayMonopolyDevCard(gs, player, requestedResource);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null, gs);
    }

    public static void PlayYearOfPlentyDevCard(GameState gs, Player player, List<ResourceType> requestedResources)
    {
        StandardPlayDevCardValidation(gs, player, DevelopmentCardType.YearOfPlenty);

        if (requestedResources.Count != 2)
            throw new InvalidOperationException($"Unexpected Error. Caller didn't request two resources.");

        foreach(var resource in requestedResources)
            if (resource == ResourceType.Desert)
                throw new InvalidOperationException($"Unexpected Error. Desert is not a valid resource to request in PlayYearOfPlentyDevCard.");

        foreach(var resource in requestedResources)
            player.AssignResources(resource, 1);

        player.PlayDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.PlayYearOfPlenty, new Dictionary<ResourceType, int>() { {requestedResources[0], 1}, {requestedResources[1], 1} }));
    }

    public static ResponseDTO PlayYearOfPlentyDevCardFromUser(GameState gs, PlayDevCardRequest request)
    {
        if (request.SelectedResources == null || request.SelectedResources.Count != 2)
            return new ResponseDTO(false, 1040, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        var response = StandardPlayDevCardValidationForUserRequest(gs, request, DevelopmentCardType.YearOfPlenty);

        if (!response.Success)
            return response;

        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        var requestedResources = new List<ResourceType>();
        foreach (var resourceString in request.SelectedResources)
            requestedResources.Add(Enum.Parse<ResourceType>(resourceString));

        PlayYearOfPlentyDevCard(gs, player, requestedResources);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null, gs);
    }

    public static void PlayRoadBuildingDevCard(GameState gs, Player player)
    {
        StandardPlayDevCardValidation(gs, player, DevelopmentCardType.RoadBuilding);

        gs.Phase.StoreStateDevCardRoadBuilding(gs.Phase.PhaseState, gs.Edges.Count(e => e.Owner != null && e.Owner.Id == player.Id));
        gs.Phase.PhaseState = GameStates.FirstDevCardRoad;
        player.PlayDevelopmentCard(DevelopmentCardType.RoadBuilding);
        gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.PlayRoadBuilding));

    }

    public static ResponseDTO PlayRoadBuildingDevCardFromUser(GameState gs, PlayDevCardRequest request)
    {
        var response = StandardPlayDevCardValidationForUserRequest(gs, request, DevelopmentCardType.RoadBuilding);

        if (!response.Success)
            return response;

        var player = gs.Players.First(p => p.Id == request.PlayerId);

        PlayRoadBuildingDevCard(gs, player);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null, gs);
    }

    public static void PlayKnightDevCard(GameState gs, Player player, Tile targetTile)
    {
        StandardPlayDevCardValidation(gs, player, DevelopmentCardType.Knight);

        if (targetTile.Id == gs.RobberTile.Id)
            throw new InvalidOperationException("Unexpected Error. The robber can not be moved to the tile it is already on.");

        player.PlayDevelopmentCard(DevelopmentCardType.Knight);
        gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.PlayKnight, targetTile));
        PlaceRobber(gs, player, targetTile);
        gs.UpdatePlayerVictoryPoints(player);
    }

    public static ResponseDTO PlayKnightDevCardFromUser(GameState gs, PlayDevCardRequest request)
    {
        if (request.TileId == null)
            return new ResponseDTO(false, 1041, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        if (!gs.Tiles.Any(t => t.Id == request.TileId))
            return new ResponseDTO(false, 1032, $"GameId: {gs.Id}; Player: {request.PlayerId}; TileId: {request.TileId}", null as GameStateDTO);

        var response = StandardPlayDevCardValidationForUserRequest(gs, request, DevelopmentCardType.Knight);

        if (!response.Success)
            return response;

        if (request.TileId == gs.RobberTile.Id)
            return new ResponseDTO(false, 1033, $"GameId: {gs.Id}; Player: {request.PlayerId}; TileId: {request.TileId}", null as GameStateDTO);

        var player = gs.Players.First(p => p.Id == request.PlayerId);
        var tile = gs.Tiles.First(t => t.Id == request.TileId);

        PlayKnightDevCard(gs, player, tile);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null, gs);
    }
}