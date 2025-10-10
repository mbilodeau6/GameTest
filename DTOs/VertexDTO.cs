using GameTest.Models;
using System.Text.Json.Serialization;

namespace GameTest.DTOs;

public class VertexDTO
{
    public string Id { get; init; }
    public string? Building { get; init; }
    public string? PlayerId { get; init; }
    public string? Direction { get; init; }
    public List<string> TileIds { get; init; } = new List<string>();

    public VertexDTO(Vertex vertex)
    {
        if (vertex == null)
            throw new ArgumentNullException(nameof(vertex));

        Id = vertex.Id;
        Building = vertex.Building != null ? vertex.Building.ToString() : null;
        PlayerId = vertex.Owner != null ? vertex.Owner.Id : null;

        if (vertex.Direction != null)
            Direction = vertex.Direction.ToString();

        foreach (var tile in vertex.Tiles)
            TileIds.Add(tile.Id);
    }
    
    // JsonConstructor parameters must match the JSON property names (case-insensitive).
    [JsonConstructor]
    public VertexDTO(string id, string building, string playerId, string direction, List<string>? tileIds = null)
    {
        Id = id ?? string.Empty;
        Building = building;
        PlayerId = playerId;
        Direction = direction;
        foreach (var tileId in tileIds ?? Enumerable.Empty<string>())
            TileIds.Add(tileId);
    }
}