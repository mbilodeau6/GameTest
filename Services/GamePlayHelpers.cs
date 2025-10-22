using System.IO.Compression;
using Azure.Storage.Blobs.Models;
using GameTest.Models;
using GameTest.Tests;
using Google.Protobuf.WellKnownTypes;

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
        }

        return nextPhase;
    }
}
