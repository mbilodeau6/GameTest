using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class GamePhaseDTOTests
{
    [Fact]
    public void Constructor_WithFullGamePhase()
    {
        // Arrange
        var player1 = new Player("Mary", PlayerColor.Red);
        var player2 = new Player("Bill", PlayerColor.Blue);
        GamePhase gamePhase = new GamePhase(GameStates.SetUpRoadAsc, player1, player2);

        // Act
        GamePhaseDTO dto = new GamePhaseDTO(gamePhase);

        // Assert
        Assert.Equal(player1.Id, dto.CurrentPlayerId);
        Assert.Equal(player2.Id, dto.EndPlayerId);
        Assert.Equal(GameStates.SetUpRoadAsc.ToString(), dto.PhaseState);
    }

    [Fact]
    public void Constructor_WithoutEndPlayer()
    {
        // Arrange
        var player1 = new Player("Mary", PlayerColor.Red);
        GamePhase gamePhase = new GamePhase(GameStates.SetUpRoadAsc, player1);

        // Act
        GamePhaseDTO dto = new GamePhaseDTO(gamePhase);

        // Assert
        Assert.Equal(player1.Id, dto.CurrentPlayerId);
        Assert.Null(dto.EndPlayerId);
        Assert.Equal(GameStates.SetUpRoadAsc.ToString(), dto.PhaseState);
    }

    [Fact]
    public void Constructor_WithoutEitherPlayer()
    {
        // Arrange
        GamePhase gamePhase = new GamePhase(GameStates.SetUpRoadAsc);

        // Act
        GamePhaseDTO dto = new GamePhaseDTO(gamePhase);

        // Assert
        Assert.Null(dto.CurrentPlayerId);
        Assert.Null(dto.EndPlayerId);
        Assert.Equal(GameStates.SetUpRoadAsc.ToString(), dto.PhaseState);
    }

}
