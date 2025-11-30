using System.Linq.Expressions;
using Azure;
using GameTest.DTOs;
using GameTest.Models;
using Microsoft.Identity.Client.Extensibility;

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

    public static bool WithdrawResourcesToBuyDevCard(Player player)
    {
        if (player.Resources.ContainsKey(ResourceType.Ore) && player.Resources[ResourceType.Ore] >= 1 &&
            player.Resources.ContainsKey(ResourceType.Grain) && player.Resources[ResourceType.Grain] >= 1 &&
            player.Resources.ContainsKey(ResourceType.Wool) && player.Resources[ResourceType.Wool] >= 1)
        {
            player.RemoveResources(ResourceType.Ore, 1);
            player.RemoveResources(ResourceType.Wool, 1);
            player.RemoveResources(ResourceType.Grain, 1);
            return true;
        }

        return false;
    }

    public static Player GetNextPlayer(Player currentPlayer, List<Player> players)
    {
        int currentPlayerIndex = players.FindIndex(p => p.Id == currentPlayer.Id);
        int nextPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        return players[nextPlayerIndex];
    }

    public static Player GetPreviousPlayer(Player currentPlayer, List<Player> players)
    {
        int currentPlayerIndex = players.FindIndex(p => p.Id == currentPlayer.Id);
        int previousPlayerIndex = (currentPlayerIndex + players.Count - 1) % players.Count;

        return players[previousPlayerIndex];
    }

    public static int CountSettlementsForPlayer(GameState gs, Player player)
    {
        return gs.Vertices.Count(v => v.Owner != null && v.Owner.Id == player.Id && v.Building == BuildingType.Settlement);
    }

    public static int CountCitiesForPlayer(GameState gs, Player player)
    {
        return gs.Vertices.Count(v => v.Owner != null && v.Owner.Id == player.Id && v.Building == BuildingType.City);
    }

    public static int CountRoadsForPlayer(GameState gs, Player player)
    {
        return gs.Edges.Count(v => v.Owner != null && v.Owner.Id == player.Id);
    }

    public static bool PlayerHasWon(GameState gs, Player player)
    {
        int victoryPoints = CountSettlementsForPlayer(gs, player) + (CountCitiesForPlayer(gs, player) * 2);

        return victoryPoints >= gs.Settings.VictoryPointsToWin;
    }

    public static GamePhase GetNextPhase(GameState gameState)
    {
        var nextPhase = gameState.Phase;

        if (gameState.Phase.PhaseState == GameStates.SettingUpBoard)
        {
            nextPhase.PhaseState = GameStates.PlaceFirstSettlement;
            nextPhase.CurrentPlayer = gameState.Players[_random.Next(gameState.Players.Count)];
            nextPhase.EndPlayer = GamePlayHelpers.GetPreviousPlayer(nextPhase.CurrentPlayer, gameState.Players);
        }
        else
        {
            if (gameState.Phase.CurrentPlayer == null)
                throw new InvalidOperationException("CurrentPlayer expected to be set to a valid value.");

            if (gameState.Phase.PhaseState == GameStates.PlaceFirstSettlement)
            {
                if (CountSettlementsForPlayer(gameState, gameState.Phase.CurrentPlayer) > 0)
                    nextPhase.PhaseState = GameStates.PlaceFirstRoad;
            }
            else if (gameState.Phase.PhaseState == GameStates.PlaceFirstRoad)
            {
                if (CountRoadsForPlayer(gameState, gameState.Phase.CurrentPlayer) > 0)
                {
                    if (gameState.Phase.CurrentPlayer == gameState.Phase.EndPlayer)
                    {
                        nextPhase.PhaseState = GameStates.PlaceSecondSettlement;
                        gameState.Phase.EndPlayer = GetNextPlayer(gameState.Phase.CurrentPlayer, gameState.Players);
                    }
                    else
                    {
                        nextPhase.PhaseState = GameStates.PlaceFirstSettlement;
                        gameState.Phase.CurrentPlayer = GetNextPlayer(gameState.Phase.CurrentPlayer, gameState.Players);
                    }
                }
            }
            else if (gameState.Phase.PhaseState == GameStates.PlaceSecondSettlement)
            {
                if (CountSettlementsForPlayer(gameState, gameState.Phase.CurrentPlayer) > 1)
                    nextPhase.PhaseState = GameStates.PlaceSecondRoad;
            }
            else if (gameState.Phase.PhaseState == GameStates.PlaceSecondRoad)
            {
                if (CountRoadsForPlayer(gameState, gameState.Phase.CurrentPlayer) > 1)
                {
                    if (gameState.Phase.CurrentPlayer == gameState.Phase.EndPlayer)
                    {
                        nextPhase.PhaseState = GameStates.RollOrUseDevCard;
                        gameState.Dice.SetWaiting();
                        gameState.Phase.EndPlayer = GetPreviousPlayer(gameState.Phase.CurrentPlayer, gameState.Players);
                    }
                    else
                    {
                        nextPhase.PhaseState = GameStates.PlaceSecondSettlement;
                        gameState.Phase.CurrentPlayer = GetPreviousPlayer(gameState.Phase.CurrentPlayer, gameState.Players);
                    }
                }
            }
            else if (gameState.Phase.PhaseState == GameStates.RollOrUseDevCard && !gameState.Dice.WaitingForRoll)
            {
                if (gameState.Dice.Die1.Value + gameState.Dice.Die2.Value == 7)
                {
                    gameState.Phase.SetStateToReturnTo(GameStates.BuildOrTrade, gameState.RobberTile);
                    nextPhase.PhaseState = GameStates.PlaceRobber;
                }
                else
                    nextPhase.PhaseState = GameStates.BuildOrTrade;
            }
            else if (gameState.Phase.PhaseState == GameStates.BuildOrTrade && PlayerHasWon(gameState, gameState.Phase.CurrentPlayer))
            {
                // TODO: Add code to move to PlaceRobber if Knight Dev Card played.
                nextPhase.PhaseState = GameStates.GameOver;
            }
            else if (gameState.Phase.PhaseState == GameStates.PlaceRobber 
                && gameState.Phase.PreviousState != null 
                && gameState.Phase.OriginalRobberTile != null && gameState.RobberTile.Id != gameState.Phase.OriginalRobberTile.Id)
            {
                nextPhase.PhaseState = (GameStates)gameState.Phase.PreviousState;
                gameState.Phase.ClearRobberState();
            }
        }

        return nextPhase;
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

        gameState.Phase.CurrentPlayer = GetNextPlayer(gameState.Phase.CurrentPlayer, gameState.Players);
        gameState.Phase.PhaseState = GameStates.RollOrUseDevCard;
        gameState.Dice.SetWaiting();

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
            gs.Phase.PhaseState == GameStates.BuildOrTrade;
    }

    private static bool BuildSettlementPhase(GameState gs)
    {
        return gs.Phase.PhaseState == GameStates.PlaceFirstSettlement ||
            gs.Phase.PhaseState == GameStates.PlaceSecondSettlement ||
            gs.Phase.PhaseState == GameStates.BuildOrTrade;
    }

    private static bool BuildCityPhase(GameState gs)
    {
        return gs.Phase.PhaseState == GameStates.BuildOrTrade;
    }

    public static void BuildRoad(GameState gs, Player player, Edge edge)
    {
        if (gs.Phase.PhaseState == GameStates.BuildOrTrade)
            WithdrawResourcesToBuildRoad(player);

        edge.BuildRoad(player);
        gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.PlaceRoad, edge));
    }

    // TODO: Return a GameResult type that can indicate success/failure and include messages.
    // Right now, an empty string indicates success.
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
        if (gs.Phase.PhaseState == GameStates.BuildOrTrade)
        {
            WithdrawResourcesToBuildCity(player);
            vertex.UpgradeToCity();
            gs.EventRecord.Add(new EventRecordDTO(player, EventRecordAction.UpgradeSettlement, vertex));
        }
    }


    public static ResponseDTO UpgradeToCityRequestFromUser(GameState gs, string playerId, string vertexId)
    {
        if (gs.Phase.PhaseState != GameStates.BuildOrTrade || gs.Phase.CurrentPlayer == null)
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

        gs.Phase = GetNextPhase(gs);

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

                gs.Phase = GetNextPhase(gs);
            }
        }
    }

    public static void RollDice(GameState gs, bool skipGameLoop = false)
    {
        if (gs.Phase.PhaseState != GameStates.RollOrUseDevCard || gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException($"Unexpected Exception. Roll called when game in {gs.Phase.PhaseState}. Player: {gs.Phase.CurrentPlayer}.");

        gs.Dice.Roll();
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
        return CountRoadsForPlayer(gs, player) < gs.Settings.RoadsPerPlayer;
    }

    public static bool UnusedSettlementAvailable(GameState gs, Player player)
    {
        return CountSettlementsForPlayer(gs, player) < gs.Settings.SettlementsPerPlayer;
    }

    public static bool UnusedCityAvailable(GameState gs, Player player)
    {
        return CountCitiesForPlayer(gs, player) < gs.Settings.CitiesPerPlayer;
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

        GameLoop(gs);
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

        return new ResponseDTO(true, 0, null, gs);
    }

    public static ResponseDTO BuyDevCard(GameState gs, string playerId)
    {
        if (gs.Phase.PhaseState != GameStates.BuildOrTrade || gs.Phase.CurrentPlayer == null)
            return new ResponseDTO(false, 1003, $"Action: BuyDevCard; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {playerId}", null as GameStateDTO);

        if (gs.Phase.CurrentPlayer.Id != playerId)
            return new ResponseDTO(false, 1011, $"GameId: {gs.Id}; PlayerTurn: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);

        // TODO: Logic to actually buy/assign dev card

        return new ResponseDTO(true, 0, null, gs);
    }

    public static ResponseDTO PlayMonopolyDevCard(GameState gs, PlayDevCardRequest request)
    {
        return new ResponseDTO(false, 9999, $"PLACEHOLDER", null as GameStateDTO);
    }
    public static ResponseDTO PlayYearOfPlentyDevCard(GameState gs, PlayDevCardRequest request)
    {
        return new ResponseDTO(false, 9999, $"PLACEHOLDER", null as GameStateDTO);
    }
    public static ResponseDTO PlayKnightDevCard(GameState gs, PlayDevCardRequest request)
    {
        return new ResponseDTO(false, 9999, $"PLACEHOLDER", null as GameStateDTO);
    }

    public static ResponseDTO PlayRoadBuildingDevCard(GameState gs, PlayDevCardRequest request)
    {
        return new ResponseDTO(false, 9999, $"PLACEHOLDER", null as GameStateDTO);
    }
}
