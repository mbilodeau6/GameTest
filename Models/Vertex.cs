using System;
using System.Threading;

namespace GameTest.Models;

/// <summary>
/// A vertex where a settlement or city can be built.
/// Holds references to up to two edges and up to three adjacent tiles.
/// </summary>

public class Vertex
{
    private static int s_nextId;

    public string Id { get; init; }
    public BuildingType? Building { get; private set; }
    public Player? Owner { get; set; }
    public VertexDirection? Direction { get; init; }


    // Up to two edges that meet at this vertex
    public Edge?[] Edges { get; } = new Edge?[2];

    // Up to three tiles that touch this vertex
    public List<Tile> Tiles { get; }

    private Vertex(Tile tile1)
    {
        Id = $"V{Interlocked.Increment(ref s_nextId)}";
        Tiles = new List<Tile>();
        Tiles.Add(tile1 ?? throw new ArgumentNullException(nameof(tile1)));
    }

    public Vertex(Tile tile1, VertexDirection direction) : this(tile1)
    {
        Direction = direction;
    }

    public Vertex(Tile tile1, Tile tile2, Tile? tile3 = null) : this(tile1)
    {
        Tiles.Add(tile2 ?? throw new ArgumentNullException(nameof(tile2)));

        if (tile3 != null)
            Tiles.Add(tile3);
    }

    // TODO: Remove version that creates new vertex with owner once I add method to add/remove settlements/cities.
    public Vertex(Player owner, Tile tile1, Tile? tile2 = null, Tile? tile3 = null) : this(tile1)
    {
        Owner = owner;
        Building = BuildingType.Settlement;

        if (tile2 != null)
            Tiles.Add(tile2);

        if (tile3 != null)
            Tiles.Add(tile3);
    }

    // TODO: Consider adding methods to add/remove edges with validation

    public void UpgradeToCity()
    {
        if (Building != BuildingType.Settlement)
            throw new InvalidOperationException("Only a settlement can be upgraded to a city.");
        Building = BuildingType.City;
    }

    public void DowngradeToSettlement()
    {
        if (Building != BuildingType.City)
            throw new InvalidOperationException("Only a city can be downgraded to a settlement.");
        Building = BuildingType.Settlement;
    }

    public Boolean ConnectsTiles(Tile tile1, Tile tile2, Tile? tile3 = null)
    {
        if (tile3 == null && Tiles.Count == 3)
            return false;

        bool hasTile1 = Tiles.Any(t => t.Id == tile1.Id);
        bool hasTile2 = Tiles.Any(t => t.Id == tile2.Id);
        bool hasTile3 = tile3 == null || Tiles.Any(t => t.Id == tile3.Id);

        return hasTile1 && hasTile2 && hasTile3;
    }

    public override string ToString()
    {
        string buildingType = Building == BuildingType.Settlement ? "S" : "C";
        string result = $"Vertex {Id} (Owner: {Owner.Name}; Building: {buildingType})";

        return result;
    }
}