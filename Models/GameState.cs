using System.Collections.Generic;

namespace GameTest.Models;

public class GameState
{
    public List<Player> Players { get; } = new();
    public List<Tile> Tiles { get; } = new();

    // Future: Add collections for Edge, Vertex, Port, etc.

    public GameState() { }

    public void AddPlayer(Player player)
    {
        Players.Add(player);
    }

    public void AddTile(Tile tile)
    {
        Tiles.Add(tile);
    }
}