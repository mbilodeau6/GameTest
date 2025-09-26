using System.Collections.Generic;

namespace GameTest.Models;

public class GameState
{
    public List<Player> Players { get; } = new();
    public List<Tile> Tiles { get; } = new();
    public List<Edge> Edges { get; } = new();
    public List<Vertex> Vertices { get; } = new();

    // Future: Add collections for Ports

    public GameState() { }

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
}