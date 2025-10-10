using GameTest.Models;

namespace GameTest.DTOs;

public class EdgeDTO
{
    public string Id { get; private set; }
    public string? PlayerId { get; private set; }
    public HexDirection? Direction { get; init; }
    public string[] TileIds { get; init; }

    public EdgeDTO(Edge edge)
    {
        Id = edge.Id;

        if (edge.Owner != null)
            PlayerId = edge.Owner.Id;

        if (edge.Direction != null)
            Direction = edge.Direction;

        var tileIdList = new List<string>();

        foreach (var tile in edge.Tiles)
            tileIdList.Add(tile.Id);

        TileIds = tileIdList.ToArray();
    }
}
