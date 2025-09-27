using GameTest.Models;

namespace GameTest.DTOs;

public class TileDTO
{
    public string Id { get; init; }
    public string Resource { get; init; }
    public int DiceNumber { get; init; }
    public bool HasRobber { get; init; }
    public int X { get; init; }
    public int Y { get; init; }

    public TileDTO(Tile tile)
    {
        Id = tile.Id;
        Resource = tile.Resource.ToString();
        DiceNumber = tile.DiceNumber;
        HasRobber = tile.HasRobber;
        X = tile.X;
        Y = tile.Y;
    }
}
