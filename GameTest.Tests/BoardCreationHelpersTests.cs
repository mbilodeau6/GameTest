using Xunit;
using GameTest.Services;
using GameTest.Models;

namespace GameTest.Tests;

public static class TileTestHelpers
{
    public static Tile GetRequiredTileAt(this IEnumerable<Tile> tiles, int x, int y)
       => tiles.First(t => t.X == x && t.Y == y);
}

public class BoardCreationHelpersTests
{
    private void ValidateGeneralRulesForDefaultBoard(List<Tile> tiles)
    {
        Assert.Equal(19, tiles.Count);

        Dictionary<ResourceType, int> resourceCounts = new Dictionary<ResourceType, int>();
        Dictionary<int, int> diceCounts = new Dictionary<int, int>();

        foreach (var tile in tiles)
        {
            if (resourceCounts.ContainsKey(tile.Resource))
            {
                resourceCounts[tile.Resource]++;
            }
            else
            {
                resourceCounts[tile.Resource] = 1;
            }

            if (tile.Resource != ResourceType.Desert)
            {
                if (diceCounts.ContainsKey(tile.DiceNumber))
                {
                    diceCounts[tile.DiceNumber]++;
                }
                else
                {
                    diceCounts[tile.DiceNumber] = 1;
                }
            }
        }

        Assert.Equal(3, resourceCounts.GetValueOrDefault(ResourceType.Brick, 0));
        Assert.Equal(4, resourceCounts.GetValueOrDefault(ResourceType.Wood, 0));
        Assert.Equal(4, resourceCounts.GetValueOrDefault(ResourceType.Wool, 0));
        Assert.Equal(4, resourceCounts.GetValueOrDefault(ResourceType.Grain, 0));
        Assert.Equal(3, resourceCounts.GetValueOrDefault(ResourceType.Ore, 0));
        Assert.Equal(1, resourceCounts.GetValueOrDefault(ResourceType.Desert, 0));
        Assert.Equal(1, diceCounts.GetValueOrDefault(2, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(3, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(4, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(5, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(6, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(8, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(9, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(10, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(11, 0));
        Assert.Equal(1, diceCounts.GetValueOrDefault(12, 0));
        Assert.False(diceCounts.ContainsKey(7));
        Assert.False(diceCounts.ContainsKey(0));
    }

    [Fact]
    public void CreateTilesForRandomBoard_CorrectTiles()
    {
        // Act
        var tiles = BoardCreationHelpers.CreateTilesForRandomBoard();

        // Assert
        ValidateGeneralRulesForDefaultBoard(tiles);
    }

    [Fact]
    public void CreateTilesForStarterBoard_CorrectTiles()
    {
        // Act
        var tiles = BoardCreationHelpers.CreateTilesForStarterBoard();

        // Assert
        ValidateGeneralRulesForDefaultBoard(tiles);
        Assert.Equal(ResourceType.Desert, tiles.GetRequiredTileAt(0, 0).Resource);
        Assert.Equal(ResourceType.Brick, tiles.GetRequiredTileAt(-1, -1).Resource);
        Assert.Equal(6, tiles.GetRequiredTileAt(-1, -1).DiceNumber);
        Assert.Equal(ResourceType.Grain, tiles.GetRequiredTileAt(0, 2).Resource);
        Assert.Equal(6, tiles.GetRequiredTileAt(0, 2).DiceNumber);
        Assert.Equal(ResourceType.Ore, tiles.GetRequiredTileAt(4, 0).Resource);
        Assert.Equal(8, tiles.GetRequiredTileAt(4, 0).DiceNumber);
        Assert.Equal(ResourceType.Wood, tiles.GetRequiredTileAt(-3, 1).Resource);
        Assert.Equal(8, tiles.GetRequiredTileAt(-3, 1).DiceNumber);
        Assert.Equal(ResourceType.Wool, tiles.GetRequiredTileAt(0, -2).Resource);
        Assert.Equal(2, tiles.GetRequiredTileAt(0, -2).DiceNumber);
    }
}