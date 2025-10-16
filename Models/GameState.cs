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
    public Tile RobberTile { get; private set; } = null!;
    public Player PlayerWithLongestRoad { get; private set; } = null!;
    public Player PlayerWithLargestArmy { get; private set; } = null!;

    public Dictionary<ResourceType, int> Resources { get; } = new()
    {
        { ResourceType.Brick, 0 },
        { ResourceType.Wood, 0 },
        { ResourceType.Ore, 0 },
        { ResourceType.Grain, 0 },
        { ResourceType.Wool, 0 }
    };

    public List<DevelopmentCardType> DevelopmentCards { get; } = new List<DevelopmentCardType>();

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
        {
            Tile tile = new Tile(tileDto);
            Tiles.Add(new Tile(tileDto));
            if (tile.Id == dto.RobberTileId)
                RobberTile = tile;
        }

        foreach (var edgeDto in dto.Edges)
            Edges.Add(new Edge(edgeDto, Players, Tiles));

        foreach (var vertexDto in dto.Vertices)
            Vertices.Add(new Vertex(vertexDto, Players, Tiles));
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

    public void SetRobberTile(Tile tile)
    {
        if (tile.Equals(RobberTile))
            throw new ArgumentException("Robber is already on the specified tile.");

        if (!Tiles.Contains(tile))
            throw new ArgumentException("The specified tile does not exist in the game.");

        RobberTile = tile;
    }
    
    public void PlaceRobberOnDesert()
    {
        var desertTile = Tiles.FirstOrDefault(t => t.Resource == ResourceType.Desert);
        if (desertTile == null)
            throw new InvalidOperationException("No desert tile found in the game.");

        RobberTile = desertTile;
    }
}