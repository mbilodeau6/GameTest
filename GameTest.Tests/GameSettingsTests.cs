using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class GameSettingsTests
{
    [Fact]
    public void Constructor_Defaults_ValidSettings()
    {
        // Act
        var settings = new GameSettings();

        // Assert
        Assert.Equal(GameType.Default, settings.Type);
        Assert.Equal(2, settings.MaxPlayers);
        Assert.Equal(10, settings.VictoryPointsToWin);
        Assert.Equal(15, settings.RoadsPerPlayer);
        Assert.Equal(5, settings.SettlementsPerPlayer);
        Assert.Equal(4, settings.CitiesPerPlayer);
    }

    [Fact]
    public void Constructor_SpecifiedValues_ValuesUsed()
    {
        // Arrange
        // Act
        var settings = new GameSettings(GameType.Starter, 3, 12, 10, 6, 2);

        // Assert
        Assert.Equal(GameType.Starter, settings.Type);
        Assert.Equal(3, settings.MaxPlayers);
        Assert.Equal(12, settings.VictoryPointsToWin);
        Assert.Equal(10, settings.RoadsPerPlayer);
        Assert.Equal(6, settings.SettlementsPerPlayer);
        Assert.Equal(2, settings.CitiesPerPlayer);
    }

    [Fact]
    public void Constructor_FromDto_ValuesUsed()
    {
        // Arrange
        var settings = new GameSettings(GameType.Starter, 3, 12, 10, 6, 2);
        var tile = new Tile(ResourceType.Brick, 10, 0, 0);
        var tiles = new List<Tile>() { tile };
        settings.SetPreRobberState(GameStates.RollOrUseDevCard, tile);
        var dto = new GameSettingsDTO(settings);

        // Act
        var newSettings = new GameSettings(tiles, dto);

        // Assert
        Assert.Equal(GameType.Starter, newSettings.Type);
        Assert.Equal(3, newSettings.MaxPlayers);
        Assert.Equal(12, newSettings.VictoryPointsToWin);
        Assert.Equal(10, newSettings.RoadsPerPlayer);
        Assert.Equal(6, newSettings.SettlementsPerPlayer);
        Assert.Equal(2, newSettings.CitiesPerPlayer);
        Assert.Equal(GameStates.RollOrUseDevCard, newSettings.PreRobberState);
        Assert.Equal(tile.Id, newSettings.OriginalRobberTile.Id);
    }

    
    [Fact]
    public void SetStatePreRobberMove_StateNullBefore()
    {
        // Arrage
        var settings = new GameSettings();
        var tile = new Tile(ResourceType.Brick, 10, 0, 0);

        // Act
        settings.SetPreRobberState(GameStates.BuildOrTrade, tile);

        // Assert
        Assert.NotNull(settings.PreRobberState);
        Assert.Equal(GameStates.BuildOrTrade, settings.PreRobberState);
    }

    [Fact]
    public void SetStatePreRobberMove_StateNotNullBefore_Exception()
    {
        // Arrage
        var settings = new GameSettings();
        var tile = new Tile(ResourceType.Brick, 10, 0, 0);

        settings.SetPreRobberState(GameStates.RollOrUseDevCard, tile);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => settings.SetPreRobberState(GameStates.BuildOrTrade, tile));
    }

    [Fact]
    public void ClearStatePreRobberMove()
    {
        // Arrage
        var settings = new GameSettings();
        var tile = new Tile(ResourceType.Brick, 10, 0, 0);

        settings.SetPreRobberState(GameStates.BuildOrTrade, tile);

        // Act
        settings.ClearRobberState();

        // Assert
        Assert.Null(settings.PreRobberState);
        Assert.Null(settings.OriginalRobberTile);
    }

}