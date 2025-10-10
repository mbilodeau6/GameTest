using System.Text.Json.Serialization;
using GameTest.Models;

namespace GameTest.DTOs;

public class GameStateDTO
{
    public string Id { get; private set; }
    public string Type { get; private set; }

    // TODO: I keep on going back and forth on whether I should only store occupied edges/vertices
    // or all edges/vertices in the game. Right now I'm only storing occupied ones. Thinking about
    // changing but need to check with Eric.
    public List<PlayerDTO> Players { get; } = new();
    public List<TileDTO> Tiles { get; } = new();
    public List<EdgeDTO> Edges { get; } = new();
    public List<VertexDTO> Vertices { get; } = new();
    public string RobberTileId { get; } = string.Empty;

    // TODO: Need to populate all collections in the DTO (players, tiles, edges, vertices)
    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public GameStateDTO(string id, string type, List<PlayerDTO>? players =  null, string? robberTileId = null) 
    {
        Id = id ?? string.Empty;
        Type = type ?? string.Empty;
        RobberTileId = robberTileId ?? string.Empty;

        foreach (var player in players ?? Enumerable.Empty<PlayerDTO>())
            Players.Add(player);
    }

    public GameStateDTO(GameState gameState)
    {
        Id = gameState.Id.ToString();
        Type = gameState.Type.ToString();

        foreach (var player in gameState.Players)
            Players.Add(new PlayerDTO(player));

        foreach (var tile in gameState.Tiles)
            Tiles.Add(new TileDTO(tile));

        foreach (var edge in gameState.Edges)
            Edges.Add(new EdgeDTO(edge));

        foreach (var vertex in gameState.Vertices)
            Vertices.Add(new VertexDTO(vertex));

        RobberTileId = gameState.RobberTileId;
    }
}