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
}