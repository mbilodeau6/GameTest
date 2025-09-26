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
    public BuildingType Building { get; private set; }
    public Player Owner { get; set; }


    // Up to two edges that meet at this vertex
    public Edge?[] Edges { get; } = new Edge?[2];

    // Up to three tiles that touch this vertex
    public Tile[] Tiles { get; }

    // Parameterless ctor for serializers
    public Vertex(Player owner, Tile tile1, Tile? tile2 = null, Tile? tile3 = null)
    {
        Owner = owner;
        Building = BuildingType.Settlement;

        // Use a list to collect non-null tiles
        var tileList = new List<Tile> { tile1 ?? throw new ArgumentNullException(nameof(tile1)) };

        if (tile2 != null)
            tileList.Add(tile2);

        if (tile3 != null)
            tileList.Add(tile3);

        Tiles = tileList.ToArray();

        Id = $"V{Interlocked.Increment(ref s_nextId)}";
    }

    // TODO: Consider adding methods to add/remove edges, tiles and owner, with validation

    public void UpgradeToCity()
    {
        if (Building != BuildingType.Settlement)
            throw new InvalidOperationException("Only a settlement can be upgraded to a city.");
        Building = BuildingType.City;
    }

    public void DowngradeToSettlement()
    {
        if (Building != BuildingType.City)
            throw new InvalidOperationException("Only a settlement can be upgraded to a city.");
        Building = BuildingType.Settlement;
    }

    public override string ToString()
    {
        string buildingType = Building == BuildingType.Settlement ? "S" : "C";
        string result = $"Vertex {Id} (Owner: {Owner.Name}; Building: {buildingType})";

        return result;
    }
}