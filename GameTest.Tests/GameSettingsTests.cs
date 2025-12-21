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
        Assert.Equal(4, settings.MaxPlayers);
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
        var dto = new GameSettingsDTO(settings);

        // Act
        var newSettings = new GameSettings(dto);

        // Assert
        Assert.Equal(GameType.Starter, newSettings.Type);
        Assert.Equal(3, newSettings.MaxPlayers);
        Assert.Equal(12, newSettings.VictoryPointsToWin);
        Assert.Equal(10, newSettings.RoadsPerPlayer);
        Assert.Equal(6, newSettings.SettlementsPerPlayer);
        Assert.Equal(2, newSettings.CitiesPerPlayer);
    }
}