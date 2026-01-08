using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class GameSettingsDTOTests
{
    [Fact]
    public void Constructor_FromGameSettings_CreatesDTO()
    {
        // Arrange
        var settings = new GameSettings();

        // Act
        var settingsDTO = new GameSettingsDTO(settings);

        // Assert
        Assert.Equal(settings.Type, settingsDTO.Type);
        Assert.Equal(settings.CitiesPerPlayer, settingsDTO.CitiesPerPlayer);
        Assert.Equal(settings.MaxPlayers, settingsDTO.MaxPlayers);
        Assert.Equal(settings.RoadsPerPlayer, settingsDTO.RoadsPerPlayer);
        Assert.Equal(settings.SettlementsPerPlayer, settingsDTO.SettlementsPerPlayer);
        Assert.Equal(settings.VictoryPointsToWin, settingsDTO.VictoryPointsToWin);
        Assert.Equal(settings.Creator, settingsDTO.Creator);
    }

    [Fact]
    public void Constructor_CopyConstructor()
    {
        // Arrange
        var settings = new GameSettings(GameType.Starter, "UT", 2, 11, 9, 5, 1);
        var settingsDTO = new GameSettingsDTO(settings);

        // Act
        var copiedDTO = new GameSettingsDTO(settingsDTO);

        // Assert
        Assert.Equal(settings.Type, copiedDTO.Type);
        Assert.Equal(settings.CitiesPerPlayer, copiedDTO.CitiesPerPlayer);
        Assert.Equal(settings.MaxPlayers, copiedDTO.MaxPlayers);
        Assert.Equal(settings.RoadsPerPlayer, copiedDTO.RoadsPerPlayer);
        Assert.Equal(settings.SettlementsPerPlayer, copiedDTO.SettlementsPerPlayer);
        Assert.Equal(settings.VictoryPointsToWin, copiedDTO.VictoryPointsToWin);
        Assert.Equal(settings.Creator, copiedDTO.Creator);
    }
}