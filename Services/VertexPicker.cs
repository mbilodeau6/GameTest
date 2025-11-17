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
                    var stats = new GoalStats(vertex, 4, new Dictionary<ResourceType, double>(), 0);
                    TargetVertices.Add(stats);
                }
            }
        }
        else
        {
            // TODO: Call AllHelpers.GetRankedListOfVertexTargets() to get list of target vertices
            // and pick the one with the highest ranking.
        }
    }

    public Vertex PickVertex()
    {
        if (TargetVertices.Count > 0)
        {
            var bestGoal = TargetVertices.OrderByDescending(g => g.OverallScore).First();
            return bestGoal.TargetVertex;
        }

        return null;
    }
}