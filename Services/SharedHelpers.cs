using GameTest.Models;

namespace GameTest.Services;

public static class SharedHelpers
{
    public static int NextRandom()
    {
        return Random.Shared.Next();
    }

    public static int NextRandom(int upper)
    {
        return Random.Shared.Next(upper);
    }

    public static int NextRandom(int lower, int upper)
    {
        return Random.Shared.Next(lower, upper);
    }
}
