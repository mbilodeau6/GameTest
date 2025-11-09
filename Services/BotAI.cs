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

        if (State.Phase.CurrentPlayer == null || !State.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

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

            // Find settlment w/o road (i.e. the one we just built)
            foreach (var vertex in State.Vertices.FindAll(v => v.Owner == State.Phase.CurrentPlayer))
            {
                if (vertex.Edges[0].Owner == null && vertex.Edges[1].Owner == null)
                {
                    target = vertex.Edges[0];  // TODO: Add logic to pick which edge is better
                    break;
                }
            }

            if (target == null)
                throw new InvalidOperationException("Unexpected State. There should be a settlement without an edge during set up.");

            move.EdgeMove = new EdgeDTO(target.Id, State.Phase.CurrentPlayer.Id, null);
        }

        return move;
    }

    // TODO: Need to create a more robust implementation that considers the game board and bot's current and future
    // opportunities. Remember to change the tests as well.
    public (bool CanTrade, TradeRequest? TradeRequest) AnalyzePossibleBankTrades(GameState gs)
    {
        if (gs.Phase.CurrentPlayer == null || !gs.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        ResourceType resourceToTrade = ResourceType.Desert;
        ResourceType resourceToGet = ResourceType.Desert;

        if (gs.Phase.CurrentPlayer.Resources[ResourceType.Wool] >= 5)
            resourceToTrade = ResourceType.Wool;
        else if (gs.Phase.CurrentPlayer.Resources[ResourceType.Wood] >= 5)
            resourceToTrade = ResourceType.Wood;
        else if (gs.Phase.CurrentPlayer.Resources[ResourceType.Brick] >= 5)
            resourceToTrade = ResourceType.Brick;
        else if (gs.Phase.CurrentPlayer.Resources[ResourceType.Grain] >= 6)
            resourceToTrade = ResourceType.Grain;
        else if (gs.Phase.CurrentPlayer.Resources[ResourceType.Ore] >= 7)
            resourceToTrade = ResourceType.Ore;


        if (resourceToTrade != ResourceType.Desert)
        {
            if (gs.Phase.CurrentPlayer.Resources[ResourceType.Ore] >= 3)
                resourceToGet = ResourceType.Grain;
            else if (gs.Phase.CurrentPlayer.Resources[ResourceType.Grain] >= 2)
                resourceToGet = ResourceType.Ore;
            else
                foreach (var resource in Enum.GetValues<ResourceType>())
                {
                    if (resource == ResourceType.Ore || resource == ResourceType.Desert)
                        continue;

                    if (gs.Phase.CurrentPlayer.Resources[resource] == 0)
                    {
                        resourceToGet = resource;
                        break;
                    }
                }

            if (resourceToGet == ResourceType.Desert)
                resourceToGet = ResourceType.Ore;

            return (true, new TradeRequest(gs.Phase.CurrentPlayer,
                    new Dictionary<ResourceType, int>() { { resourceToTrade, 4 } },
                    new Dictionary<ResourceType, int>() { { resourceToGet, 1 } }));

        }

        return (false, null);
    }

    // TODO: Need to restrict bot to building things it has the resources to build and to make more intelligent choices. 
    // Current code just picks next available spot for a settlement (if there is one). If there isn't, it picks the next
    // spot for a road.
    public BotMove GetBuildMove()
    {
        if (State.Phase.PhaseState != GameStates.BuildOrTrade)
            throw new InvalidOperationException($"GetBuildMove should only be called if phase is BuildOrTrade. Current phase is {State.Phase.PhaseState.ToString()}");

        if (State.Phase.CurrentPlayer == null || !State.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        var move = new BotMove();
        bool spotForSettlement = false;


        // First look to see if we can upgrade settlements to a city
        foreach (var vertex in State.Vertices.FindAll(v => v.Owner == State.Phase.CurrentPlayer && v.Building == BuildingType.Settlement))
        {
            if (GamePlayHelpers.HasResourcesToBuildCity(State.Phase.CurrentPlayer))
            {
                move.VertexMove = new VertexDTO(vertex.Id, BuildingType.City.ToString(), State.Phase.CurrentPlayer.Id, null);
                return move;
            }
        }

        foreach (var edge in State.Edges.FindAll(e => e.Owner == State.Phase.CurrentPlayer))
        {
            // Otherwise, look for a place to build a settlement adjacent to an existing road
            foreach (var vertex in edge.Vertices)
            {
                if (vertex.Building == null)
                {
                    if (GamePlayHelpers.HasResourcesToBuildSettlement(State.Phase.CurrentPlayer))
                    {
                        move.VertexMove = new VertexDTO(vertex.Id, BuildingType.Settlement.ToString(), State.Phase.CurrentPlayer.Id, null);
                        return move;
                    }
                    else
                    {
                        spotForSettlement = true;
                    }
                }
            }

            // Otherwise, look for a place to build a road adjacent to an existing road
            if (!spotForSettlement && GamePlayHelpers.HasResourcesToBuildRoad(State.Phase.CurrentPlayer))
                foreach (var vertex in edge.Vertices.FindAll(v => !GamePlayHelpers.HasBuilding(v)))
                {
                    foreach (var adjacentEdge in vertex.Edges.FindAll(e => e.Owner == null))
                    {
                        move.EdgeMove = new EdgeDTO(adjacentEdge.Id, State.Phase.CurrentPlayer.Id, null);
                        return move;
                    }
                }
        }

        var tradeAnalsysis = AnalyzePossibleBankTrades(State);
        if (tradeAnalsysis.CanTrade)
        {
            move.BankTrade = new TradeRequestDTO(tradeAnalsysis.TradeRequest);
            return move;
        }

        // Finally, see if you can build a road off of a settlement or city
        foreach (var vertex in State.Vertices.FindAll(v => v.Owner == State.Phase.CurrentPlayer && GamePlayHelpers.HasBuilding(v)))
        {
            if (GamePlayHelpers.HasResourcesToBuildRoad(State.Phase.CurrentPlayer))
                foreach (var adjacentEdge in vertex.Edges)
                {
                    if (adjacentEdge.Owner == null)
                    {
                        move.EdgeMove = new EdgeDTO(adjacentEdge.Id, State.Phase.CurrentPlayer.Id, null);
                        return move;
                    }
                }
        }

        move.EndTurn = true;
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
