using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Numerics;
using GameTest.DTOs;
using GameTest.Models;
using Microsoft.Identity.Client;

namespace GameTest.Services;

public static class UndoHelpers
{
    // Valid game states for undo
    private static readonly HashSet<GameStates> UndoValidGameStates = new()
    {
        GameStates.PlaceFirstSettlement,
        GameStates.PlaceFirstRoad,
        GameStates.PlaceSecondSettlement,
        GameStates.PlaceSecondRoad,
        GameStates.RollOrUseDevCard,
        GameStates.FirstDevCardRoad,
        GameStates.SecondDevCardRoad,
        GameStates.DiscardCards,
        GameStates.BuildOrTrade
    };

    public static UndoValidationResponse ValidateUndoRequest(GameState gs, UndoRequest request)
    {
        // Validate game state allows undo
        if (!UndoValidGameStates.Contains(gs.Phase.PhaseState))
            return new UndoValidationResponse { UndoPossible = false, ErrorCode = 1003};

        // Validate player exists
        var player = gs.Players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
            return new UndoValidationResponse { UndoPossible = false, ErrorCode = 1012, Player = player};

        // Validate event exists
        var eventToUndo = gs.EventRecord.FirstOrDefault(e => e.Id == request.EventId);
        if (eventToUndo == null)
            return new UndoValidationResponse { UndoPossible = false, ErrorCode = 1070, Player = player, Event = eventToUndo };

        // Validate the event belongs to the requesting player
        if (eventToUndo.PlayerId != request.PlayerId)
            return new UndoValidationResponse { UndoPossible = false, ErrorCode = 1071, Player = player, Event = eventToUndo };

        // Validate that there is an undoable action (i.e. there is state in the Undo stack)
        if (gs.UndoState.Count() == 0)
            return new UndoValidationResponse { UndoPossible = false, ErrorCode = 1072, Player = player, Event = eventToUndo };

        return new UndoValidationResponse { UndoPossible = true, Player = player, Event = eventToUndo };
    }

    public static ResponseDTO UndoFromUser(GameState gs, UndoRequest request, bool stateAlreadyChanged = false)
    {
        var validationResponse = UndoHelpers.ValidateUndoRequest(gs, request);

        if (!validationResponse.UndoPossible)
            switch (validationResponse.ErrorCode)
            {
                case 1003:
                    return new ResponseDTO(false, 1003, $"Action: Undo; GameId: {gs.Id}; State: {gs.Phase.PhaseState}", null as GameStateDTO);
                case 1013:
                    return new ResponseDTO(false, 1012, $"GameId: {gs.Id}; Player: {request.PlayerId}", null as GameStateDTO);
                case 1070:
                    return new ResponseDTO(false, 1070, $"GameId: {gs.Id}; EventId: {request.EventId}", null as GameStateDTO);
                case 1071:
                    if (validationResponse.Event == null)
                        throw new InvalidOperationException("UnexpectedError. Undo error 1071 missing event information.");
                    else
                        return new ResponseDTO(false, 1071, $"GameId: {gs.Id}; EventId: {request.EventId}; EventPlayer: {validationResponse.Event.PlayerId}; RequestingPlayer: {request.PlayerId}", null as GameStateDTO);
                case 1072:
                    if (validationResponse.Event == null)
                        throw new InvalidOperationException("UnexpectedError. Undo error 1072 missing event information.");
                    else
                        return new ResponseDTO(false, 1072, $"GameId: {gs.Id}; EventId: {request.EventId}; Action: {validationResponse.Event.Action}", null as GameStateDTO);
                case 1074:
                    return new ResponseDTO(false, 1074, $"GameId: {gs.Id}; EventId: {request.EventId}; EventBlockingUndo: {validationResponse.EventBlockingUndo}", null as GameStateDTO);
            }
        
        if (validationResponse.Player == null || validationResponse.Event == null)
            throw new InvalidOperationException("UnexpectedError. PossibleUndo response missing player or event.");

        ReverseAction(gs, validationResponse.Player, validationResponse.Event, stateAlreadyChanged);
        gs.AddEventRecord(new EventRecordDTO(validationResponse.Player, EventRecordAction.Undo, request.EventId));
        GamePlayHelpers.GameLoop(gs);

        return new ResponseDTO(true, 0, null!, gs, validationResponse.Player);
    }


