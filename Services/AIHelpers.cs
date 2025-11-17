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
        if (gs.Phase.CurrentPlayer != null && GamePlayHelpers.HasResourcesToBuildCity(gs.Phase.CurrentPlayer))
        {
            var baseRates = GetBaseResourceAcquisitionRates(gs, gs.Phase.CurrentPlayer);
            var ownedSettlements = gs.Vertices.Where(v => GamePlayHelpers.HasBuilding(v)
                                                        && v.Building == BuildingType.Settlement
                                                        && v.Owner != null
                                                        && v.Owner.Id == gs.Phase.CurrentPlayer.Id)
                                                        .OrderByDescending(g => (new GoalStats(g, 4, baseRates, 0)).OverallScore).ToList();

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

            foreach (var edge in gs.Edges.Where(e => e.Owner != null && e.Owner.Id == gs.Phase.CurrentPlayer.Id))
                foreach (var vertex in edge.Vertices)
                    if (vertex.Building == null)
                        options.Add(new GoalStats(vertex, 4, baseRates, 0));

            if (options.Count == 0)
                return null;    

            return options.OrderByDescending(g => g.OverallScore).First().TargetVertex;
        }

        return null;
    }

    public static List<GoalStats> GetRankedListOfVertexTargets(GameState gs)
    {
        var rankedGoals = new List<GoalStats>();

        if (gs.Phase.CurrentPlayer != null)
        {
            var baseRates = GetBaseResourceAcquisitionRates(gs, gs.Phase.CurrentPlayer);

            List<string> visitedEdgeIds = new List<string>();
            List<string> visitedVertexIds = new List<string>();
            PriorityQueue<CandidatePath, int> pathQueue = new PriorityQueue<CandidatePath, int>();

            foreach (var vertex in gs.Vertices.FindAll(v => GamePlayHelpers.HasBuilding(v) && v.Owner != null && v.Owner.Id == gs.Phase.CurrentPlayer.Id))
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

                // Skip vertices that have already been considered
                // TODO: Is it possible that existing candidates in the list have longer roads than the new candidate?
                // If yes, need to replace the existing with the new.
                Vertex candidateVertex;
                if (!visitedVertexIds.Contains(pathCandidate.NextEdge.Vertices[0].Id))
                    candidateVertex = pathCandidate.NextEdge.Vertices[0];
                else if (!visitedVertexIds.Contains(pathCandidate.NextEdge.Vertices[1].Id))
                    candidateVertex = pathCandidate.NextEdge.Vertices[1];
                else
                    continue;

                // If candidate vertex is open for building, add stats to candidate vertices
                if (candidateVertex.Building == null)
                    rankedGoals.Add(new GoalStats(candidateVertex, 4, baseRates, pathCandidate.NewBuildRequired));
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
}
