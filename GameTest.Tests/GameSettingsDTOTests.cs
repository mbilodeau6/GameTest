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
        Assert.Equal(settings.Type.ToString(), settingsDTO.Type);
        Assert.Equal(settings.CitiesPerPlayer, settingsDTO.CitiesPerPlayer);
        Assert.Equal(settings.MaxPlayers, settingsDTO.MaxPlayers);
        Assert.Equal(settings.RoadsPerPlayer, settingsDTO.RoadsPerPlayer);
        Assert.Equal(settings.SettlementsPerPlayer, settingsDTO.SettlementsPerPlayer);
        Assert.Equal(settings.VictoryPointsToWin, settingsDTO.VictoryPointsToWin);
    }

    [Fact]
    public void Constructor_CopyConstructor()
    {
        // Arrange
        var settings = new GameSettings();
        var settingsDTO = new GameSettingsDTO(settings);

        // Act
        var copiedDTO = new GameSettingsDTO(settingsDTO);

        // Assert
        Assert.Equal(settings.Type.ToString(), copiedDTO.Type);
        Assert.Equal(settings.CitiesPerPlayer, copiedDTO.CitiesPerPlayer);
        Assert.Equal(settings.MaxPlayers, copiedDTO.MaxPlayers);
        Assert.Equal(settings.RoadsPerPlayer, copiedDTO.RoadsPerPlayer);
        Assert.Equal(settings.SettlementsPerPlayer, copiedDTO.SettlementsPerPlayer);
        Assert.Equal(settings.VictoryPointsToWin, copiedDTO.VictoryPointsToWin);

    }
}