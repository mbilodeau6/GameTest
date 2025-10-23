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
        var move = new BotMove();

        if (State.Phase.PhaseState == GameStates.PlaceFirstSettlement || State.Phase.PhaseState == GameStates.PlaceSecondSettlement)
        {
            int vIndex = 0;

            while (State.Vertices[vIndex].Building != null)
                vIndex++;

            // #pragma warning disable CS8604 // Constructor checks for null reference
            //             State.Vertices[vIndex].BuildSettlement(State.Phase.CurrentPlayer);
            // #pragma warning restore CS8604

            var target = State.Vertices[vIndex];
            move.VertexMove = new VertexDTO(target.Id, BuildingType.Settlement.ToString(), State.Phase.CurrentPlayer.Id, null);
        }
        else if (State.Phase.PhaseState == GameStates.PlaceFirstRoad || State.Phase.PhaseState == GameStates.PlaceSecondRoad)
        {
            int eIndex = 0;

            while (State.Edges[eIndex].Owner != null)
                eIndex++;

            // #pragma warning disable CS8604 // Constructor checks for null reference
            //             State.Edges[eIndex].BuildRoad(State.Phase.CurrentPlayer);
            // #pragma warning restore CS8604

            var target = State.Edges[eIndex];
            move.EdgeMove = new EdgeDTO(target.Id, State.Phase.CurrentPlayer.Id, null);    
        }

        return move;
    }

    public BotMove GetBuildMove()
    {
        return null;
    }
    
    public BotMove GetPreRollMove()
    {
        return null;
    }
}
