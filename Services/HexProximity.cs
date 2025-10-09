using GameTest.Models;

namespace GameTest.Services;

public class HexProximity
{
    public static (int, int) GetDirectionOffset(HexDirection dir)
    {
        return dir switch
        {
            HexDirection.NE => (1, -1),
            HexDirection.E => (2, 0),
            HexDirection.SE => (1, 1),
            HexDirection.SW => (-1, 1),
            HexDirection.W => (-2, 0),
            HexDirection.NW => (-1, -1),
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    public static (int, int) GetCoordinates((int, int) a, HexDirection dir)
    {
        var offset = GetDirectionOffset(dir);
        return (a.Item1 + offset.Item1, a.Item2 + offset.Item2);
    }

    public static HexDirection getPrecedingDirection(HexDirection dir)
    {
        return dir == HexDirection.NE ? HexDirection.NW : dir - 1;
    }
}