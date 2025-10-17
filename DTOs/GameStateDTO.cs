using System.Text.Json.Serialization;
using GameTest.Models;

namespace GameTest.DTOs;

public class GameStateDTO
{
    public string Id { get; private set; }
    public GameSettingsDTO Settings { get; }

    public List<PlayerDTO> Players { get; } = new();
    public List<TileDTO> Tiles { get; } = new();
    public List<EdgeDTO> Edges { get; } = new();
    public List<VertexDTO> Vertices { get; } = new();
    public string RobberTileId { get; } = string.Empty;

    public string CurrentPlayerId { get; } = string.Empty;
    public string CurrentState { get; } = string.Empty;
    public string HasLongestRoadPlayerId { get; } = string.Empty;
    public string HasLargestArmyPlayerId { get; } = string.Empty;

    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public GameStateDTO(string id, GameSettingsDTO settings, List<PlayerDTO>? players = null,
        List<TileDTO>? tiles = null, List<EdgeDTO>? edges = null,
        List<VertexDTO>? vertices = null, string? robberTileId = null) 
    {
        Id = id ?? string.Empty;
        Settings = settings;
        RobberTileId = robberTileId ?? string.Empty;

        foreach (var player in players ?? Enumerable.Empty<PlayerDTO>())
            Players.Add(player);

        foreach (var tile in tiles ?? Enumerable.Empty<TileDTO>())
            Tiles.Add(tile);

        foreach (var edge in edges ?? Enumerable.Empty<EdgeDTO>())
            Edges.Add(edge);

        foreach (var vertex in vertices ?? Enumerable.Empty<VertexDTO>())
            Vertices.Add(vertex);
    }

    public GameStateDTO(GameState gameState)
    {
        Id = gameState.Id.ToString();
        Settings = new GameSettingsDTO(gameState.Settings);

        foreach (var player in gameState.Players)
            Players.Add(new PlayerDTO(player));

        foreach (var tile in gameState.Tiles)
            Tiles.Add(new TileDTO(tile));

        foreach (var edge in gameState.Edges)
            Edges.Add(new EdgeDTO(edge));

        foreach (var vertex in gameState.Vertices)
            Vertices.Add(new VertexDTO(vertex));

        RobberTileId = gameState.RobberTile.Id;
    }
}