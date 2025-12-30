using System.Diagnostics.Eventing.Reader;
using System.Numerics;
using GameTest.DTOs;
using GameTest.Models;

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

    // Valid event actions that can be undone
    private static readonly HashSet<EventRecordAction> UndoValidEventActions = new()
    {
        EventRecordAction.PlaceFirstSettlement,
        EventRecordAction.PlaceSecondSettlement,
        EventRecordAction.PlaceSettlement,
        EventRecordAction.UpgradeSettlement,
        EventRecordAction.PlaceRoad,
        EventRecordAction.DiscardCards,
        EventRecordAction.PlayYearOfPlenty,
        EventRecordAction.PlayRoadBuilding,
        EventRecordAction.TradeWithBank
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

        // Validate the event action is undoable
        if (!UndoValidEventActions.Contains(eventToUndo.Action))
            return new UndoValidationResponse { UndoPossible = false, ErrorCode = 1072, Player = player, Event = eventToUndo };

        // Validate no player has acted since this event
        List<EventRecordDTO> copyOfEventRecords = gs.EventRecord.Where(e => e.Id > request.EventId).ToList();
        foreach(var undoEvent in gs.EventRecord.Where(e => e.Id > request.EventId && e.Action == EventRecordAction.Undo))
        {
            copyOfEventRecords.RemoveAll(e => e.Id == undoEvent.Id);
            copyOfEventRecords.RemoveAll(e => e.Id == undoEvent.EventReversed);
        }

        if (copyOfEventRecords.Any())
            return new UndoValidationResponse { UndoPossible = false, ErrorCode = 1074, Player = player, Event = eventToUndo };

        return new UndoValidationResponse { UndoPossible = true, Player = player, Event = eventToUndo };
    }

    public static void ReverseAction(GameState gs, Player player, EventRecordDTO er)
    {
        if (player.Id != er.PlayerId)
            throw new InvalidOperationException("Unexpected Error. Player doesn't match event player.");

        if (gs.Phase.CurrentPlayer != null && 
                player.Id != gs.Phase.CurrentPlayer.Id && 
                player.Id != gs.Phase.GetPreviousPlayer(gs.Phase.CurrentPlayer, gs.Players).Id)
            throw new InvalidOperationException("Unexpected Error. Player doesn't match current or previous player.");

        switch(er.Action)
        {
            case EventRecordAction.PlaceFirstSettlement:
                UndoPlaceFirstSettlement(gs, player, er.VertexId!);
                break;
            case EventRecordAction.PlaceRoad:
                UndoPlaceRoad(gs, player, er.EdgeId!);
                break;
            default:
                throw new NotImplementedException("Unexpected Error. Haven't implemented ReverseAction yet.");
        }
    }

    private static void UndoPlaceFirstSettlement(GameState gs, Player player, string vertexId)
    {
        var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);

        if (vertex == null)
            throw new InvalidOperationException("Unexpected Error. Event vertex not found.");

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
                throw new NotImplementedException("Haven't implemented UndoPlaceRoad beyond initial set up.");
        }
    }
}
