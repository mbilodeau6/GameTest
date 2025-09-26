using System;
using System.Threading;

namespace GameTest.Models;

/// <summary>
/// Represents an edge (where a road can be built) between two vertices and adjacent to up to two tiles.
/// The arrays are length-2; index order has no enforced semantic meaning.
/// </summary>
public class Edge
{
    private static int s_nextId;

    // TODO: Switch to string
    public int Id { get; init; }
    public bool HasRoad { get; private set; } = false;
    public Player? Owner { get; set; } = null;

    // References to the two vertices this edge connects (required)
    public Vertex[] Vertices { get; } = new Vertex[2];

    // References to up to two adjacent tiles (nullable)
    public Tile?[] Tiles { get; } = new Tile?[2];

    public Edge()
    {
        Id = Interlocked.Increment(ref s_nextId);
    }

    // TODO: Consider adding methods to add/remove vertices, tiles and owner, with validation

    public void BuildRoad()
    {
        HasRoad = true;  
    }

    public void RemoveRoad() {
      HasRoad = false;  
    }

    public override string ToString()
    {
        // string v0 = Vertices[0]?.ToString() ?? "null";
        // string v1 = Vertices[1]?.ToString() ?? "null";
        // string t0 = Tiles[0]?.ToString() ?? "-";
        // string t1 = Tiles[1]?.ToString() ?? "-";
        // return $"Edge #{Id}: V[{v0},{v1}] T[{t0},{t1}] Road:{HasRoad}";
        return string.Empty;
    }
}