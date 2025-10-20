using Azure.Storage.Blobs.Models;
using GameTest.Models;
using GameTest.Tests;
using Google.Protobuf.WellKnownTypes;

namespace GameTest.Services;

public static class GamePlayHelpers
{

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
}
