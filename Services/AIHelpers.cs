using GameTest.Models;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;

namespace GameTest.Services;

public static class AIHelpers
{
    public static double GetProbabilityForDiceRoll(int diceRoll)
    {
        switch (diceRoll)
        {
            case 2:
            case 12:
                return 1.0 / 36.0;
            case 3:
            case 11:
                return 2.0 / 36.0;
            case 4:
            case 10:
                return 3.0 / 36.0;
            case 5:
            case 9:
                return 4.0 / 36.0;
            case 6:
            case 8:
               return 5.0 / 36.0;
            case 7:
                return 6.0 / 36.0;
            default:
                throw new ArgumentOutOfRangeException("diceRoll", "Dice roll must be between 2 and 12.");
        }
    }

    public static double GetResourcePayoutValueForTile(GameState gs, Tile tile, Player player)
    {
        var ownedVertices = gs.Vertices.Where(v => v.Owner != null && v.Owner.Id == player.Id);
        var victoryPoints = 0;
        foreach(var vertex in ownedVertices)
        {
            if (vertex.Tiles.Any(t => t.Id == tile.Id))
                victoryPoints += GamePlayHelpers.GetVictoryPointsForBuild(vertex.Building);
        }

        return victoryPoints * GetProbabilityForDiceRoll(tile.DiceNumber) * GetResourceWeight(tile.Resource);
    }

    public static Dictionary<ResourceType, double> GetBaseResourceAcquisitionRates(GameState gs, Player player)
    {
        var baseStats = new Dictionary<ResourceType, double>();
        foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
        {
            if (rt == ResourceType.Desert)
                continue;

            baseStats[rt] = 0.0;
        }

        var ownedVertices = gs.Vertices.Where(v => v.Owner != null && v.Owner.Id == player.Id).ToList();
        foreach (var vertex in ownedVertices)
        {
            foreach (var tile in vertex.Tiles)
            {
                if (tile.Resource == ResourceType.Desert)
                    continue;

                baseStats[tile.Resource] += GetProbabilityForDiceRoll(tile.DiceNumber);
            }
        }

        return baseStats;
    }

    public static Vertex? GetSettlementToUpgrade(GameState gs)
    {
        if (gs.Phase.CurrentPlayer != null)
        {
            var baseRates = GetBaseResourceAcquisitionRates(gs, gs.Phase.CurrentPlayer);
            var ownedSettlements = gs.Vertices.Where(v => GamePlayHelpers.HasBuilding(v)
                                                        && v.Building == BuildingType.Settlement
                                                        && v.Owner != null
                                                        && v.Owner.Id == gs.Phase.CurrentPlayer.Id)
                                                        .OrderByDescending(g => (new GoalStats(g, gs.Phase.CurrentPlayer, baseRates, 0, null)).OverallScore).ToList();

            if (ownedSettlements.Count > 0)
            {
                // Simple strategy: upgrade the first settlement found
                return ownedSettlements.First();
            }
        }   

        return null;
    }

    public static Vertex? GetVertexReadyForSettlement(GameState gs)
    {
        if (gs.Phase.CurrentPlayer != null)
        {
            var baseRates = GetBaseResourceAcquisitionRates(gs, gs.Phase.CurrentPlayer);
            var options = new List<GoalStats>();

            foreach (var v1 in gs.Vertices.Where(v => v.Owner != null && v.Owner.Id == gs.Phase.CurrentPlayer.Id))
                foreach (var e1 in v1.Edges.Where(e => e.Owner != null && e.Owner.Id == gs.Phase.CurrentPlayer.Id))
                    foreach (var v2 in e1.Vertices.Where(v => v.Owner == null && v.Building == BuildingType.Blocked))
                        foreach (var e2 in v2.Edges.Where(e => e.Owner != null && e.Owner.Id == gs.Phase.CurrentPlayer.Id))
                            foreach (var v3 in e2.Vertices.Where(v => v.Owner == null && v.Building == null))
                                options.Add(new GoalStats(v3, gs.Phase.CurrentPlayer, baseRates, 0, null));

            if (options.Count == 0)
                return null;    

            return options.OrderByDescending(g => g.OverallScore).First().TargetVertex;
        }

        return null;
    }

    public static List<Vertex> GetAllOwnedBuildings(GameState gs, Player player)
    {
        return gs.Vertices.FindAll(v => GamePlayHelpers.HasBuilding(v) && v.Owner != null && v.Owner.Id == player.Id);
    }

