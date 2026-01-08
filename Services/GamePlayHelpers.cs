using GameTest.DTOs;
using GameTest.Models;

namespace GameTest.Services;

public static class GamePlayHelpers
{
    public static int GetVictoryPointsForBuild(BuildingType? type)
    {
        if (type == BuildingType.Settlement)
            return 1;

        if (type == BuildingType.City)
            return 2;

        return 0;
    }

    public static Dictionary<ResourceType, int> GetResourcesEarnedOnVertex(GameState gs, Vertex vertex)
    {
        var resources = new Dictionary<ResourceType, int>();

        foreach (var tile in vertex.Tiles)
        {
            if (tile.Resource == ResourceType.Desert)
                continue;

            if (!resources.ContainsKey(tile.Resource))
                resources.Add(tile.Resource, 1);
            else
                resources[tile.Resource]++;    
        }

        return resources;
    }

    public static bool HasResourcesToBuildRoad(Player player)
    {
        return player.Resources.ContainsKey(ResourceType.Wood) && player.Resources[ResourceType.Wood] >= 1 &&
               player.Resources.ContainsKey(ResourceType.Brick) && player.Resources[ResourceType.Brick] >= 1;
    }

    public static bool HasResourcesToBuildSettlement(Player player)
    {
        return player.Resources.ContainsKey(ResourceType.Wood) && player.Resources[ResourceType.Wood] >= 1 &&
               player.Resources.ContainsKey(ResourceType.Brick) && player.Resources[ResourceType.Brick] >= 1 &&
               player.Resources.ContainsKey(ResourceType.Wool) && player.Resources[ResourceType.Wool] >= 1 &&
               player.Resources.ContainsKey(ResourceType.Grain) && player.Resources[ResourceType.Grain] >= 1;
    }

    public static bool HasResourcesToBuildCity(Player player)
    {
        return player.Resources.ContainsKey(ResourceType.Grain) && player.Resources[ResourceType.Grain] >= 2 &&
               player.Resources.ContainsKey(ResourceType.Ore) && player.Resources[ResourceType.Ore] >= 3;
    }

