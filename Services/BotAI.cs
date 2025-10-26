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
            int eIndex = 0;

            while (State.Edges[eIndex].Owner != null)
                eIndex++;

            var target = State.Edges[eIndex];
            move.EdgeMove = new EdgeDTO(target.Id, State.Phase.CurrentPlayer.Id, null);
        }

        return move;
    }

    // TODO: Need to restrict bot to building things it has the resources to build and to make more intelligent choices. 
    // Current code just picks next available spot for a road and settlement.
    public BotMove GetBuildMove()
    {
        if (State.Phase.PhaseState != GameStates.BuildOrTrade)
            throw new InvalidOperationException($"GetBuildMove should only be called if phase is BuildOrTrade. Current phase is {State.Phase.PhaseState.ToString()}");

        var move = new BotMove();

        // build settlement
        int vIndex = 0;

        while (State.Vertices[vIndex].Building != null)
            vIndex++;

        var newVertex = State.Vertices[vIndex];
        move.VertexMove = new VertexDTO(newVertex.Id, BuildingType.Settlement.ToString(), State.Phase.CurrentPlayer.Id, null);

        // build road
        int eIndex = 0;

        while (State.Edges[eIndex].Owner != null)
            eIndex++;

        var newEdge = State.Edges[eIndex];
        move.EdgeMove = new EdgeDTO(newEdge.Id, State.Phase.CurrentPlayer.Id, null);

        return move;
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
