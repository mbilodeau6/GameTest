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

    // TODO: Switch to string
    public int Id { get; init; }
    public BuildingType Building { get; private set; } = BuildingType.None;
    public Player? Owner { get; set; } = null;


    // Up to two edges that meet at this vertex
    public Edge?[] Edges { get; } = new Edge?[2];

    // Up to three tiles that touch this vertex
    public Tile?[] Tiles { get; } = new Tile?[3];

    // Parameterless ctor for serializers
    public Vertex()
    {
        Id = Interlocked.Increment(ref s_nextId);
    }

    // TODO: Consider adding methods to add/remove edges, tiles and owner, with validation

    public void BuildSettlement()
    {
        if (Building != BuildingType.None)
            throw new InvalidOperationException("A building already exists on this vertex.");
        Building = BuildingType.Settlement;
    }

    public void UpgradeToCity()
    {
        if (Building != BuildingType.Settlement)
            throw new InvalidOperationException("Only a settlement can be upgraded to a city.");
        Building = BuildingType.City;
    }

    public void RemoveBuilding()
    {
       Building = BuildingType.None; 
    }

    public override string ToString()
    {
        // string e0 = Edges[0]?.Id.ToString() ?? "-";
        // string e1 = Edges[1]?.Id.ToString() ?? "-";
        // string t0 = Tiles[0]?.Id.ToString() ?? "-";
        // string t1 = Tiles[1]?.Id.ToString() ?? "-";
        // string t2 = Tiles[2]?.Id.ToString() ?? "-";
        // return $"Vertex #{Id} Building:{Building} Edges:[{e0},{e1}] Tiles:[{t0},{t1},{t2}]";
        return string.Empty;
    }
}