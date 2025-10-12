using GameTest.DTOs;

namespace GameTest.Models;

public class Tile
{
    private static int s_nextId = 0;

    public string Id { get; init; }
    public ResourceType Resource { get; private set; }
    public int DiceNumber { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }

    public Tile(ResourceType resource, int diceNumber, int x, int y)
    {
        Id = $"T{Interlocked.Increment(ref s_nextId)}";

        if (resource != ResourceType.Desert && diceNumber < 2 || diceNumber > 12)
            throw new ArgumentException("Dice number must be between 2 and 12", nameof(diceNumber));

        Resource = resource;
        DiceNumber = resource == ResourceType.Desert ? 7 : diceNumber;
        X = x;
        Y = y;
    }

    public Tile(TileDTO dto)
    {
        Id = dto.Id;
        Resource = Enum.Parse<ResourceType>(dto.Resource);
        DiceNumber = dto.DiceNumber;
        X = dto.X;
        Y = dto.Y;
    }

    public override string ToString()
    {
        return $"{Resource} ({X},{Y})({DiceNumber})";
    }
}