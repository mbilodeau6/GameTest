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

    public string Id { get; init; }
    public Player? Owner { get; init; }
    // TODO: If I want to support ships, may need EdgeType (Road, ShipRoute)

    // References to the two vertices this edge connects (required)
    public Vertex[] Vertices { get; } = new Vertex[2];

    // References to up to two adjacent tiles (nullable)
    public List<Tile> Tiles { get; }

    public HexDirection? Direction { get; }

    private Edge(Tile t1)
    {
        Id = $"E{Interlocked.Increment(ref s_nextId)}";

        Tiles = new List<Tile>();
        Tiles.Add(t1 ?? throw new ArgumentNullException(nameof(t1)));
    }

    // TODO: Remove version that creates new edge with owner once I add method to add/remove roads.
    public Edge(Player owner, Tile t1, Tile t2) : this(t1, t2)
    {
        Owner = owner;
    }

    public Edge(Tile t1, HexDirection direction) : this(t1)
    {
        Direction = direction;
    }

    public Edge(Tile t1, Tile t2) : this(t1)
    {
        if (t2 != null)
            Tiles.Add(t2);
    }

    // TODO: Consider adding methods to add/remove vertices with validation

    public bool ConnectsTiles(string tileId1, string tileId2)
    {
        if (Tiles.Count == 1)
            return false;

        return (Tiles[0].Id == tileId1 && Tiles[1].Id == tileId2) ||
               (Tiles[0].Id == tileId2 && Tiles[1].Id == tileId1);
    }

    public override string ToString()
    {
        return $"Edge {Id} (Owner: {Owner.Name})";
    }
}