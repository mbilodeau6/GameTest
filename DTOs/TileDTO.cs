using GameTest.Models;
using System.Text.Json.Serialization;

namespace GameTest.DTOs;

public class TileDTO
{
    public string Id { get; init; }
    public string Resource { get; init; }
    public int DiceNumber { get; init; }
    public int X { get; init; }
    public int Y { get; init; }

    public TileDTO(Tile tile)
    {
        Id = tile.Id;
        Resource = tile.Resource.ToString();
        DiceNumber = tile.DiceNumber;
        X = tile.X;
        Y = tile.Y;
    }

    // JsonConstructor parameters must match the JSON property names (case-insensitive).
    [JsonConstructor]
    public TileDTO(string id, string resource, int diceNumber, int x, int y)
    {
        Id = id ?? string.Empty;
        Resource = resource ?? string.Empty;
        DiceNumber = diceNumber;
        X = x;
        Y = y;
    }
}
