using System.Collections.Generic;

namespace GameTest.Models;

public class GameState
{
    public Guid Id { get; init; }
    public GameType Type { get; init; } = GameType.Default;

    // TODO: I keep on going back and forth on whether I should only store occupied edges/vertices
    // or all edges/vertices in the game. Right now I'm only storing occupied ones. Thinking about
    // changing but need to check with Eric.

    // TODO: Also realized I need to come up with a standard for associating edge/vertex indexes
    // with the tiles. Current thought is top starts at top and goes clockwise.
    public List<Player> Players { get; } = new();
    public List<Tile> Tiles { get; } = new();
    public List<Edge> Edges { get; } = new();
    public List<Vertex> Vertices { get; } = new();
    public string RobberTileId { get; set; } = string.Empty;

    // Future: Add collections for Ports

    public GameState(Guid guid)
    {
        Id = guid;
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
}