using GameTest.Models;


namespace GameTest.Services;

public static class BoardCreationHelpers
{
    public static List<Tile> CreateTilesForRandomBoard()
    {
        var resourceValues = new List<ResourceType>();
        resourceValues.AddRange(Enumerable.Repeat(ResourceType.Brick, 3));
        resourceValues.AddRange(Enumerable.Repeat(ResourceType.Wood, 4));
        resourceValues.AddRange(Enumerable.Repeat(ResourceType.Wool, 4));
        resourceValues.AddRange(Enumerable.Repeat(ResourceType.Grain, 4));
        resourceValues.AddRange(Enumerable.Repeat(ResourceType.Ore, 3));
        resourceValues.Add(ResourceType.Desert);

        var rnd = new Random();
        for (int i = resourceValues.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            var tmp = resourceValues[i];
            resourceValues[i] = resourceValues[j];
            resourceValues[j] = tmp;
        }

        var diceValues = new List<int>
        {
            5, 2, 6, 3, 8, 10, 9, 12, 11, 4, 8, 10, 9, 4, 5, 6, 3, 11
        };

        var xCoordinates = new List<int>
        {
            -1, -2, -2, -2, -1, 0, 1, 2, 2, 2, 1, 0, -1, -1, -1, 1, 1, 1, 0
        };

        var yCoordinates = new List<int>
        {
            -2, -1, 0, 1, 2, 2, 2, 1, 0, -1, -2, -2, -1, 0, 1, 1, 0, -1, 0
        };

        int diceIndex = 0;
        int coordIndex = 0;

        List<Tile> tiles = new List<Tile>();

        foreach (var resourceType in resourceValues)
        {
            if (resourceType != ResourceType.Desert)
            {
                tiles.Add(new Tile(resourceType, diceValues[diceIndex], xCoordinates[coordIndex], yCoordinates[coordIndex]));
                diceIndex++;
            }
            else
            {
                tiles.Add(new Tile(resourceType, 7, xCoordinates[coordIndex], yCoordinates[coordIndex]));
            }
            coordIndex++;
        }

        return tiles;
    }

    public static List<Tile> CreateTilesForStarterBoard()
    {
        List<Tile> tiles = new List<Tile>
        {
            new Tile(ResourceType.Ore, 10, -1, -2),
            new Tile(ResourceType.Grain, 12, -2, -1),
            new Tile(ResourceType.Grain, 9, -2, 0),
            new Tile(ResourceType.Wood, 8, -2, 1),
            new Tile(ResourceType.Brick, 5, -1, 2),
            new Tile(ResourceType.Grain, 6, 0, 2),
            new Tile(ResourceType.Wool, 11, 1, 2),
            new Tile(ResourceType.Wool, 5, 2, 1),
            new Tile(ResourceType.Ore, 8, 2, 0),
            new Tile(ResourceType.Brick, 10, 2, -1),
            new Tile(ResourceType.Wood, 9, 1, -2),
            new Tile(ResourceType.Wool, 2, 0, -2),
            new Tile(ResourceType.Brick, 6, -1, -1),
            new Tile(ResourceType.Wood, 11, -1, 0),
            new Tile(ResourceType.Ore, 3, -1, 1),
            new Tile(ResourceType.Grain, 4, 1, 1),
            new Tile(ResourceType.Wood, 3, 1, 0),
            new Tile(ResourceType.Wool, 4, 1, -1),
            new Tile(ResourceType.Desert, 7, 0, 0), // Desert tile
        };

        return tiles;
    }

}