    public static List<GoalStats> GetRankedListOfVertexTargets(GameState gs, List<Vertex> startingVertices)
    {
        var rankedGoals = new List<GoalStats>();

        if (gs.Phase.CurrentPlayer != null)
        {
            var baseRates = GetBaseResourceAcquisitionRates(gs, gs.Phase.CurrentPlayer);

            List<string> visitedEdgeIds = new List<string>();
            List<string> visitedVertexIds = new List<string>();

            // Note: The pathQueue is be defined as a priority queue so that we analyze targerts that require
            // fewer new roads first.
            PriorityQueue<CandidatePath, int> pathQueue = new PriorityQueue<CandidatePath, int>();

            foreach (var vertex in startingVertices)
            {
                visitedVertexIds.Add(vertex.Id);
                foreach(var edge in vertex.Edges)
                {
                    if ((edge.Owner == null || edge.Owner.Id == gs.Phase.CurrentPlayer.Id) && !visitedEdgeIds.Contains(edge.Id))
                    {
                        visitedEdgeIds.Add(edge.Id);
                        var newCandidatePath = new CandidatePath(edge);
                        pathQueue.Enqueue(newCandidatePath, newCandidatePath.NewBuildRequired);
                    }
                }
            }

            while(pathQueue.Count > 0)
            {
                var pathCandidate = pathQueue.Dequeue();

                // Skip vertices that have already been considered.
                Vertex candidateVertex;
                if (!visitedVertexIds.Contains(pathCandidate.NextEdge.Vertices[0].Id))
                    candidateVertex = pathCandidate.NextEdge.Vertices[0];
                else if (!visitedVertexIds.Contains(pathCandidate.NextEdge.Vertices[1].Id))
                    candidateVertex = pathCandidate.NextEdge.Vertices[1];
                else
                    continue;

                // If candidate vertex is open for building, add stats to candidate vertices
                if (candidateVertex.Building == null)
                    rankedGoals.Add(new GoalStats(candidateVertex, gs.Phase.CurrentPlayer, baseRates, pathCandidate.NewBuildRequired, pathCandidate.FirstEdge));
                else if (GamePlayHelpers.HasBuilding(candidateVertex))
                    continue;

                // Determine if vertex leads to other vertices that should be considered.
                visitedVertexIds.Add(candidateVertex.Id);
                foreach(var edge in candidateVertex.Edges)
                {
                    if (!visitedEdgeIds.Contains(edge.Id) && (edge.Owner == null || edge.Owner.Id == gs.Phase.CurrentPlayer.Id))
                    {
                        visitedEdgeIds.Add(edge.Id);
                        var newCandidatePath = pathCandidate.CreateBranchOfPath(edge);
                        pathQueue.Enqueue(newCandidatePath, newCandidatePath.NewBuildRequired);
                    }
                }
            }
        }

        return rankedGoals.OrderByDescending(g => g.OverallScore).ToList();
    }

    public static Dictionary<ResourceType, int> CalculateResourcesNeededForRoad(Dictionary<ResourceType, int> ownedResources)
    {
        var needed = new Dictionary<ResourceType, int>();

        if (!ownedResources.ContainsKey(ResourceType.Wood) || ownedResources[ResourceType.Wood] == 0)
            needed.Add(ResourceType.Wood, 1);

        if (!ownedResources.ContainsKey(ResourceType.Brick) || ownedResources[ResourceType.Brick] == 0)
            needed.Add(ResourceType.Brick, 1);

        return needed;
    }

    public static Dictionary<ResourceType, int> CalculateResourcesNeededForSettlement(Dictionary<ResourceType, int> ownedResources)
    {
        var needed = new Dictionary<ResourceType, int>();

        if (!ownedResources.ContainsKey(ResourceType.Wood) || ownedResources[ResourceType.Wood] == 0)
            needed.Add(ResourceType.Wood, 1);

        if (!ownedResources.ContainsKey(ResourceType.Brick) || ownedResources[ResourceType.Brick] == 0)
            needed.Add(ResourceType.Brick, 1);

        if (!ownedResources.ContainsKey(ResourceType.Wool) || ownedResources[ResourceType.Wool] == 0)
            needed.Add(ResourceType.Wool, 1);

        if (!ownedResources.ContainsKey(ResourceType.Grain) || ownedResources[ResourceType.Grain] == 0)
            needed.Add(ResourceType.Grain, 1);

        return needed;
    }

    public static Dictionary<ResourceType, int> CalculateResourcesNeededForCity(Dictionary<ResourceType, int> ownedResources)
    {
        var needed = new Dictionary<ResourceType, int>();

        if (!ownedResources.ContainsKey(ResourceType.Ore))
            needed.Add(ResourceType.Ore, 3);
        else if (ownedResources[ResourceType.Ore] < 3)
            needed.Add(ResourceType.Ore, 3 - ownedResources[ResourceType.Ore]);

        if (!ownedResources.ContainsKey(ResourceType.Grain))
            needed.Add(ResourceType.Grain, 2);
        else if (ownedResources[ResourceType.Grain] < 2)
            needed.Add(ResourceType.Grain, 2 - ownedResources[ResourceType.Grain]);

        return needed;
    }

