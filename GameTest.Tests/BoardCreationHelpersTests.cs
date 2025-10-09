using Xunit;
using GameTest.Services;
using GameTest.Models;

namespace GameTest.Tests;

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

    [Fact]
    public void CreateTilesForTestBoard_CorrectTiles()
    {
        // Act
        var tiles = BoardCreationHelpers.CreateTilesForTestBoard();

        // Assert
        Assert.Equal(7, tiles.Count);
        Assert.Equal(ResourceType.Desert, tiles.GetRequiredTileAt(-1, -1).Resource);
        Assert.Equal(ResourceType.Wool, tiles.GetRequiredTileAt(1, -1).Resource);
        Assert.Equal(8, tiles.GetRequiredTileAt(1, -1).DiceNumber);
        Assert.Equal(ResourceType.Brick, tiles.GetRequiredTileAt(-2, 0).Resource);
        Assert.Equal(5, tiles.GetRequiredTileAt(-2, 0).DiceNumber);
        Assert.Equal(ResourceType.Grain, tiles.GetRequiredTileAt(0, 0).Resource);
        Assert.Equal(10, tiles.GetRequiredTileAt(0, 0).DiceNumber);
        Assert.Equal(ResourceType.Ore, tiles.GetRequiredTileAt(2, 0).Resource);
        Assert.Equal(3, tiles.GetRequiredTileAt(2, 0).DiceNumber);
        Assert.Equal(ResourceType.Wool, tiles.GetRequiredTileAt(-1, 1).Resource);
        Assert.Equal(2, tiles.GetRequiredTileAt(-1, 1).DiceNumber);
        Assert.Equal(ResourceType.Wood, tiles.GetRequiredTileAt(1, 1).Resource);
        Assert.Equal(6, tiles.GetRequiredTileAt(1, 1).DiceNumber);
    }

    [Fact]
    public void CreateEdgesAndVerticesForTestBoard_AllCreated()
    {
        // Arrange
        var gameState = new GameState(new Guid());

        // TODO: provide a way to provide all tiles at once
        foreach (var tile in BoardCreationHelpers.CreateTilesForTestBoard())
            gameState.AddTile(tile);

        // Act
        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gameState);

        // Assert
        Assert.Equal(30, gameState.Edges.Count);
        Assert.Equal(24, gameState.Vertices.Count);
        
        // TODO: Need to check more than just the count
    }

    [Fact]
    public void CreateEdgesAndVerticesForBoard_AllCreated()
    {
        // Arrange
        var gameState = new GameState(new Guid());

        // TODO: provide a way to provide all tiles at once
        foreach (var tile in BoardCreationHelpers.CreateTilesForStarterBoard())
            gameState.AddTile(tile);

        // Act
        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gameState);

        // Assert
        Assert.Equal(72, gameState.Edges.Count);
        Assert.Equal(54, gameState.Vertices.Count);   
        
        // TODO: Need to check more than just the count
    }
}