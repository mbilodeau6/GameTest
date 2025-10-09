namespace GameTest.Services;

public class HexProximity
{
    public enum Direction
    {
        NE, E, SE, SW, W, NW
    }

    public static (int, int) GetDirectionOffset(Direction dir)
    {
        return dir switch
        {
            Direction.NE => (1, -1),
            Direction.E => (2, 0),
            Direction.SE => (1, 1),
            Direction.SW => (-1, 1),
            Direction.W => (-2, 0),
            Direction.NW => (-1, -1),
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    public static (int, int) GetCoordinates((int, int) a, Direction dir)
    {
        var offset = GetDirectionOffset(dir);
        return (a.Item1 + offset.Item1, a.Item2 + offset.Item2);
    }

    public static Direction getPrecedingDirection(Direction dir)
    {
        return dir == Direction.NE ? Direction.NW : dir - 1;
    }
}