    public static double GetResourceWeight(ResourceType type)
    {
        switch(type)
        {
           case ResourceType.Ore:
                return GetAIWeight(AIWeights.PreferenceValueMultiplierForOreTiles);
           case ResourceType.Grain:
                return GetAIWeight(AIWeights.PreferenceValueMultiplierForGrainTiles);
           case ResourceType.Brick:
                return GetAIWeight(AIWeights.PreferenceValueMultiplierForBrickTiles);
           case ResourceType.Wood:
                return GetAIWeight(AIWeights.PreferenceValueMultiplierForWoodTiles);
           case ResourceType.Wool:
                return GetAIWeight(AIWeights.PreferenceValueMultiplierForWoolTiles);
        }

        return 0.0;
    }

    public static double GetAIResourceAcquisitionScore(double acquisitionRate, ResourceType type, bool hasPort)
    {
        double rate = acquisitionRate;

        // If the acquisition rate for the specified resource doesn't meet some minimum threshold,
        // there is no bonus for having a port for that resource.
        if (acquisitionRate >= GetAIWeight(AIWeights.MinimumThresholdForResourceSpecificPortBonus) && hasPort)
            rate *= GetAIWeight(AIWeights.VertexValueMultiplierForResourceSpecificPort);

        // Some resources may be more valuable than others.
        rate *= GetResourceWeight(type);

        return rate;
    }

    // TODO: Enhance routine to consider which player may have a card the Bot wants
    // or to stop a player that may be trying to intercept the Bot's objectives.
    // Currently the routine is just looking for the most valuable tile for the Bot's
    // opponents that won't also impact the Bot.
    public static Tile PickTargetForRobber(GameState gs, Player currentPlayer)
    {
        double highestValue = -100.0;
        Tile? tileWithHighestValue = null;

        // If opponents are close to winning, target them. Otherwise, look at all.
        var playersOfInterest = gs.Players.Where(p => p.FullVictoryPoints >= gs.Settings.VictoryPointsToWin - 2 || p.Id == currentPlayer.Id);
        if (playersOfInterest.Count() == 1)
            playersOfInterest = gs.Players.ToList();

        foreach(var tile in gs.Tiles)
        {
            // Skip tile that already has robber
            if (tile.Id == gs.RobberTile.Id)
                continue;

            var tileValue = 0.0;

            foreach (var player in playersOfInterest)
                tileValue += GetResourcePayoutValueForTile(gs, tile, player) * (player.Id == currentPlayer.Id ? -1 : 1);

            // Check if value to opponent is higher than current selected tile
            if (tileValue > highestValue)
            {
                highestValue = tileValue;
                tileWithHighestValue = tile;
            }
        }

        if (tileWithHighestValue == null)
            throw new InvalidOperationException("Unexpected Error. Routine claims there is no tile that the current player isn't on. I didn't think this was possible.");

        return tileWithHighestValue;
    }

    // Primary use case is to help the Bot decide which resources to discard if a 7 is rolled. Could also be used
    // to identify cards that it would be willing to trade (and/or wants in a trade).
    // TODO: Review to determine if better to return a specialized class vs tuple.
    // InputParams: "needForX" values should add up to 1.0. They could have equal need but likely there are 
    // scenarios where one has need of 1.0 or that multiple have need of 0.0.
    // ReturnValue: First member of tuple reflects the value (between 0.0 and 1.0). The second number reflects 
    // how much of that resources the player needs to meet one of its goals (TBD if it is the max for any of the 
    // goals or max for one of the goals).
    public static Dictionary<ResourceType, (double, int)> RankedResourceListGivenStateAndGoals(GameState gs, Player player, 
        double needForRoad, double needForSettlement, double needForCity, double needForDevCard)
    {
        if (needForRoad + needForSettlement + needForCity + needForDevCard != 1.0)
            throw new ArgumentException("The sum of needFor values must be 1.0.");

        var resourceValues = new Dictionary<ResourceType, (double, int)>();

        foreach(var resource in Enum.GetValues<ResourceType>())
            resourceValues.Add(resource, (0.0, 0));

        // TODO: Shoudl return sorted to make it easy to pick the resource(s) that can be given up first.
        return resourceValues;
    }

    // Remove all instances of items in b from a
    public static List<ResourceType> MultiSetSubtraction(List<ResourceType> a, List<ResourceType> b)
    {
        var result = new List<ResourceType>(a);

        foreach (var item in b)
        {
            if (result.Contains(item))
                result.Remove(item);
        }

        return result;
    }

