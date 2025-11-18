using GameTest.DTOs;
using GameTest.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GameTest.Services;

public class VertexPicker
{
    private List<GoalStats> TargetVertices = new List<GoalStats>();

    public VertexPicker(GameState gs)
    {
        if (gs.Phase.PhaseState == GameStates.PlaceFirstSettlement || gs.Phase.PhaseState == GameStates.PlaceSecondSettlement)
        {
            foreach (var vertex in gs.Vertices)
            {
                if (vertex.Building == null)
                {
                    // TODO: Update to set trade rate based on port info when ports supported
                    var stats = new GoalStats(vertex, 4, new Dictionary<ResourceType, double>(), 0, null);
                    TargetVertices.Add(stats);
                }
            }
        }
        else if (gs.Phase.PhaseState == GameStates.PlaceFirstRoad || gs.Phase.PhaseState == GameStates.PlaceSecondRoad)
        {
            if (gs.Phase.CurrentPlayer == null)
                throw new InvalidOperationException("Invalid state. Can't be in the set up phase for roads without a current player.");

            var targetVertex = AIHelpers.FindVertexWithoutRoads(gs, gs.Phase.CurrentPlayer);
            // TODO: Search for target vertice that start from vertex found above
        }
        else
        {
            TargetVertices = AIHelpers.GetRankedListOfVertexTargets(gs);
        }
    }

    public Vertex? PickVertex()
    {
        if (TargetVertices.Count > 0)
        {
            var bestGoal = TargetVertices.OrderByDescending(g => g.OverallScore).First();
            return bestGoal.TargetVertex;
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