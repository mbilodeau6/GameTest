using GameTest.Models;
using System.Text.Json.Serialization;

namespace GameTest.DTOs;

public class VertexDTO
{
    public string Id { get; init; }
    public string? Building { get; init; }
    public string? PlayerId { get; init; }
    public VertexDirection? Direction { get; init; }
    public string[] TileIds { get; init; }

    public VertexDTO(Vertex vertex)
    {
        if (vertex == null)
            throw new ArgumentNullException(nameof(vertex));

        Id = vertex.Id;
        Building = vertex.Building != null ? vertex.Building.ToString() : null;
        PlayerId = vertex.Owner != null ? vertex.Owner.Id : null;

        if (vertex.Direction != null)
            Direction = vertex.Direction;

        var tileIdList = new List<string>();

        foreach (var tile in vertex.Tiles)
            tileIdList.Add(tile.Id);

        TileIds = tileIdList.ToArray();
    }
    
    // JsonConstructor parameters must match the JSON property names (case-insensitive).
    [JsonConstructor]
    public VertexDTO(string id, string building, string playerId, VertexDirection vertexDirection, string[] tileIds)
    {
        Id = id ?? string.Empty;
        Building = building;
        PlayerId = playerId;
        Direction = vertexDirection;
        TileIds = tileIds ?? Array.Empty<string>();
    }
}