using GameTest.DTOs;
using GameTest.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GameTest.Services;

public class BotAI
{
    // Temporarily storing the full GameState object. Assume that I'll eventually have a more optimized
    // internal representation.
    private GameState State;

    public BotAI(GameState gs)
    {
        if (gs == null)
            throw new ArgumentNullException("gs");

        if (gs.Phase.CurrentPlayer == null || !gs.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        State = gs;
    }

    // TODO: Need to make more intelligent choices. Current code just picks next available spot.
    public BotMove GetSetUpMove()
    {
        if (!GamePlayHelpers.IsPlayerSetupPhase(State))
            throw new InvalidOperationException($"GetSetUp should only be called if in one of the phases. Current phase is {State.Phase.PhaseState.ToString()}");

        var move = new BotMove();

        if (State.Phase.PhaseState == GameStates.PlaceFirstSettlement || State.Phase.PhaseState == GameStates.PlaceSecondSettlement)
        {
            int vIndex = 0;

            while (State.Vertices[vIndex].Building != null)
                vIndex++;

            var target = State.Vertices[vIndex];
            move.VertexMove = new VertexDTO(target.Id, BuildingType.Settlement.ToString(), State.Phase.CurrentPlayer.Id, null);
        }
        else if (State.Phase.PhaseState == GameStates.PlaceFirstRoad || State.Phase.PhaseState == GameStates.PlaceSecondRoad)
        {
            Edge? target = null;

            // Find settlment w/o road
            foreach (var vertex in State.Vertices.FindAll(v => v.Owner == State.Phase.CurrentPlayer))
            {
                if (vertex.Edges[0].Owner == null && vertex.Edges[1].Owner == null)
                {
                    target = vertex.Edges[0];  // TODO: Add logic to pick which edge is better
                    break;
                }
            }

            if (target == null)
                throw new InvalidOperationException("Unexpected State. There should be a settlement without an edge.");

            move.EdgeMove = new EdgeDTO(target.Id, State.Phase.CurrentPlayer.Id, null);
        }

        return move;
    }

    // TODO: Need to restrict bot to building things it has the resources to build and to make more intelligent choices. 
    // Current code just picks next available spot for a settlement (if there is one). If there isn't, it picks the next
    // spot for a road.
    public BotMove GetBuildMove()
    {
        if (State.Phase.PhaseState != GameStates.BuildOrTrade)
            throw new InvalidOperationException($"GetBuildMove should only be called if phase is BuildOrTrade. Current phase is {State.Phase.PhaseState.ToString()}");

        var move = new BotMove();

        foreach(var edge in State.Edges.FindAll(e => e.Owner == State.Phase.CurrentPlayer))
        {
            // First loook for a place to build a settlement adjacent to an existing road
            foreach (var vertex in edge.Vertices)
            {
                if (vertex.Building == null)
                {
                    move.VertexMove = new VertexDTO(vertex.Id, BuildingType.Settlement.ToString(), State.Phase.CurrentPlayer.Id, null);
                    return move;
                }
            }

            // Otherwise look for a place to build a road adjacent to an existing road
            foreach (var vertex in edge.Vertices)
            {
                foreach (var adjacentEdge in vertex.Edges)
                {
                    if (adjacentEdge.Owner == null)
                    {
                        move.EdgeMove = new EdgeDTO(adjacentEdge.Id, State.Phase.CurrentPlayer.Id, null);
                        return move;
                    }
                }
            }
        }

        // TODO: Since the current version doesn't require resources to build, we should never reach this point.
        throw new InvalidOperationException("No valid build moves available for Bot.");
    }
    
    public BotMove GetPreRollMove()
    {
        if (State.Phase.PhaseState != GameStates.RollOrUseDevCard)
            throw new InvalidOperationException($"GetPreRollMove should only be called if phase is RollOrUseDevCard. Current phase is {State.Phase.PhaseState.ToString()}");

        var move = new BotMove();
        move.RollDice = true;

        return move;
    }
}
