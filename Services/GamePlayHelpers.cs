using System.Linq.Expressions;
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

    public static Dictionary<Player, Dictionary<ResourceType, int>> GetResourcesEarnedOnLastRoll(GameState gameState)
    {
        var resourcesEarned = new Dictionary<Player, Dictionary<ResourceType, int>>();

        if (gameState.Dice.GetCombinedValue() == 7)
            return resourcesEarned;

        var matchingTiles = gameState.Tiles.FindAll(t => t.DiceNumber == gameState.Dice.GetCombinedValue());

        foreach (var tile in matchingTiles)
        {
            foreach (var vertex in gameState.Vertices)
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
            nextPhase.CurrentPlayer = gameState.Players[_random.Next(1, gameState.Players.Count)];
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
                nextPhase.PhaseState = GameStates.BuildOrTrade;
            }
            else if (gameState.Phase.PhaseState == GameStates.BuildOrTrade && PlayerHasWon(gameState, gameState.Phase.CurrentPlayer))
            {
                nextPhase.PhaseState = GameStates.GameOver;
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
    }

    // TODO: Return a GameResult type that can indicate success/failure and include messages.
    // Right now, an empty string indicates success.
    public static string BuildRoadRequestFromUser(GameState gs, string playerId, string edgeId)
    {
        if (!BuildRoadPhase(gs) || gs.Phase.CurrentPlayer == null)
            return $"Game is not in a state that allows building roads. Current state: {gs.Phase.PhaseState}";

        var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return $"Player {playerId} not found in game {gs.Id}";

        if (gs.Phase.CurrentPlayer.Id != playerId)
            return $"It is not {playerId}'s turn.";

        var edge = gs.Edges.FirstOrDefault(e => e.Id == edgeId);
        if (edge == null)
            return $"Edge {edgeId} not found in game {gs.Id}";

        if (!IsEdgeAdjacentToPlayerBuild(gs, edge, player))
            return $"Edge {edgeId} not adjacent to a city/settlement for player {playerId}.";

        if (edge.Owner != null)
            return $"Edge {edgeId} in game {gs.Id} already has a road.";

        if (gs.Phase.PhaseState == GameStates.BuildOrTrade && !HasResourcesToBuildRoad(player))
            return $"Player {player.Id} does not have the required resources to build a road.";

        BuildRoad(gs, player, edge);

        GameLoop(gs);

        return string.Empty;
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
        MarkBlockedVertices(gs, vertex);
    }

    // TODO: Return a GameResult type that can indicate success/failure and include messages.
    // Right now, an empty string indicates success.
    public static string BuildSettlementRequestFromUser(GameState gs, string playerId, string vertexId)
    {
        if (!BuildSettlementPhase(gs) || gs.Phase.CurrentPlayer == null)
            return $"Game is not in a state that allows building settlements. Current state: {gs.Phase.PhaseState}";

        var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return $"Player {playerId} not found in game {gs.Id}";

        if (gs.Phase.CurrentPlayer.Id != playerId)
            return $"It is not {playerId}'s turn.";

        var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);
        if (vertex == null)
            return $"Vertex {vertexId} not found in game {gs.Id}";

        if (vertex.Building == BuildingType.Settlement)
            return $"Vertex {vertexId} in game {gs.Id} already has a settlement.";

        if (vertex.Building == BuildingType.City)
            return $"Vertex {vertexId} in game {gs.Id} already has a city.";

        if (vertex.Building == BuildingType.Blocked)
            return $"Vertex {vertexId} in game {gs.Id} is too close to another development.";

        if (gs.Phase.PhaseState == GameStates.BuildOrTrade && !IsVertexAdjacentToPlayerRoad(gs, vertex, player))
            return $"Vertex {vertexId} not adjacent to a road for player {playerId}.";

        if (gs.Phase.PhaseState == GameStates.BuildOrTrade && !HasResourcesToBuildSettlement(player))
            return $"Player {player.Id} does not have the required resources to build a settlement.";

        BuildSettlement(gs, player, vertex);

        GameLoop(gs);

        return string.Empty;
    }

    public static void UpgradeToCity(GameState gs, Player player, Vertex vertex)
    {
        if (gs.Phase.PhaseState == GameStates.BuildOrTrade)
        {
            WithdrawResourcesToBuildCity(player);
            vertex.UpgradeToCity();
        }
    }


    public static string UpgradeToCityRequestFromUser(GameState gs, string playerId, string vertexId)
    {
        if (gs.Phase.PhaseState != GameStates.BuildOrTrade || gs.Phase.CurrentPlayer == null)
            return $"Game is not in a state that allows building cities. Current state: {gs.Phase.PhaseState}; Current player: {gs.Phase.CurrentPlayer.Id}";

        var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return $"Player {playerId} not found in game {gs.Id}";

        if (gs.Phase.CurrentPlayer.Id != playerId)
            return $"It is not {playerId}'s turn.";

        var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);
        if (vertex == null)
            return $"Vertex {vertexId} not found in game {gs.Id}";

        if (vertex.Building == null || vertex.Owner == null)
            return $"Vertex {vertexId} in game {gs.Id} does not have a settlement to upgrade.";

        if (vertex.Building == BuildingType.City)
            return $"Vertex {vertexId} in game {gs.Id} already has a city.";

        if (vertex.Owner.Id != playerId)
            return $"Vertex {vertexId} in game {gs.Id} is owned by another player.";

        if (!HasResourcesToBuildCity(player))
            return $"Player {player.Id} does not have the required resources to build a city.";

        UpgradeToCity(gs, player, vertex);

        GameLoop(gs);

        return string.Empty;
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

                gs.Phase = GetNextPhase(gs);
            }
        }
    }

    public static void RollDice(GameState gs, bool skipGameLoop = false)
    {
        gs.Dice.Roll();
        GamePlayHelpers.AssignResourcesBasedOnLastDiceRoll(gs);

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

    public static string BankTrade(TradeRequest request)
    {
        Bank bank = new Bank();

        if (bank.TradeWithBank(request.Player, request.Offer, request.Request))
            return string.Empty;
        else
            return "Bank trade request rejected.";
    }
    
    public static string BankTradeFromUser(GameState gs, TradeRequestDTO request)
    {
        if (gs.Phase.PhaseState != GameStates.BuildOrTrade || gs.Phase.CurrentPlayer == null)
            return $"Game is not in a state that allows trades. Current state: {gs.Phase.PhaseState}";

        if (gs.Phase.CurrentPlayer.Id != request.PlayerId)
            return $"It is not {request.PlayerId}'s turn.";

        return BankTrade(new TradeRequest(gs, request));
    }

}
