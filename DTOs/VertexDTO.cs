using GameTest.Models;

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
}