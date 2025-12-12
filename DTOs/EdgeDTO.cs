using GameTest.Models;
using System.Text.Json.Serialization;

namespace GameTest.DTOs;

public class EdgeDTO
{
    public string Id { get; private set; }
    public string? PlayerId { get; private set; }
    public HexDirection? Direction { get; init; }
    public List<string> TileIds { get; init; } = new List<string>();

    public EdgeDTO(Edge edge)
    {
        Id = edge.Id;

        if (edge.Owner != null)
            PlayerId = edge.Owner.Id;

        if (edge.Direction != null)
            Direction = edge.Direction;

        foreach (var tile in edge.Tiles)
            TileIds.Add(tile.Id);
    }

    // JsonConstructor parameters must match the JSON property names (case-insensitive).
    [JsonConstructor]
    public EdgeDTO(string id, string playerId, HexDirection? direction = null, List<string>? tileIds = null)
    {
        Id = id ?? string.Empty;
        PlayerId = playerId;
        Direction = direction;
        foreach (var tileId in tileIds ?? Enumerable.Empty<string>())
            TileIds.Add(tileId);
    }
}