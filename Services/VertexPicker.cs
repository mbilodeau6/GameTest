using GameTest.DTOs;
using GameTest.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GameTest.Services;

public class VertexPicker
{
    private List<GoalStats> TargetVertices = new List<GoalStats>();

    public VertexPicker(GameState gs)
    {
        if (gs.Phase.CurrentPlayer == null)
            throw new InvalidOperationException("Invalid state. Can't be in the set up phase for roads without a current player.");

        if (gs.Phase.PhaseState == GameStates.PlaceFirstSettlement || gs.Phase.PhaseState == GameStates.PlaceSecondSettlement)
        {
            // Search for any open vertex
            foreach (var vertex in gs.Vertices)
            {
                if (vertex.Building == null)
                {
                    var stats = new GoalStats(vertex, gs.Phase.CurrentPlayer, new Dictionary<ResourceType, double>(), 0, null);
                    TargetVertices.Add(stats);
                }
            }
        }
        else if (gs.Phase.PhaseState == GameStates.PlaceFirstRoad || gs.Phase.PhaseState == GameStates.PlaceSecondRoad)
        {
            // Search for vertices that can be reached from new settlement
            var targetVertex = GamePlayHelpers.FindSettlementWithNoRoads(gs, gs.Phase.CurrentPlayer);
            TargetVertices = AIHelpers.GetRankedListOfVertexTargets(gs, new List<Vertex> { targetVertex });
        }
        else
        {
            // Search for vertices that can be reached from any owned settlement
            TargetVertices = AIHelpers.GetRankedListOfVertexTargets(gs, AIHelpers.GetAllOwnedBuildings(gs, gs.Phase.CurrentPlayer));
        }
    }

    public Vertex? PickVertex()
    {
        if (TargetVertices.Count > 0)
        {
            var targetsSorted = TargetVertices.OrderByDescending(g => g.OverallScore);
            return targetsSorted.First().TargetVertex;
        }

        return null;
    }

    public Edge? PickEdge()
    {
        if (TargetVertices.Count> 0)
        {
            var bestGoal = TargetVertices.OrderByDescending(g => g.OverallScore).First();
            return bestGoal.NextEdgeToTarget;
        }

        return null;
    }
}