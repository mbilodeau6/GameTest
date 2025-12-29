using GameTest.Models;

namespace GameTest.DTOs;

public class PossiblePlayerAction
{
    public PlayerAction Action { get; init; }
    
    // For PlaceSettlement, UpgradeSettlement - valid vertex locations
    public List<string>? VertexIds { get; init; }
    
    // For PlaceRoad - valid edge locations
    public List<string>? EdgeIds { get; init; }
    
    // For PlaceRobber, PlayKnight - valid tile locations
    public List<string>? TileIds { get; init; }
    
    // For SelectTarget - valid player IDs to steal from
    // For AcceptTrade - player IDs who have accepted/countered the offer
    public List<string>? PlayerIds { get; init; }

    // For Undo - The EventRecord Id that can be undone
    public int? EventId { get; init; }
}