    public static void ReverseAction(GameState gs, Player player, EventRecordDTO er, bool stateAlreadyChanged = false)
    {
        if (player.Id != er.PlayerId)
            throw new InvalidOperationException("Unexpected Error. Player doesn't match event player.");

        if (gs.UndoState.Count() == 0)
            throw new InvalidOperationException("Unexpected Error. ReverseAction called when UndoState empty.");

        switch(er.Action)
        {
            case EventRecordAction.PlaceFirstSettlement:
                UndoPlaceFirstSettlement(gs, player, er.VertexId!);
                break;
            case EventRecordAction.PlaceRoad:
                UndoPlaceRoad(gs, player, er.EdgeId!);
                break;
            case EventRecordAction.PlaceSecondSettlement:
                UndoPlaceSecondSettlement(gs, player, er.VertexId!);
                break;
            case EventRecordAction.GainedLongestRoad:
                UndoFromUser(gs, new UndoRequest(player.Id, er.Id -1), true);
                break;
            case EventRecordAction.PlaceSettlement:
                UndoPlaceSettlement(gs, player, er.VertexId!);
                break;
            case EventRecordAction.UpgradeSettlement:
                UndoUpgradeSettlement(gs, player, er.VertexId!);
                break;
            default:
                throw new NotImplementedException($"Unexpected Error. Haven't implemented ReverseAction yet for {er.Action}.");
        }

        if (!stateAlreadyChanged)
        {
            var preActionState = gs.UndoState.Pop();
            gs.Phase = new GamePhase(gs, preActionState.Phase);

            if (preActionState.HasLargestArmyPlayerId == null)
                gs.ClearLargestArmyPlayer();
            else
            {
                var largestArmyPlayer = gs.Players.FirstOrDefault(p => p.Id == preActionState.HasLargestArmyPlayerId);
                if (largestArmyPlayer == null)
                    throw new InvalidOperationException($"Unexpected Error. Couldn't find player who use to have largest army. LargestArmyPlayer: {preActionState.HasLargestArmyPlayerId}");

                gs.AssignLargestArmyToPlayer(largestArmyPlayer);
            }

            if (preActionState.HasLongestRoadPlayerId == null)
                gs.ClearLongestRoadPlayer();
            else
            {
                var longestRoadPlayer = gs.Players.FirstOrDefault(p => p.Id == preActionState.HasLongestRoadPlayerId);
                if (longestRoadPlayer == null)
                    throw new InvalidOperationException($"Unexpected Error. Couldn't find player who use to have longest raod.LongestRoadPlayer: {preActionState.HasLongestRoadPlayerId}");
                    
                gs.AssignLongestRoadToPlayer(longestRoadPlayer);
            }
        }

        gs.UpdatePlayerVictoryPoints();
    }

    private static void UndoPlaceFirstSettlement(GameState gs, Player player, string vertexId)
    {
        var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);

        if (vertex == null)
            throw new InvalidOperationException($"Unexpected Error. Event vertex not found. Vertex: {vertexId}");

        if (vertex.Owner == null || vertex.Building != BuildingType.Settlement || vertex.Owner.Id != player.Id)
            throw new InvalidOperationException($"Unexpected Error. Vertex {vertexId} not in expected state.");

        // Remove settlement itself
        vertex.ClearVertex();

        // Remove Blocked status from neighboring
        foreach(var edge in vertex.Edges)
            foreach(var v2 in edge.Vertices.Where(v => v.Id != vertex.Id))
                v2.ClearVertex();

        GamePlayHelpers.MarkBlockedVertices(gs);
    }

    private static void UndoPlaceSecondSettlement(GameState gs, Player player, string vertexId)
    {
        var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);

        if (vertex == null)
            throw new InvalidOperationException($"Unexpected Error. Event vertex {vertexId} not found.");
        
        var resources = GamePlayHelpers.GetResourcesEarnedOnVertex(gs, vertex);

        UndoPlaceFirstSettlement(gs, player, vertexId);

        GamePlayHelpers.RemoveResourcesFromPlayer(gs, player, resources);
    }

    private static void UndoPlaceSettlement(GameState gs, Player player, string vertexId)
    {
        var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);

        if (vertex == null)
            throw new InvalidOperationException($"Unexpected Error. Event vertex {vertexId} not found.");
        
        UndoPlaceFirstSettlement(gs, player, vertexId);

        player.AssignResources(ResourceType.Wood, 1);
        player.AssignResources(ResourceType.Wool, 1);
        player.AssignResources(ResourceType.Brick, 1);
        player.AssignResources(ResourceType.Grain, 1);
    }

    private static void UndoUpgradeSettlement(GameState gs, Player player, string vertexId)
    {
        var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);

        if (vertex == null)
            throw new InvalidOperationException($"Unexpected Error. Event vertex not found. Vertex: {vertexId}");

        if (vertex.Owner == null || vertex.Building != BuildingType.City || vertex.Owner.Id != player.Id)
            throw new InvalidOperationException($"Unexpected Error. Vertex {vertexId} not in expected state.");

        vertex.DowngradeToSettlement();

        player.AssignResources(ResourceType.Grain, 2);
        player.AssignResources(ResourceType.Ore, 3);
    }

    private static void UndoPlaceRoad(GameState gs, Player player, string edgeId)
    {
        var edge = gs.Edges.FirstOrDefault(e => e.Id == edgeId);

        if (edge == null)
            throw new InvalidOperationException("Unexpected Error. Event edge not found.");

        if (edge.Owner == null)
            throw new InvalidOperationException($"Unexpected Error. Edge {edgeId} not owned.");

        switch (gs.CountRoadsForPlayer(player))
        {
            case 0:
                throw new InvalidOperationException("Unexpected Error. An event was found suggesting a road exists but it doesn't.");
            case 1:
            case 2:
                edge.ClearEdge();
                break;
            default:
                edge.ClearEdge();

                if (gs.Phase.PhaseState != GameStates.SecondDevCardRoad && gs.UndoState.Peek().Phase.PhaseState != GameStates.SecondDevCardRoad)
                {
                    player.AssignResources(ResourceType.Wood, 1);
                    player.AssignResources(ResourceType.Brick, 1);
                }
                break;
        }
    }
}
