using System.Diagnostics.Eventing.Reader;
using GameTest.DTOs;
using GameTest.Models;

namespace GameTest.Services;

public static class UndoHelpers
{
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

        vertex.ClearBuilding();
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
