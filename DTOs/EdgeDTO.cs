using GameTest.Models;

namespace GameTest.DTOs;

public class EdgeDTO
{
    public string Id { get; private set; }
    public string? PlayerId { get; private set; }
    public string[] TileIds { get; init; }

    public EdgeDTO(Edge edge)
    {
        Id = edge.Id;

        if (edge.Owner != null)
            PlayerId = edge.Owner.Id;

        var tileIdList = new List<string>();

        foreach (var tile in edge.Tiles)
            tileIdList.Add(tile.Id);

        TileIds = tileIdList.ToArray();
    }
}
