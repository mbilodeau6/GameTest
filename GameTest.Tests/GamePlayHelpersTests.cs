using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;

namespace GameTest.Tests;

public class GamePlayHelpersTests
{
    [Fact]
    public void GetResourcesEarnedOnLastRoll_NoBuildingsOnMatchingTiles()
    {
        // Arrage
        var gameState = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        gameState.SetDiceForTesting(new GameDice(new GameDie(3), new GameDie(5)));

        // Act
        var resources = GamePlayHelpers.GetResourcesEarnedOnLastRoll(gameState);

        // Assert
        Assert.Empty(resources);
    }

    [Fact]
    public void GetResourcesEarnedOnLastRoll_BuildingsOnMatchingTiles()
    {
        // Arrage
        var gameState = BoardCreationHelpers.CreateNewBoard(GameType.Test);

        var targetTile = BoardCreationHelpers.GetRequiredTileAt(gameState.Tiles, 0, 0);
        Assert.NotNull(targetTile);
        Assert.Equal(10, targetTile.DiceNumber);

        var bluePlayer = gameState.Players.First(p => p.Color == PlayerColor.Blue);
        Assert.NotNull(bluePlayer);
        var redPlayer = gameState.Players.First(p => p.Color == PlayerColor.Red);
        Assert.NotNull(redPlayer);

        var blueVertex = gameState.Vertices.First(v => v.Tiles.Contains(targetTile) && v.Owner == bluePlayer);
        Assert.NotNull(blueVertex);
        Assert.Equal(BuildingType.Settlement, blueVertex.Building);
        blueVertex.UpgradeToCity();

        gameState.SetDiceForTesting(new GameDice(new GameDie(4), new GameDie(6)));

        // Act
        var resources = GamePlayHelpers.GetResourcesEarnedOnLastRoll(gameState);

        // Assert
        Assert.NotEmpty(resources);
        Assert.True(resources.ContainsKey(bluePlayer));
        Assert.True(resources.ContainsKey(redPlayer));
        Assert.True(resources[redPlayer].ContainsKey(ResourceType.Grain));
        Assert.Equal(1, resources[redPlayer][ResourceType.Grain]);
        Assert.True(resources[bluePlayer].ContainsKey(ResourceType.Grain));
        Assert.Equal(2, resources[bluePlayer][ResourceType.Grain]);
    }

    [Fact]
    public void GetVictoryPointsForBuild_ForSettlement()
    {
        // Arrange
        // Act
        // Assert
        Assert.Equal(1, GamePlayHelpers.GetVictoryPointsForBuild(BuildingType.Settlement));
    }    

    [Fact]
    public void GetVictoryPointsForBuild_ForCity()
    {
        // Arrange
        // Act
        // Assert
        Assert.Equal(2, GamePlayHelpers.GetVictoryPointsForBuild(BuildingType.City));
    }    

}
