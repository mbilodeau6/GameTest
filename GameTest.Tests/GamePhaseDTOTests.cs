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
        GamePhase gamePhase = new GamePhase(GameStates.PlaceFirstRoad, player1, player2);

        // Act
        GamePhaseDTO dto = new GamePhaseDTO(gamePhase);

        // Assert
        Assert.Equal(player1.Id, dto.CurrentPlayerId);
        Assert.Equal(player2.Id, dto.EndPlayerId);
        Assert.Equal(GameStates.PlaceFirstRoad.ToString(), dto.PhaseState);
    }

    [Fact]
    public void Constructor_WithoutEndPlayer()
    {
        // Arrange
        var player1 = new Player("Mary", PlayerColor.Red);
        GamePhase gamePhase = new GamePhase(GameStates.PlaceFirstRoad, player1);

        // Act
        GamePhaseDTO dto = new GamePhaseDTO(gamePhase);

        // Assert
        Assert.Equal(player1.Id, dto.CurrentPlayerId);
        Assert.Null(dto.EndPlayerId);
        Assert.Equal(GameStates.PlaceFirstRoad.ToString(), dto.PhaseState);
    }

    [Fact]
    public void Constructor_WithoutEitherPlayer()
    {
        // Arrange
        GamePhase gamePhase = new GamePhase(GameStates.PlaceFirstRoad);

        // Act
        GamePhaseDTO dto = new GamePhaseDTO(gamePhase);

        // Assert
        Assert.Null(dto.CurrentPlayerId);
        Assert.Null(dto.EndPlayerId);
        Assert.Equal(GameStates.PlaceFirstRoad.ToString(), dto.PhaseState);
    }

    [Fact]
    public void Constructor_FromDTO()
    {
        var p1 = new Player("Tim", PlayerColor.Red);
        var p2 = new Player("Mary", PlayerColor.Blue);
        GamePhase gamePhase = new GamePhase(GameStates.RollOrUseDevCard, p1, p2);
        var tile = new Tile(ResourceType.Brick, 10, 0, 0);
        gamePhase.SetStateToReturnTo(GameStates.RollOrUseDevCard, tile);
        gamePhase.StoreStateDevCardRoadBuilding(GameStates.RollOrUseDevCard, 7);

        GamePhaseDTO gamePhaseDTO = new GamePhaseDTO(gamePhase);

        Assert.Equal(gamePhase.PhaseState.ToString(), gamePhaseDTO.PhaseState);
        Assert.Equal(gamePhase.CurrentPlayer.Id, gamePhaseDTO.CurrentPlayerId);
        Assert.Equal(gamePhase.EndPlayer.Id, gamePhaseDTO.EndPlayerId);
        Assert.Equal(gamePhase.PreviousState.ToString(), gamePhaseDTO.PreviousState);
        Assert.Equal(gamePhase.OriginalRobberTile.Id, gamePhaseDTO.OriginalRobberTileId);
        Assert.Equal(gamePhase.OriginalRobberTile.Id.ToString(), gamePhaseDTO.OriginalRobberTileId);
        Assert.Equal(7, gamePhaseDTO.RoadsPreRoadBuilding);
    }

}
