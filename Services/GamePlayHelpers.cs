using GameTest.Models;

namespace GameTest.Services;

public static class GamePlayHelpers
{
    private static readonly Random _random = new();

    // TODO: Need test and implementation
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

    public static bool WithdrawResourcesToBuildRoad(Player player)
    {
        if (player.Resources.ContainsKey(ResourceType.Wood) && player.Resources[ResourceType.Wood] >= 1 &&
            player.Resources.ContainsKey(ResourceType.Brick) && player.Resources[ResourceType.Brick] >= 1)
        {
            player.RemoveResources(ResourceType.Wood, 1);
            player.RemoveResources(ResourceType.Brick, 1);
            return true;
        }

        return false;
    }

    public static bool WithdrawResourcesToBuildSettlement(Player player)
    {
        if (player.Resources.ContainsKey(ResourceType.Wood) && player.Resources[ResourceType.Wood] >= 1 &&
            player.Resources.ContainsKey(ResourceType.Brick) && player.Resources[ResourceType.Brick] >= 1 &&
            player.Resources.ContainsKey(ResourceType.Wool) && player.Resources[ResourceType.Wool] >= 1 &&
            player.Resources.ContainsKey(ResourceType.Grain) && player.Resources[ResourceType.Grain] >= 1)
        {
            player.RemoveResources(ResourceType.Wood, 1);
            player.RemoveResources(ResourceType.Brick, 1);
            player.RemoveResources(ResourceType.Wool, 1);
            player.RemoveResources(ResourceType.Grain, 1);
            return true;
        }

        return false;
    }

    public static bool WithdrawResourcesToBuildCity(Player player)
    {
        if (player.Resources.ContainsKey(ResourceType.Grain) && player.Resources[ResourceType.Grain] >= 2 &&
            player.Resources.ContainsKey(ResourceType.Ore) && player.Resources[ResourceType.Ore] >= 3)
        {
            player.RemoveResources(ResourceType.Ore, 3);
            player.RemoveResources(ResourceType.Grain, 2);
            return true;
        }

        return false;
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

    public static int CountRoadsForPlayer(GameState gs, Player player)
    {
        return gs.Edges.Count(v => v.Owner != null && v.Owner.Id == player.Id);
    }

    public static bool PlayerHasWon(GameState gs, Player player)
    {
        int settlementCount = gs.Vertices.Count(v => v.Owner != null && v.Owner.Id == player.Id && v.Building == BuildingType.Settlement);
        int cityCount = gs.Vertices.Count(v => v.Owner != null && v.Owner.Id == player.Id && v.Building == BuildingType.City);

        int victoryPoints = settlementCount + (cityCount * 2); 

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

    public static void EndTurn(Player player, GameState gameState)
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
    }

    private static Edge GetEdgeFromEdgeId(GameState gs, string edgeId)
    {
        return gs.Edges.First(e => e.Id == edgeId);
    }

    private static Vertex GetVertexFromVertexId(GameState gs, string vertexId)
    {
        return gs.Vertices.First(e => e.Id == vertexId);
    }

    private static bool IsPlayerSetupPhase(GameState gs)
    {
        return gs.Phase.PhaseState == GameStates.PlaceFirstSettlement ||
            gs.Phase.PhaseState == GameStates.PlaceFirstRoad ||
            gs.Phase.PhaseState == GameStates.PlaceSecondSettlement ||
            gs.Phase.PhaseState == GameStates.PlaceSecondRoad;
    }

    public static void StartGame(GameState gameState)
    {
        gameState.Phase = GetNextPhase(gameState);

        if (gameState.Phase.CurrentPlayer == null)
            throw new InvalidOperationException("Can not start game without a current player.");

        // Play for bot until bot's turn is over
        while (gameState.Phase.CurrentPlayer.IsBot && IsPlayerSetupPhase(gameState))
        {
            var bot = new BotAI(gameState);
            var move = bot.GetSetUpMove();

            if (move.EdgeMove != null)
            {
                var edge = GetEdgeFromEdgeId(gameState, move.EdgeMove.Id);
                edge.BuildRoad(gameState.Phase.CurrentPlayer);
            }

            if (move.VertexMove != null)
            {
                var vertex = GetVertexFromVertexId(gameState, move.VertexMove.Id);
                vertex.BuildSettlement(gameState.Phase.CurrentPlayer);
            }

            gameState.Phase = GetNextPhase(gameState);
        }

        if (gameState.Phase.CurrentPlayer.IsBot && gameState.Phase.PhaseState == GameStates.RollOrUseDevCard)
        {
            // TODO: Need to call RollOrUserDevCard logic for Bots
        }
    }


    private static bool BuildRoadPhase(GameState gs)
    {
        return gs.Phase.PhaseState == GameStates.PlaceFirstRoad ||
            gs.Phase.PhaseState == GameStates.PlaceSecondRoad ||
            gs.Phase.PhaseState == GameStates.BuildOrTrade;
    }

    // TODO: Return a GameResult type that can indicate success/failure and include messages.
    // Right now, an empty string indicates success.
    public static string BuildRoad(GameState gs, string playerId, string edgeId)
    {
        if (!BuildRoadPhase(gs))
        {
            return $"Game is not in a state that allows building roads. Current state: {gs.Phase.PhaseState}";
        }
        
        var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return $"Player {playerId} not found in game {gs.Id}";

        if (gs.Phase.CurrentPlayer.Id != playerId)
            return $"It is not {playerId}'s turn.";

        var edge = gs.Edges.FirstOrDefault(e => e.Id == edgeId);
        if (edge == null)
            return $"Edge {edgeId} not found in game {gs.Id}";

        if (edge.Owner != null)
            return $"Edge {edgeId} in game {gs.Id} already has a road.";

        edge.BuildRoad(player);



        return string.Empty;
    }
    }