    public static bool HasResourcesToBuyDevCard(Player player)
    {
        return (player.Resources.ContainsKey(ResourceType.Ore) && player.Resources[ResourceType.Ore] >= 1 &&
            player.Resources.ContainsKey(ResourceType.Grain) && player.Resources[ResourceType.Grain] >= 1 &&
            player.Resources.ContainsKey(ResourceType.Wool) && player.Resources[ResourceType.Wool] >= 1);
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
        gameState.Phase.ClearDevCardPlayState();

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
        if (gameState.Players.Count < 2)
            throw new InvalidOperationException("Require at least 2 players to start a game.");

        if (gameState.Phase != null && gameState.Phase.PhaseState != GameStates.SettingUpBoard)
            throw new InvalidOperationException("Unexpected Error. Can not start a game that has already started.");

        if (gameState.Phase == null)
            throw new InvalidOperationException("Unexpected Error. The Phase in GameState isn't initialized.");

        gameState.Phase.PhaseState = GameStates.PlaceFirstSettlement;
        gameState.Phase.CurrentPlayer = gameState.Players[SharedHelpers.NextRandom(gameState.Players.Count)];
        gameState.Phase.EndPlayer = gameState.Phase.GetPreviousPlayer(gameState.Phase.CurrentPlayer, gameState.Players);

        // Log events to store current state
        gameState.AddEventRecord(new EventRecordDTO(gameState.Phase.CurrentPlayer, EventRecordAction.PlaceRobber, gameState.RobberTile));

        List<string> playerLineup = new();
        playerLineup.Add(gameState.Phase.CurrentPlayer.Id);
        var nextPlayer = gameState.Phase.GetNextPlayer(gameState.Phase.CurrentPlayer, gameState.Players);
        while (nextPlayer.Id != gameState.Phase.CurrentPlayer.Id)
        {
            playerLineup.Add(nextPlayer.Id);
            nextPlayer = gameState.Phase.GetNextPlayer(nextPlayer, gameState.Players);
        }
        gameState.AddEventRecord(new EventRecordDTO(gameState.Phase.CurrentPlayer, EventRecordAction.InitialSetUp, playerLineup));

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
        var preActionState = gs.GetPreActionStat();
        if (gs.Phase.PhaseState == GameStates.BuildOrTrade)
            gs.WithdrawResourcesToBuildRoad(player);

        if (gs.Phase.PhaseState == GameStates.PlaceSecondRoad)
        {
            var targetVertex =  FindSettlementWithNoRoads(gs, player);
            if (!edge.Vertices.Any(v => v.Id == targetVertex.Id))
            throw new InvalidOperationException($"Unexpected Error. Attempting to place second road away from second settlement. GameId: {gs.Id}; EdgeId: {edge.Id}; Player: {player.Id}");
        }

        edge.BuildRoad(player);
        var eventRecordId = gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.PlaceRoad, edge));
        gs.PushUndoState(preActionState, eventRecordId);

        if (gs.PlayerWithLongestRoad == null && gs.GetLongestRoadLength(player) > 4)
        {
            gs.AssignLongestRoadToPlayer(player);
            gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.GainedLongestRoad));
        }

        if (gs.PlayerWithLongestRoad != null && player.Id != gs.PlayerWithLongestRoad.Id && gs.GetLongestRoadLength(player) > gs.GetLongestRoadLength(gs.PlayerWithLongestRoad))
        {
            var previousPlayer = gs.PlayerWithLongestRoad;
            gs.AssignLongestRoadToPlayer(player);
            gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.GainedLongestRoad));
            gs.UpdatePlayerVictoryPoints(previousPlayer);
        }

        gs.UpdatePlayerVictoryPoints(player);
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

        if (!gs.UnusedRoadAvailable(player))
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

        return new ResponseDTO(true, 0, string.Empty, gs, player);
    }

    public static void BuildSettlement(GameState gs, Player player, Vertex vertex)
    {
        var preActionState = gs.GetPreActionStat();
        var resourcesGained = new Dictionary<ResourceType, int>();

        if (gs.Phase.PhaseState == GameStates.BuildOrTrade)
            gs.WithdrawResourcesToBuildSettlement(player);
        else if (gs.Phase.PhaseState == GameStates.PlaceSecondSettlement) 
            foreach (var tile in vertex.Tiles)
                if (tile.Resource != ResourceType.Desert)
                {
                    gs.AssignResourcesToPlayer(player, tile.Resource, 1);
                    if (!resourcesGained.ContainsKey(tile.Resource))
                        resourcesGained.Add(tile.Resource, 1);
                    else
                        resourcesGained[tile.Resource]++;
                }

        vertex.BuildSettlement(player);

        var eventRecordId = -1;
        if (gs.Phase.PhaseState == GameStates.PlaceFirstSettlement)
            eventRecordId = gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.PlaceFirstSettlement, vertex));
        else if (gs.Phase.PhaseState == GameStates.PlaceSecondSettlement)
            eventRecordId = gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.PlaceSecondSettlement, vertex, resourcesGained));
        else
            eventRecordId = gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.PlaceSettlement, vertex));

        gs.PushUndoState(preActionState, eventRecordId);
        MarkBlockedVertices(gs, vertex);
        PopulatePlayerPorts(gs);
        gs.UpdatePlayerVictoryPoints(player);
    }

    // TODO: Return a GameResult type that can indicate success/failure and include messages.
    // Right now, an empty string indicates success.
    public static ResponseDTO BuildSettlementRequestFromUser(GameState gs, string playerId, string vertexId)
    {
        if (!BuildSettlementPhase(gs) || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: BuildSettlement; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != playerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (!gs.UnusedSettlementAvailable(player))
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

        return new ResponseDTO(true, 0, string.Empty, gs, player);
    }

    public static void UpgradeToCity(GameState gs, Player player, Vertex vertex)
    {
        if (BuildCityPhase(gs))
        {
            var preActionState = gs.GetPreActionStat();
            gs.WithdrawResourcesToBuildCity(player);
            vertex.UpgradeToCity();
            var eventRecordId = gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.UpgradeSettlement, vertex));
            gs.UpdatePlayerVictoryPoints(player);
            gs.PushUndoState(preActionState, eventRecordId);
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

        if (!gs.UnusedCityAvailable(player))
            return new ResponseDTO(false, 1028, $"GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);
        if (vertex == null)
            return new ResponseDTO(false, 1014, $"GameId: {gs.Id}; VertexId: {vertexId}", null as GameStateDTO);

        if (vertex.Building == null || vertex.Owner == null)
            return new ResponseDTO(false, 1020, $"GameId: {gs.Id}; VertexId: {vertexId}", null as GameStateDTO);

        if (vertex.Building == BuildingType.City)
            return new ResponseDTO(false, 1019, $"GameId: {gs.Id}; VertexId: {vertexId}", null as GameStateDTO);

        if (vertex.Owner.Id != playerId)
            return new ResponseDTO(false, 1063, $"GameId: {gs.Id}; VertexId: {vertexId}; OwnerId: {vertex.Owner.Id}", null as GameStateDTO);

        if (!HasResourcesToBuildCity(player))
            return new ResponseDTO(false, 1017, $"Action: BuildCity; GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        UpgradeToCity(gs, player, vertex);

        GameLoop(gs);

        return new ResponseDTO(true, 0, string.Empty, gs, player);
    }

    private static void UpdateStatsOnGameOver(GameState gs)
    {
        gs.ClearUndoState();
        foreach(var player in gs.Players)
            player.SetVictoryPoints(player.FullVictoryPoints, player.FullVictoryPoints);
    }

    public static void GameLoop(GameState gs)
    {
        int loopCounter = 0; // Failsafe to prevent infinite loops

        var settlementCount =  gs.CountSettlementsForPlayer(gs.Phase.CurrentPlayer!);
        var playerRoadCount = gs.CountRoadsForPlayer(gs.Phase.CurrentPlayer!);
        var diceValue = gs.Dice.GetCombinedValue();

        gs.Phase = gs.Phase.GetNextPhase(gs.Players, settlementCount, playerRoadCount, diceValue, gs.RobberTile);

        if (gs.Phase.PhaseState == GameStates.GameOver)
        {
            UpdateStatsOnGameOver(gs);
            return;
        }

        if (gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException("Shouldn't call GameLoop before current player set.");

        if (gs.Phase.CurrentPlayer.IsBot)
        {
            var bot = new BotAI(gs);

            while (gs.Phase.CurrentPlayer != null && gs.Phase.CurrentPlayer.IsBot)
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
                else if (gs.Phase.PhaseState == GameStates.DiscardCards)
                {
                    move = bot.GetDiscardMove();
                }   
                else if (gs.Phase.PhaseState == GameStates.PlaceRobber)
                {
                    move = bot.GetRobberMove();
                }
                else if (gs.Phase.PhaseState == GameStates.SelectTarget)
                {
                    move = bot.SelectTargetMove();
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

                    if (move.VertexMove.Building == BuildingType.Settlement)
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

                if (move.DiscardResources != null && move.DiscardResources.Count > 0)
                    GamePlayHelpers.DiscardCards(gs, gs.Phase.CurrentPlayer, move.DiscardResources);

                if (move.SelectedPlayer != null && gs.Phase.PhaseState == GameStates.SelectTarget)
                    GamePlayHelpers.SelectTarget(gs, gs.Phase.CurrentPlayer, move.SelectedPlayer);

                settlementCount =  gs.CountSettlementsForPlayer(gs.Phase.CurrentPlayer!);
                playerRoadCount = gs.CountRoadsForPlayer(gs.Phase.CurrentPlayer!);
                diceValue = gs.Dice.GetCombinedValue();

                gs.Phase = gs.Phase.GetNextPhase(gs.Players, settlementCount, playerRoadCount, diceValue, gs.RobberTile);

                if (gs.Phase.PhaseState == GameStates.GameOver)
                {
                    UpdateStatsOnGameOver(gs);
                    return;
                }
            }
        }
    }

    public static void RollDice(GameState gs, bool skipGameLoop = false)
    {
        if (gs.Phase.PhaseState != GameStates.RollOrUseDevCard || gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException($"Unexpected Exception. Roll called when game in {gs.Phase.PhaseState}. Player: {gs.Phase.CurrentPlayer}.");

        gs.ClearUndoState();
        gs.Dice.Roll();
        gs.Phase.ClearWaitingForRoll();
        gs.AddEventRecord(new EventRecordDTO(gs.Phase.CurrentPlayer, EventRecordAction.RollDice, gs.Dice));
        gs.AssignResourcesBasedOnLastDiceRoll();

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
        if (dto == null)
            throw new ArgumentNullException("Unexpected Error. GameStateDTO should not be null.");

        var gs = new GameState(dto);

        if (gs.Phase.PhaseState != GameStates.GameOver)
        {
            BoardCreationHelpers.LinkEdgesAndVertices(gs);
            MarkBlockedVertices(gs);
            PopulatePlayerPorts(gs);
            foreach(var player in gs.Players)
                gs.UpdatePlayerVictoryPoints(player);
        }

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
        var preActionState = gs.GetPreActionStat();
        var response = gs.TradeWithBank(gs, request.Player, request.Offer, request.Request);

        if (!response.Success)
            return response;

        var eventRecordId = gs.AddEventRecord(new EventRecordDTO(request.Player, EventRecordAction.TradeWithBank, request.Request, request.Offer));
        gs.PushUndoState(preActionState, eventRecordId);

        return new ResponseDTO(true, 0, string.Empty, gs, request.Player);
    }
    
    public static ResponseDTO BankTradeFromUser(GameState gs, TradeRequestDTO request)
    {
        if (gs.Phase.PhaseState != GameStates.BuildOrTrade || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: BankTrade; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != request.PlayerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        
        return BankTrade(gs, new TradeRequest(gs, request));
    }

    public static void PopulatePlayerPorts(GameState gs)
    {
        foreach(var port in gs.Ports)
            foreach(var vertex in port.Vertices)
                if (vertex.Owner != null)
                    vertex.Owner.AddPort(port.Type);
    }

    private static void StealResource(GameState gs, Player player, Tile tile)
    {
        gs.ClearUndoState();
        if (gs.Phase.TargetPlayers != null && gs.Phase.TargetPlayers.Count != 1)
            throw new InvalidOperationException("StealResources should only be called after a target player has been selected");

        // Look through all vertices and locate the ones that are on the specified tile
        foreach(var vertex in gs.Vertices)
        {
            // If TargetPlayers is null, steal from a building not owned by the player. If
            // TargetPlayers is set, steal from that player.
            if (vertex.Tiles.Any(t => t.Id == tile.Id) && vertex.Owner != null &&
                ((gs.Phase.TargetPlayers == null && vertex.Owner.Id != player.Id) ||
                (gs.Phase.TargetPlayers != null && vertex.Owner.Id == gs.Phase.TargetPlayers.First().Id)))
            {
                List<ResourceType> targetResources = new List<ResourceType>();
                foreach (var pair in vertex.Owner.Resources)
                    for (int i = 0; i < pair.Value; i++ )
                        targetResources.Add(pair.Key);

                if (targetResources.Count > 0)
                {
                    var resourceToSteal = targetResources[SharedHelpers.NextRandom(targetResources.Count)];
                    vertex.Owner.RemoveResources(resourceToSteal, 1);
                    player.AssignResources(resourceToSteal, 1);

                    gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.StealResource, vertex.Owner, resourceToSteal));
                    break;
                }
            }
        }
    }

    private static List<Player> GetOpponentsOnTile(GameState gs, Player player, Tile tile)
    {
        HashSet<Player> opponents = new();

        foreach(var vertex in gs.Vertices)
            if (vertex.Tiles.Any(t => t.Id == tile.Id) && vertex.Owner != null && vertex.Owner.Id != player.Id)
                opponents.Add(vertex.Owner);

        return opponents.ToList();
    }

    public static void PlaceRobber(GameState gs, Player player, Tile tile)
    {
        gs.ClearUndoState();
        gs.Phase.SetTargetPlayers(GetOpponentsOnTile(gs, player, tile));
        gs.SetRobberTile(tile);
        gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.PlaceRobber, tile));

        if (gs.Phase.TargetPlayers == null || gs.Phase.TargetPlayers.Count == 1)
            StealResource(gs, player, tile);
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
        
        if (gs.Phase.OriginalRobberTile == null)
            return new ResponseDTO(false, 9999, "OriginalRobberTile shouldn't be null.", null as GameStateDTO);
        
        if (gs.Phase.OriginalRobberTile.Id == tile.Id)
            return new ResponseDTO(false, 1033, $"GameId: {gs.Id}; Player: {player.Id}; OriginalTile: {gs.Phase.OriginalRobberTile.Id}; NewTile: {tile.Id}", null as GameStateDTO);

        PlaceRobber(gs, player, tile);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null!, gs, player);
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

        gs.ClearUndoState();
        player.AssignDevelopmentCard(gs.DevelopmentCards[0]);
        gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.BuyDevelopmentCard, gs.DevelopmentCards[0]));
        gs.DevelopmentCards.RemoveAt(0);
        gs.WithdrawResourcesToBuyDevCard(player);
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

        return new ResponseDTO(true, 0, null!, gs, player);
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

        if (gs.Phase.DevCardPlayedThisRound)
            throw new InvalidOperationException($"Unexpected Error. Players shouldn't be allowed to play two dev cards in a single round.");
    }

    private static ResponseDTO StandardPlayDevCardValidationForUserRequest(GameState gs, PlayDevCardRequest request, DevelopmentCardType targetType)
    {
        if ((gs.Phase.PhaseState != GameStates.RollOrUseDevCard && gs.Phase.PhaseState != GameStates.BuildOrTrade) || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: Play{targetType}DevCard; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (request.DevCardType != targetType)
            return new ResponseDTO(false, 9999, $"Requested: {request.DevCardType}; Called: {targetType}; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != request.PlayerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (request.SelectedResources != null)
        {
            foreach (var resource in request.SelectedResources)
            {
                if (resource == ResourceType.Desert)
                    return new ResponseDTO(false, 1038, $"Action: Play{targetType}DevCard; RequestedResource: {resource}; GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);
            }
        }

        if (!player.DevCardsReadyToPlay.Contains(targetType))
            return new ResponseDTO(false, 1039, $"Action: Play{targetType}DevCard; GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        if (gs.Phase.DevCardPlayedThisRound)
            return new ResponseDTO(false, 1043, $"Action: Play{targetType}DevCard; GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        return new ResponseDTO(true, 0, null!, gs, player);
    }

    public static void SharedPlayDevCard(GameState gs, Player player, DevelopmentCardType type)
    {
        player.PlayDevelopmentCard(type);
        gs.Phase.SetDevCardPlayedThisRound();
    }

    public static void PlayMonopolyDevCard(GameState gs, Player player, ResourceType requestedResource)
    {
        gs.ClearUndoState();
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

        SharedPlayDevCard(gs, player, DevelopmentCardType.Monopoly);
        gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.PlayMonopoly, new Dictionary<ResourceType, int>() { {requestedResource, resourcesReceived} }));
    }

    public static ResponseDTO PlayMonopolyDevCardFromUser(GameState gs, PlayDevCardRequest request)
    {
        if (request.SelectedResources == null || request.SelectedResources.Count != 1)
            return new ResponseDTO(false, 1037, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        var response = StandardPlayDevCardValidationForUserRequest(gs, request, DevelopmentCardType.Monopoly);

        if (!response.Success)
            return response;

        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"Player: {request.PlayerId}", null as GameState);

        var requestedResource = request.SelectedResources[0];

        PlayMonopolyDevCard(gs, player, requestedResource);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null!, gs, player);
    }

    public static void PlayYearOfPlentyDevCard(GameState gs, Player player, List<ResourceType> requestedResources)
    {
        StandardPlayDevCardValidation(gs, player, DevelopmentCardType.YearOfPlenty);

        if (requestedResources.Count != 2)
            throw new InvalidOperationException($"Unexpected Error. Caller didn't request two resources.");

        foreach(var resource in requestedResources)
            if (resource == ResourceType.Desert)
                throw new InvalidOperationException($"Unexpected Error. Desert is not a valid resource to request in PlayYearOfPlentyDevCard.");

        var preActionState = gs.GetPreActionStat();

        foreach(var resource in requestedResources)
            gs.AssignResourcesToPlayer(player, resource, 1);

        var eventRecordId = -1;
        SharedPlayDevCard(gs, player, DevelopmentCardType.YearOfPlenty);
        if (requestedResources[0] == requestedResources[1])
            eventRecordId = gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.PlayYearOfPlenty, new Dictionary<ResourceType, int>() { {requestedResources[0], 2} }));
        else
            eventRecordId = gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.PlayYearOfPlenty, new Dictionary<ResourceType, int>() { {requestedResources[0], 1}, {requestedResources[1], 1} }));

        gs.PushUndoState(preActionState, eventRecordId);
    }

    public static ResponseDTO PlayYearOfPlentyDevCardFromUser(GameState gs, PlayDevCardRequest request)
    {
        if (request.SelectedResources == null || request.SelectedResources.Count != 2)
            return new ResponseDTO(false, 1040, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        var response = StandardPlayDevCardValidationForUserRequest(gs, request, DevelopmentCardType.YearOfPlenty);

        if (!response.Success)
            return response;

        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"Player: {request.PlayerId}", null as GameState);

        PlayYearOfPlentyDevCard(gs, player, request.SelectedResources);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null!, gs, player);
    }

    public static void PlayRoadBuildingDevCard(GameState gs, Player player)
    {
        StandardPlayDevCardValidation(gs, player, DevelopmentCardType.RoadBuilding);

        var preActionState = gs.GetPreActionStat();

        gs.Phase.StoreStateDevCardRoadBuilding(gs.Phase.PhaseState, gs.Edges.Count(e => e.Owner != null && e.Owner.Id == player.Id));
        gs.Phase.PhaseState = GameStates.FirstDevCardRoad;

        SharedPlayDevCard(gs, player, DevelopmentCardType.RoadBuilding);
        var eventRecordId = gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.PlayRoadBuilding));
        gs.PushUndoState(preActionState, eventRecordId);
    }

    public static ResponseDTO PlayRoadBuildingDevCardFromUser(GameState gs, PlayDevCardRequest request)
    {
        var response = StandardPlayDevCardValidationForUserRequest(gs, request, DevelopmentCardType.RoadBuilding);

        if (!response.Success)
            return response;

        var player = gs.Players.First(p => p.Id == request.PlayerId);

        PlayRoadBuildingDevCard(gs, player);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null!, gs, player);
    }

    public static void PlayKnightDevCard(GameState gs, Player player, Tile targetTile)
    {
        StandardPlayDevCardValidation(gs, player, DevelopmentCardType.Knight);

        var origRobberLocation = gs.RobberTile;

        if (targetTile.Id == gs.RobberTile.Id)
            throw new InvalidOperationException("Unexpected Error. The robber can not be moved to the tile it is already on.");

        SharedPlayDevCard(gs, player, DevelopmentCardType.Knight);
        gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.PlayKnight, targetTile));
        PlaceRobber(gs, player, targetTile);

        if (gs.PlayerWithLargestArmy == null && player.CountPlayedKnights() > 2) 
        {
            gs.AssignLargestArmyToPlayer(player);
            gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.GainedLargestArmy));
        }

        if (gs.PlayerWithLargestArmy != null && player.CountPlayedKnights() > gs.PlayerWithLargestArmy.CountPlayedKnights())
        {
            var otherPlayer = gs.PlayerWithLargestArmy;
            gs.AssignLargestArmyToPlayer(player);
            gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.GainedLargestArmy));
            gs.UpdatePlayerVictoryPoints(otherPlayer);
        }

        gs.UpdatePlayerVictoryPoints(player);

        // TODO: I don't like setting the state outside of GetNextState. Is there
        // a cleaner way to do this?
        if (gs.Phase.TargetPlayers != null && gs.Phase.TargetPlayers.Count > 1)
        {
            gs.Phase.SetStateToReturnTo(gs.Phase.PhaseState, origRobberLocation);
            gs.Phase.PhaseState = GameStates.PlaceRobber;
        }
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

        return new ResponseDTO(true, 0, null!, gs, player);
    }

    public static void DiscardCards(GameState gs, Player player, List<ResourceType> cardsToDiscard)
    {
        if (player.ResourceCount <= 7)
            throw new InvalidOperationException($"Unexpected Error. Player doen't have enough cards to be included in discarding cards. GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}");

        if ((gs.Phase.PhaseState != GameStates.DiscardCards) || gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException($"Unexpected Error. Invalid state for calling DiscardCards. GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}");

        if (gs.Phase.CurrentPlayer.Id != player.Id)
            throw new InvalidOperationException($"Unexpected Error. It isn't the player's turn. GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}");

        if (cardsToDiscard.Count != player.ResourceCount / 2)
            throw new InvalidOperationException($"Unexpected Error. The calling is trying to discard the wrong number of cards. GameId: {gs.Id}; Player: {player.Id}; TotalCards: {player.ResourceCount}; DiscardCount: {cardsToDiscard.Count}");

        var preActionState = gs.GetPreActionStat();

        foreach (var resource in cardsToDiscard)
        {
            if (player.Resources[resource] <= 0)
                throw new InvalidOperationException($"Unexpected Error: Player doesn't have a resource they are trying to discard. MissingResource: {resource}; GameId: {gs.Id}; Player: {player.Id}");

            gs.RemoveResourcesFromPlayer(player, resource, 1);
        }

        var eventRecordId = gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.DiscardCards, cardsToDiscard));
        gs.PushUndoState(preActionState, eventRecordId);
    }

    public static ResponseDTO DiscardCardRequestFromUser(GameState gs, DiscardRequest request)
    {
        if ((gs.Phase.PhaseState != GameStates.DiscardCards) || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: DiscardCards; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != request.PlayerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (request.SelectedResources == null || request.SelectedResources.Count == 0)
            return new ResponseDTO(false, 1044, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        var totalCards = player.Resources.Values.Sum();
        if (request.SelectedResources.Count != totalCards / 2)
            return new ResponseDTO(false, 1045, $"GameId: {gs.Id}; Player: {request.PlayerId}; TotalCards: {totalCards}; RequestedDiscardCount: {request.SelectedResources.Count}", null as GameStateDTO);

        var ownedResourcesAsList = AIHelpers.ConvertResourceDictToList(player.Resources);
        foreach (var resource in request.SelectedResources)
        {
            if (!ownedResourcesAsList.Contains(resource))
                return new ResponseDTO(false, 1017, $"Action: DiscardCards; MissingResource: {resource}; GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

            ownedResourcesAsList.Remove(resource);
        }

        DiscardCards(gs, player, request.SelectedResources);
        GameLoop(gs);

        // Return possible actions for the new current player (after GameLoop, current player may have changed)
        return new ResponseDTO(true, 0, null!, gs, gs.Phase.CurrentPlayer);
    }

    public static void OpenTrade(GameState gs, Player player, Dictionary<ResourceType, int> offer,Dictionary<ResourceType, int> request)
    {
        if ((gs.Phase.PhaseState != GameStates.BuildOrTrade) || gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException("Unexpected Error. Invalid GameState for Open Trade. State: {gs.Phase.PhaseState}");

        if (gs.Phase.CurrentPlayer.Id != player.Id)
            throw new InvalidOperationException($"Unexpected Error. It is not the identified player's turn. PlayerTurn: {gs.Phase.CurrentPlayer}; ActingPlayer: {player}");

        if (offer == null || offer.Count == 0)
            throw new InvalidOperationException($"Unexpected Error. Trade requested with missing resources offered.");

        if (request == null || request.Count == 0)
            throw new InvalidOperationException($"Unexpected Error. Trade requested with missing requested resources.");

        gs.ClearUndoState();
        var offerAsList = AIHelpers.ConvertResourceDictToList(offer);
        var requestAsList = AIHelpers.ConvertResourceDictToList(request);
        if (offerAsList.Count == requestAsList.Count && AIHelpers.MultiSetSubtraction(offerAsList, requestAsList).Count == 0)
            throw new InvalidOperationException($"Unexpected Error. Trade requested where offer matches request.");

        var missingResources = AIHelpers.MultiSetSubtraction(offerAsList, AIHelpers.ConvertResourceDictToList(player.Resources));
        if (missingResources.Count > 0)
            throw new InvalidOperationException($"Unexpected Error. Player doesn't have resources to cover their offer. ResourceMissing: {missingResources[0].ToString()}");

        gs.Phase.AddPendingTradeResponse(new TradeResponse(player, TradeResponseType.Original, offer, request));
        gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.OfferToTrade, request, offer, null!));
    }

    public static ResponseDTO OpenTradeFromUser(GameState gs, TradeRequestDTO request)
    {
        if ((gs.Phase.PhaseState != GameStates.BuildOrTrade) || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: OpenTrade; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != request.PlayerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (request.Offer == null || request.Offer.Count == 0)
            return new ResponseDTO(false, 1048, $"GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; NoCardsOffered", null as GameStateDTO);

        if (request.Request == null || request.Request.Count == 0)
            return new ResponseDTO(false, 1048, $"GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; NoCardsRequested", null as GameStateDTO);

        var offerAsList = AIHelpers.ConvertResourceDictToList(request.Offer);
        var requestAsList = AIHelpers.ConvertResourceDictToList(request.Request);
        if (offerAsList.Count == requestAsList.Count && AIHelpers.MultiSetSubtraction(offerAsList, requestAsList).Count == 0)
            return new ResponseDTO(false, 1049,  $"GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}", null as GameStateDTO);

        var missingResources = AIHelpers.MultiSetSubtraction(offerAsList, AIHelpers.ConvertResourceDictToList(player.Resources));
        if (missingResources.Count > 0)
            return new ResponseDTO(false, 1006, $"GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; ResourceMissing: {missingResources[0].ToString()}", null as GameStateDTO);

        OpenTrade(gs, player, request.Offer, request.Request);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null!, gs, player);
    }

    public static void RespondToTrade(GameState gs, Player player, TradeResponseDTO response)
    {
        if ((gs.Phase.PhaseState != GameStates.RespondToTrade) || gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException("Unexpected Error. Invalid GameState for Respond To Trade. State: {gs.Phase.PhaseState}");

        if (gs.Phase.CurrentPlayer.Id == response.PlayerId)
            throw new InvalidOperationException($"Unexpected Error. Player should not be responding to their own trade request. PlayerTurn: {gs.Phase.CurrentPlayer}; ActingPlayer: {player}");

        if (response.ResponseType == TradeResponseType.Original)
            throw new InvalidOperationException($"Unexpected Error. Trade responses should not have a type of Original");

        if (response.ResponseType == TradeResponseType.Accept)
        {
            if (gs.Phase.PendingTradeResponses == null)
                throw new InvalidOperationException("Unexpected Error. Trade response received when pending trade responses is null.");

            var pendingResponse = gs.Phase.PendingTradeResponses.FirstOrDefault(p => p.ResponseType == TradeResponseType.Original);

            if (pendingResponse == null || pendingResponse.Offer == null)
                throw new InvalidOperationException("Unexpected Error. Trade response received when no original offers present.");

            var requestAsList = AIHelpers.ConvertResourceDictToList(pendingResponse.Offer);
            var playerResources = AIHelpers.ConvertResourceDictToList(gs.Phase.CurrentPlayer.Resources);
            if (AIHelpers.MultiSetSubtraction(requestAsList, playerResources).Count > 0)
                throw new InvalidOperationException("Unexpected Error. Player accepting a trade they don't have the resources to fulfill.");
        }

        if (response.ResponseType == TradeResponseType.Counter)
        {
            if (response.Offer == null || response.Offer.Count == 0)
                throw new InvalidOperationException($"Unexpected Error. Trade response counter offer is missing resources offered.");

            if (response.Request == null || response.Request.Count == 0)
                throw new InvalidOperationException($"Unexpected Error. Trade response counter is missing requested resources.");

            gs.ClearUndoState();
            var offerAsList = AIHelpers.ConvertResourceDictToList(response.Offer);
            var requestAsList = AIHelpers.ConvertResourceDictToList(response.Request);
            if (offerAsList.Count == requestAsList.Count && AIHelpers.MultiSetSubtraction(offerAsList, requestAsList).Count == 0)
                throw new InvalidOperationException($"Unexpected Error. Trade response counter offer where offer matches request.");

            var missingResources = AIHelpers.MultiSetSubtraction(offerAsList, AIHelpers.ConvertResourceDictToList(player.Resources));
            if (missingResources.Count > 0)
                throw new InvalidOperationException($"Unexpected Error. Player doesn't have resources to cover their counter offer. ResourceMissing: {missingResources[0].ToString()}");

            gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.CounterOffer, response.Request, response.Offer, null!));
        }
        else
            if (response.ResponseType == TradeResponseType.Accept)
                gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.AcceptTrade));
            else
                gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.RejectTrade));

        gs.Phase.AddPendingTradeResponse(new TradeResponse(player, response.ResponseType, response.Offer, response.Request));
    }


    public static ResponseDTO RespondToTradeFromUser(GameState gs, TradeResponseDTO response)
    {
        if ((gs.Phase.PhaseState != GameStates.RespondToTrade) || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: RespondToTrade; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == response.PlayerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {response.PlayerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id == response.PlayerId)
            return new ResponseDTO(false, 1050, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; TradeResponsePlayer: {player}", null as GameStateDTO);

        if (response.ResponseType == TradeResponseType.Original)
            return new ResponseDTO(false, 1051, $"GameId: {gs.Id}; TradeResponsePlayer: {player}; ResponseType: {response.ResponseType.ToString()}", null as GameStateDTO);

        if (response.ResponseType == TradeResponseType.Accept)
        {
            var pendingResponse = gs.Phase.PendingTradeResponses.FirstOrDefault(p => p.ResponseType == TradeResponseType.Original);

            if (pendingResponse == null || pendingResponse.Offer == null)
                return new ResponseDTO(false, 9999, $"Original trade not found.", null as GameStateDTO);

            var requestAsList = AIHelpers.ConvertResourceDictToList(pendingResponse.Offer);
            var playerResources = AIHelpers.ConvertResourceDictToList(gs.Phase.CurrentPlayer.Resources);
            if (AIHelpers.MultiSetSubtraction(requestAsList, playerResources).Count > 0)
                return new ResponseDTO(false, 1017, $"GameId: {gs.Id}; TradeResponsePlayer: {player}", null as GameStateDTO);
        }

        if (response.ResponseType == TradeResponseType.Counter)
        {
            if (response.Offer == null || response.Offer.Count == 0)
                return new ResponseDTO(false, 1052, $"GameId: {gs.Id}; TradeResponsePlayer: {player}; NoCardsOffered", null as GameStateDTO);

            if (response.Request == null || response.Request.Count == 0)
                return new ResponseDTO(false, 1052, $"GameId: {gs.Id}; TradeResponsePlayer: {player}; NoCardsRequested", null as GameStateDTO);

            var offerAsList = AIHelpers.ConvertResourceDictToList(response.Offer);
            var requestAsList = AIHelpers.ConvertResourceDictToList(response.Request);
            if (offerAsList.Count == requestAsList.Count && AIHelpers.MultiSetSubtraction(offerAsList, requestAsList).Count == 0)
                return new ResponseDTO(false, 1049,  $"GameId: {gs.Id}; TradeResponsePlayer: {player}", null as GameStateDTO);

            var missingResources = AIHelpers.MultiSetSubtraction(offerAsList, AIHelpers.ConvertResourceDictToList(player.Resources));
            if (missingResources.Count > 0)
                return new ResponseDTO(false, 1006, $"GameId: {gs.Id}; TradeResponsePlayer: {player}; ResourceMissing: {missingResources[0].ToString()}", null as GameStateDTO);
        }

        RespondToTrade(gs, player, response);
        GameLoop(gs);

        // Return possible actions for current player (the trade opener, not the responder)
        return new ResponseDTO(true, 0, null!, gs, gs.Phase.CurrentPlayer);
    }

    public static void AcceptTrade(GameState gs, Player player, Player acceptedPlayer)
    {
        if ((gs.Phase.PhaseState != GameStates.RespondToTrade) || gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException($"Unexpected Error. Invalid state for AcceptTrade. Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}");

        if (gs.Phase.CurrentPlayer.Id != player.Id)
            throw new InvalidOperationException($"Unexpected Error. It isn't the player's turn. PlayerTurn: {gs.Phase.CurrentPlayer}");

        if (player.Id == acceptedPlayer.Id)
            throw new InvalidOperationException($"Unexpected Error. Player trying to accept their own trade. PlayerTurn: {gs.Phase.CurrentPlayer}; AcceptedPlayer: {acceptedPlayer.Id}");

        if (gs.Phase == null || gs.Phase.PendingTradeResponses == null)
            throw new InvalidOperationException($"Unexpected Error. PendingTradeResponses missing.");

        var response = gs.Phase.PendingTradeResponses.FirstOrDefault(t => t.Player.Id == acceptedPlayer.Id);
        if (response == null)
            throw new InvalidOperationException($"Unexpected Error. Trying to accept offer that wasn't made. AcceptedPlayer: {acceptedPlayer.Id}");

        if (response.ResponseType == TradeResponseType.Reject)
            throw new InvalidOperationException($"Unexpected Error. Trying to accept rejection to trade. AcceptedPlayer: {acceptedPlayer.Id}");

        if (response.ResponseType == TradeResponseType.Counter && response.Request != null)
        {
            var requestAsList = AIHelpers.ConvertResourceDictToList(response.Request);
            var missingResources = AIHelpers.MultiSetSubtraction(requestAsList, AIHelpers.ConvertResourceDictToList(player.Resources));
            if (missingResources.Count > 0)
                throw new InvalidOperationException($"Unexpected Error. Trying to accept a counter offer that can't be met. Player: {player}; ResourceMissing: {missingResources[0].ToString()}");
        }

        gs.ClearUndoState();
        var offer = new Dictionary<ResourceType, int>();
        var request = new Dictionary<ResourceType, int>();

        if (response.ResponseType == TradeResponseType.Accept)
        {
            var trade = gs.Phase.PendingTradeResponses.FirstOrDefault(t => t.Player.Id == player.Id && t.ResponseType == TradeResponseType.Original);
            if (trade == null || trade.Offer == null || trade.Request == null)
                throw new InvalidOperationException($"Unexpected Error. Couldn't locate original trade request details.");

            foreach(var kvp in trade.Offer)
                offer.Add(kvp.Key, kvp.Value);
            
            foreach(var kvp in trade.Request)
                request.Add(kvp.Key, kvp.Value);
        }
        else if (response.ResponseType == TradeResponseType.Counter)
        {
            if (response.Offer == null || response.Request == null)
                throw new InvalidOperationException($"Unexpected Error. Counter offer is incomplete.");

            foreach(var kvp in response.Offer)
                request.Add(kvp.Key, kvp.Value);
            
            foreach(var kvp in response.Request)
                offer.Add(kvp.Key, kvp.Value);
        }
        else
            throw new InvalidOperationException($"Unexpected Error. Accepted unsupported trade type ${response.ResponseType.ToString()}.");

        foreach(var kvp in offer)
        {
            player.RemoveResources(kvp.Key, kvp.Value);
            acceptedPlayer.AssignResources(kvp.Key, kvp.Value);
        }

        foreach(var kvp in request)
        {
            player.AssignResources(kvp.Key, kvp.Value);
            acceptedPlayer.RemoveResources(kvp.Key, kvp.Value);
        }

        gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.TradeWithPlayer, request, offer, acceptedPlayer));
        gs.Phase.ClearPendingTradeResponses();
    }

    public static ResponseDTO AcceptTradeFromUser(GameState gs, AcceptTradeDTO request)
    {
        if ((gs.Phase.PhaseState != GameStates.RespondToTrade) || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: AcceptTrade; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != request.PlayerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        if (request.PlayerId == request.AcceptedPlayerId)
            return new ResponseDTO(false, 1053, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; AcceptedPlayer: {request.AcceptedPlayerId}", null as GameStateDTO);

        var acceptedPlayer = gs.Players.FirstOrDefault(p => p.Id == request.AcceptedPlayerId);
        if (acceptedPlayer == null)
            return new ResponseDTO(false, 1056, $"GameId: {gs.Id}; Player: {request.PlayerId}; AcceptedPlayer: {request.AcceptedPlayerId}", null as GameStateDTO);

        if (gs.Phase == null || gs.Phase.PendingTradeResponses == null)
            return new ResponseDTO(false, 9999, "Action: AcceptTrade; GameId: {gs.Id}; PendingTradeResponses missing.", null as GameStateDTO);

        var response = gs.Phase.PendingTradeResponses.FirstOrDefault(t => t.Player.Id == request.AcceptedPlayerId);
        if (response == null)
            return new ResponseDTO(false, 1054, $"GameId: {gs.Id}; Player: {request.PlayerId}; AcceptedPlayer: {request.AcceptedPlayerId}", null as GameStateDTO);

        if (response.ResponseType == TradeResponseType.Reject)
            return new ResponseDTO(false, 1055, $"GameId: {gs.Id}; Player: {request.PlayerId}; AcceptedPlayer: {request.AcceptedPlayerId}", null as GameStateDTO);

        if (response.ResponseType == TradeResponseType.Counter && response.Request != null)
        {
            var requestAsList = AIHelpers.ConvertResourceDictToList(response.Request);
            var missingResources = AIHelpers.MultiSetSubtraction(requestAsList, AIHelpers.ConvertResourceDictToList(player.Resources));
            if (missingResources.Count > 0)
                return new ResponseDTO(false, 1017, $"GameId: {gs.Id}; TradeResponsePlayer: {player}; ResourceMissing: {missingResources[0].ToString()}", null as GameStateDTO);
        }

        AcceptTrade(gs, player, acceptedPlayer);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null!, gs, player);
    }

    public static void RejectAllOffers(GameState gs, Player player)
    {
        if ((gs.Phase.PhaseState != GameStates.RespondToTrade) || gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException($"Unexpected Error. Invalid state for RejectAllOffers. Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}");

        if (gs.Phase.CurrentPlayer.Id != player.Id)
            throw new InvalidOperationException($"Unexpected Error. It isn't the player's turn. PlayerTurn: {gs.Phase.CurrentPlayer}");

        gs.ClearUndoState();
        gs.AddEventRecord(new EventRecordDTO(player, EventRecordAction.RejectTrade));
        gs.Phase.ClearPendingTradeResponses();
    }

    public static ResponseDTO RejectAllOffersFromUser(GameState gs, BaseRequest request)
    {
        if ((gs.Phase.PhaseState != GameStates.RespondToTrade) || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: RejectAllOffers; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != request.PlayerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        RejectAllOffers(gs, player);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null!, gs, player);
    }

    private static readonly System.Text.RegularExpressions.Regex ValidNamePattern = new(@"^[a-zA-Z0-9 ]+$");

    public static ResponseDTO AddPlayerToGame(GameState gs, AddPlayerRequest request)
    {
        // Validate game state
        if (gs.Phase.PhaseState != GameStates.SettingUpBoard)
            return new ResponseDTO(false, 1003, $"Action: AddPlayer; GameId: {gs.Id}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        // Validate max players
        if (gs.Players.Count >= gs.Settings.MaxPlayers)
            return new ResponseDTO(false, 1060, $"GameId: {gs.Id}; CurrentPlayers: {gs.Players.Count}; MaxPlayers: {gs.Settings.MaxPlayers}", null as GameStateDTO);

        // Get used names and colors
        var usedNames = gs.Players.Select(p => p.Name.ToLowerInvariant()).ToHashSet();
        var usedColors = gs.Players.Select(p => p.Color).ToHashSet();

        // Validate/generate name
        string playerName;
        if (request.IsBot && string.IsNullOrWhiteSpace(request.PlayerName))
        {
            // Auto-generate bot name
            int botNumber = 1;
            while (usedNames.Contains($"bot {botNumber}"))
                botNumber++;
            playerName = $"Bot {botNumber}";
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.PlayerName))
                return new ResponseDTO(false, 1061, $"GameId: {gs.Id}; PlayerName: (empty)", null as GameStateDTO);

            playerName = request.PlayerName.Trim();

            if (playerName.Length == 0)
                return new ResponseDTO(false, 1061, $"GameId: {gs.Id}; PlayerName: (empty after trim)", null as GameStateDTO);

            if (playerName.Length > 15)
                return new ResponseDTO(false, 1061, $"GameId: {gs.Id}; PlayerName: {playerName}; Must be 15 characters or less", null as GameStateDTO);

            if (!ValidNamePattern.IsMatch(playerName))
                return new ResponseDTO(false, 1061, $"GameId: {gs.Id}; PlayerName: {playerName}; Reason: invalid characters", null as GameStateDTO);

            if (usedNames.Contains(playerName.ToLowerInvariant()))
                return new ResponseDTO(false, 1058, $"GameId: {gs.Id}; PlayerName: {playerName}", null as GameStateDTO);
        }

        // Validate/generate color
        PlayerColor playerColor;
        if (request.PreferredColor.HasValue)
        {
            if (!Enum.IsDefined(typeof(PlayerColor), request.PreferredColor.Value))
                return new ResponseDTO(false, 1062, $"GameId: {gs.Id}; Color: {request.PreferredColor.Value}", null as GameStateDTO);

            if (usedColors.Contains(request.PreferredColor.Value))
                return new ResponseDTO(false, 1059, $"GameId: {gs.Id}; Color: {request.PreferredColor.Value}", null as GameStateDTO);

            playerColor = request.PreferredColor.Value;
        }
        else if (request.IsBot)
        {
            // Auto-assign first available color for bot
            var availableColor = Enum.GetValues<PlayerColor>().FirstOrDefault(c => !usedColors.Contains(c));
            playerColor = availableColor;
        }
        else
        {
            return new ResponseDTO(false, 1062, $"GameId: {gs.Id}; Color: (not specified for human player)", null as GameStateDTO);
        }

        // Create and add player
        gs.Players.Add(new Player(gs.GetNewPlayerId(), playerName, playerColor, request.IsBot));

        return new ResponseDTO(true, 0, string.Empty, gs);
    }

    public static List<Player> GetOpponentsOnTile(GameState gs, Tile tile)
    {
        HashSet<Player> players = new();

        foreach(var vertex in gs.Vertices.Where(v => v.Owner != null && v.Owner.Id != gs.Phase.CurrentPlayer.Id && v.Tiles.Any(t => t.Id == tile.Id)))
            players.Add(vertex.Owner);

        return players.ToList();
    }

    public static void SelectTarget(GameState gs, Player player, Player targetPlayer)
    {
        if (gs.Phase.PhaseState != GameStates.SelectTarget || gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException($"Unexpected Error. SelectTarget not valid in {gs.Phase.PhaseState} state.");

        if (gs.Phase.CurrentPlayer.Id != player.Id)
            throw new InvalidOperationException($"Unexpected Error. It is not player {player.Id}'s turn.");

        if (gs.Phase.TargetPlayers == null || !gs.Phase.TargetPlayers.Any(p => p.Id == targetPlayer.Id))
            throw new InvalidOperationException($"Unexpected Error. Player {targetPlayer.Id} is not a valid target.");

        gs.ClearUndoState();
        gs.Phase.SetTargetPlayer(targetPlayer);
        StealResource(gs, player, gs.RobberTile);
    }

    public static ResponseDTO SelectTargetFromUser(GameState gs, SelectTargetRequest request)
    {

        if (gs.Phase.PhaseState != GameStates.SelectTarget || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: SelectTarget; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != request.PlayerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var targetPlayer = gs.Players.FirstOrDefault(p => p.Id == request.TargetPlayerId);
        if (targetPlayer == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; TargetPlayer: {request.TargetPlayerId}", null as GameStateDTO);

        if (gs.Phase.TargetPlayers == null || !gs.Phase.TargetPlayers.Any(p => p.Id == request.TargetPlayerId))
            return new ResponseDTO(false, 1064, $"GameId: {gs.Id}; Player: {request.PlayerId}; TargetPlayer: {request.TargetPlayerId}", null as GameStateDTO);

        SelectTarget(gs, player, targetPlayer);
        GameLoop(gs);

        return new ResponseDTO(true, 0, null!, gs, player);
    }
}