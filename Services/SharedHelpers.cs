using GameTest.Models;

namespace GameTest.Services;

public static class SharedHelpers
{
    private static readonly Random _random = new();

    public static int NextRandom()
    {
        return _random.Next();
    }

    public static int NextRandom(int upper)
    {
        return _random.Next(upper);
    }

    public static int NextRandom(int lower, int upper)
    {
        return _random.Next(lower, upper);
    }
}