    public static List<ResourceType> ConvertResourceDictToList(Dictionary<ResourceType, int> resourceDict)
    {
        var resourceList = new List<ResourceType>();

        foreach (var kvp in resourceDict)
        {
            for (int i = 0; i < kvp.Value; i++)
                resourceList.Add(kvp.Key);
        }

        return resourceList;
    }

    /// <summary>
    /// Determines if the bot should play a development card and which one.
    /// Considers game phase, available cards, and whether playing would be beneficial.
    /// </summary>
    /// <param name="gs">Current game state</param>
    /// <param name="player">The bot player</param>
    /// <returns>The development card to play, or null if no card should be played</returns>
    public static DevelopmentCardType? GetDevCardToPlay(GameState gs, Player player)
    {
        // TODO: Implement - evaluate available dev cards and return best option
        return null;
    }

    /// <summary>
    /// Determines which resource to target with a Monopoly card.
    /// </summary>
    /// <returns>The resource type that would yield the best result</returns>
    public static ResourceType GetMonopolyTarget(GameState gs, Player player)
    {
        // TODO: Implement - analyze opponent resources and return best target
        return ResourceType.Desert; // Invalid placeholder
    }

    /// <summary>
    /// Determines which two resources to take with a Year of Plenty card.
    /// </summary>
    /// <returns>List of exactly 2 resources to take from the bank</returns>
    public static List<ResourceType> GetYearOfPlentyResources(GameState gs, Player player)
    {
        // TODO: Implement - determine which resources the bot needs most
        return new List<ResourceType>(); // Invalid placeholder - should have exactly 2
    }

    public static double ShouldBuyDevelopmentCard(GameState gs, Player player, bool spotReadyForSettlement = false)
    {
        if (!player.IsBot)
            throw new InvalidOperationException("ShouldBuyDevelopmentCard should only be called for Bot players.");
            
        if (GamePlayHelpers.HasResourcesToBuyDevCard(player))
        {
            if (gs.PlayerWithLargestArmy == null && player.CountPlayedKnights() == 2 && gs.Players.Any(p => p.Id != player.Id && p.VisibleVictoryPoints >= gs.Settings.VictoryPointsToWin - 2))
            {
                // prioritize buying a development card to try to get largest army
                return 0.8;
            }

            // prioritize cities over development cards
            if (GamePlayHelpers.HasResourcesToBuildCity(player) && gs.UnusedCityAvailable(player) && gs.CountSettlementsForPlayer(player) > 0)
            {
                return 0.1;
            }

            // prioritize settlements over development cards
            if (GamePlayHelpers.HasResourcesToBuildSettlement(player) && gs.UnusedSettlementAvailable(player) && spotReadyForSettlement)
            {
                return 0.1;
            }

            // Simple strategy for now: Buy a development card if we have enough resources
            return 0.9;
        }

        return 0.0;
    }

    public static double GetAIWeight(AIWeights name)
    {
        if (!AIWeightValues.ContainsKey(name))
            throw new InvalidOperationException($"Unexpected Exception: Requested AIWeight {name} doesn't exist.");
            
        return AIWeightValues[name];
    }

    // Weights/parameters an AI may track/adjust as we gather game data. Try to stick with numbers
    // that are added to a weight (use "ValueAdd" in the name and number between 0 and 1) and 
    // numbers multiplied to a weight (use "ValueMultiplier" in the name and number greater than 0).
    private static Dictionary<AIWeights, double> AIWeightValues = new Dictionary<AIWeights, double>()
    {
        {AIWeights.VertexValueAddForThreeToOnePort, 2.0/36.0 },
        {AIWeights.VertexValueMultiplierForResourceSpecificPort, 1.0 + 2.0/36.0},
        {AIWeights.MinimumThresholdForResourceSpecificPortBonus, 2.0/36.0 },
        {AIWeights.PreferenceValueMultiplierForOreTiles, 1.2 },
        {AIWeights.PreferenceValueMultiplierForGrainTiles, 1.1 },
        {AIWeights.PreferenceValueMultiplierForBrickTiles, 1.0 },
        {AIWeights.PreferenceValueMultiplierForWoodTiles, 0.9 },
        {AIWeights.PreferenceValueMultiplierForWoolTiles, 0.8 },
        {AIWeights.VertexValuePenaltyForDistanceToClosestBuild, 0.9 },
    };

    public enum AIWeights
    {
        VertexValueAddForThreeToOnePort,
        VertexValueMultiplierForResourceSpecificPort,
        MinimumThresholdForResourceSpecificPortBonus,
        PreferenceValueMultiplierForOreTiles,
        PreferenceValueMultiplierForGrainTiles,
        PreferenceValueMultiplierForBrickTiles,
        PreferenceValueMultiplierForWoodTiles,
        PreferenceValueMultiplierForWoolTiles,
        VertexValuePenaltyForDistanceToClosestBuild,
    }
}
