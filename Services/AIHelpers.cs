using GameTest.Models;

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
                    if (!visitedEdgeIds.Contains(edge.Id))
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

    public static Vertex FindVertexWithoutRoads(GameState gs, Player player)
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
            return null;
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

    public static double GetAIResourceAcquisitionScore(double acquisitionRate, ResourceType type, bool hasPort)
    {
        double rate = acquisitionRate;

        // If the acquisition rate for the specified resource doesn't meet some minimum threshold,
        // there is no bonus for having a port for that resource.
        if (acquisitionRate >= GetAIWeight(AIWeights.MinimumThresholdForResourceSpecificPortBonus) && hasPort)
            rate *= GetAIWeight(AIWeights.VertexValueMultiplierForResourceSpecificPort);

        // Some resources may be more valuable than others.
        switch(type)
        {
           case ResourceType.Ore:
                rate *= GetAIWeight(AIWeights.PreferenceValueMultiplierForOreTiles);
                break;
           case ResourceType.Grain:
                rate *= GetAIWeight(AIWeights.PreferenceValueMultiplierForGrainTiles);
                break;
           case ResourceType.Brick:
                rate *= GetAIWeight(AIWeights.PreferenceValueMultiplierForBrickTiles);
                break;
           case ResourceType.Wood:
                rate *= GetAIWeight(AIWeights.PreferenceValueMultiplierForWoodTiles);
                break;
           case ResourceType.Wool:
                rate *= GetAIWeight(AIWeights.PreferenceValueMultiplierForWoolTiles);
                break;
        }

        return rate;
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
