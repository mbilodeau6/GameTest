using System.Collections.Generic;
using GameTest.DTOs;

namespace GameTest.Models;

public class GameState
{
    public Guid Id { get; init; }
    public GameType Type { get; init; }
    public List<Player> Players { get; } = new();
    public List<Tile> Tiles { get; } = new();
    public List<Edge> Edges { get; } = new();
    public List<Vertex> Vertices { get; } = new();
    public string RobberTileId { get; set; } = string.Empty;

    // Future: Add collections for Ports

    public GameState(Guid guid, GameType? type = GameType.Default)
    {
        Id = guid;
        Type = type ?? GameType.Default;
    }

    public GameState(GameStateDTO dto)
    {
        Id = Guid.Parse(dto.Id);
        Type = Enum.Parse<GameType>(dto.Type);

        foreach (var playerDto in dto.Players)
            Players.Add(new Player(playerDto));

        foreach (var tileDto in dto.Tiles)
            Tiles.Add(new Tile(tileDto));

        foreach (var edgeDto in dto.Edges)
            Edges.Add(new Edge(edgeDto, Players, Tiles));

        foreach (var vertexDto in dto.Vertices)
            Vertices.Add(new Vertex(vertexDto, Players, Tiles));

        RobberTileId = dto.RobberTileId;
    }

    public void AddPlayer(Player player)
    {
        Players.Add(player);
    }

    public void AddTile(Tile tile)
    {
        Tiles.Add(tile);
    }

    public void AddEdge(Edge edge)
    {
        Edges.Add(edge);
    }

    public void AddVertex(Vertex vertex)
    {
        Vertices.Add(vertex);
    }

    public void SetRobberTile(string tileId)
    {
        if (tileId.Equals(RobberTileId, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Robber is already on the specified tile.");

        if (!Tiles.Any(t => t.Id == tileId))
            throw new ArgumentException("The specified tile does not exist in the game.");

        RobberTileId = tileId;
    }
    
    public void PlaceRobberOnDesert()
    {
        var desertTile = Tiles.FirstOrDefault(t => t.Resource == ResourceType.Desert);
        if (desertTile == null)
            throw new InvalidOperationException("No desert tile found in the game.");

        RobberTileId = desertTile.Id;
    }
}