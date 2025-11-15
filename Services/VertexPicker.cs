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
                    var stats = new GoalStats(vertex, 4);
                    TargetVertices.Add(stats);
                }
            }
        }
        else
        {
            List<string> visitedEdgeIds = new List<string>();
            List<string> visitedVertexIds = new List<string>();
            Queue<Edge> EdgeQueue = new Queue<Edge>();

            foreach (var vertex in gs.Vertices.FindAll(v => GamePlayHelpers.HasBuilding(v) && v.Owner != null && v.Owner.Id == gs.Phase.CurrentPlayer.Id))
            {
                foreach(var edge in vertex.Edges)
                {
                    if (!visitedEdgeIds.Contains(edge.Id))
                    {
                        visitedEdgeIds.Add(edge.Id);
                        EdgeQueue.Enqueue(edge);
                    }
                }
            }

            while(EdgeQueue.Count > 0)
            {
                var edge = EdgeQueue.Dequeue();
                // TODO: What's next?
            }
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