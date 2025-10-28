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

    public static VertexDirection GetVertexDirectionForEdgeDirection(HexDirection edgeDir)
    {
        return edgeDir switch
        {
            HexDirection.NE => VertexDirection.N,
            HexDirection.E => VertexDirection.NE,
            HexDirection.SE => VertexDirection.SE,
            HexDirection.SW => VertexDirection.S,
            HexDirection.W => VertexDirection.SW,
            HexDirection.NW => VertexDirection.NW,
            _ => throw new ArgumentOutOfRangeException(nameof(edgeDir), edgeDir, null)
        };
    }

    public static HexDirection GetHexDirectionForVertexDirection(VertexDirection dir)
    {
        return dir switch
        {
            VertexDirection.N => HexDirection.NE,
            VertexDirection.NE => HexDirection.E,
            VertexDirection.SE => HexDirection.SE,
            VertexDirection.S => HexDirection.SW,
            VertexDirection.SW => HexDirection.W,
            VertexDirection.NW => HexDirection.NW,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }
}