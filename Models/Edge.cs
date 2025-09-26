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
    public Player Owner { get; init; }
    // TODO: If I want to support ships, may need EdgeType (Road, ShipRoute)

    // References to the two vertices this edge connects (required)
    public Vertex[] Vertices { get; } = new Vertex[2];

    // References to up to two adjacent tiles (nullable)
    public Tile[] Tiles { get; }

    public Edge(Player owner, Tile t1, Tile? t2 = null)
    {
        Id = $"E{Interlocked.Increment(ref s_nextId)}";

        var tileList = new List<Tile> { t1 ?? throw new ArgumentNullException(nameof(t1)) };

        if (t2 != null)
            tileList.Add(t2);

        // Convert the list to an array, ensuring no nulls
        Tiles = tileList.ToArray();

        Tiles[0] = t1;

        if (t2 != null)
            Tiles[1] = t2;

        Owner = owner;
    }

    // TODO: Consider adding methods to add/remove vertices, tiles and owner, with validation

    public override string ToString()
    {
        return $"Edge {Id} (Owner: {Owner.Name})";
    }
}