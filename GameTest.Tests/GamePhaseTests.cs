using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using System.Security.Cryptography;

namespace GameTest.Tests;

public class GamePhaseTests
{
    [Fact]
    public void Constructor_DTO()
    {
        // Arrange
        GameState gs = new GameState(new Guid());
        var p1 = new Player("Tim", PlayerColor.Red);
        gs.Players.Add(p1);
        var p2 = new Player("Mary", PlayerColor.Blue);
        gs.Players.Add(p2);
        var t1 = new Tile(ResourceType.Grain, 8, 0, 0);
        gs.Tiles.Add(t1);

        GamePhase gamePhase = new GamePhase(GameStates.PlaceSecondSettlement, p1, p2);
        gamePhase.SetStateToReturnTo(GameStates.BuildOrTrade, t1);
        gamePhase.StoreStateDevCardRoadBuilding(GameStates.BuildOrTrade, 3);

        GamePhaseDTO dto = new GamePhaseDTO(gamePhase);

        // Act
        GamePhase newGamePhase = new GamePhase(gs, dto);

        // Assert
        Assert.Equal(gamePhase.PhaseState, newGamePhase.PhaseState);
        Assert.Equal(gamePhase.CurrentPlayer.Id, newGamePhase.CurrentPlayer.Id);
        Assert.Equal(gamePhase.EndPlayer.Id, newGamePhase.EndPlayer.Id);
        Assert.Equal(gamePhase.OriginalRobberTile.Id, newGamePhase.OriginalRobberTile.Id);
        Assert.Equal(gamePhase.PreviousState, newGamePhase.PreviousState);
        Assert.Equal(3, newGamePhase.RoadsPreRoadBuilding);
    }

    [Fact]
    public void Constructor_Copy()
    {
        // Arrange
        var p1 = new Player("Tim", PlayerColor.Red);
        var p2 = new Player("Mary", PlayerColor.Blue);
        GamePhase originalGamePhase = new GamePhase(GameStates.PlaceSecondSettlement, p1, p2);

        // Act
        var newGamePhase = new GamePhase(originalGamePhase);

        // Assert
        Assert.NotNull(newGamePhase.CurrentPlayer);
        Assert.Equal(p1.Id, newGamePhase.CurrentPlayer.Id);
        Assert.NotNull(newGamePhase.EndPlayer);
        Assert.Equal(p2.Id, newGamePhase.EndPlayer.Id);
        Assert.Equal(GameStates.PlaceSecondSettlement, newGamePhase.PhaseState);
        Assert.Equal(originalGamePhase.OriginalRobberTile, newGamePhase.OriginalRobberTile);
        Assert.Equal(originalGamePhase.PreviousState, newGamePhase.PreviousState);
        Assert.Equal(originalGamePhase.RoadsPreRoadBuilding, newGamePhase.RoadsPreRoadBuilding);

        // Change original and make sure new not changed
        originalGamePhase.PhaseState = GameStates.PlaceSecondRoad;
        originalGamePhase.CurrentPlayer = p2;
        originalGamePhase.EndPlayer = p1;

        Assert.Equal(p1.Id, newGamePhase.CurrentPlayer.Id);
        Assert.Equal(p2.Id, newGamePhase.EndPlayer.Id);
        Assert.Equal(GameStates.PlaceSecondSettlement, newGamePhase.PhaseState);
    }
    
    [Fact]
    public void SetStateToReturnTo_StateNullBefore()
    {
        // Arrage
        var gamePhase = new GamePhase(GameStates.RollOrUseDevCard, null, null);
        var tile = new Tile(ResourceType.Brick, 10, 0, 0);

        // Act
        gamePhase.SetStateToReturnTo(GameStates.BuildOrTrade, tile);

        // Assert
        Assert.NotNull(gamePhase.PreviousState);
        Assert.Equal(GameStates.BuildOrTrade, gamePhase.PreviousState);
    }

    [Fact]
    public void SetStateToReturnTo_StateNotNullBefore_Exception()
    {
        // Arrage
        var phaseState = new GamePhase(GameStates.BuildOrTrade, null, null);
        var tile = new Tile(ResourceType.Brick, 10, 0, 0);

        phaseState.SetStateToReturnTo(GameStates.RollOrUseDevCard, tile);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => phaseState.SetStateToReturnTo(GameStates.BuildOrTrade, tile));
    }

    [Fact]
    public void ClearStatePreRobberMove()
    {
        // Arrage
        var phaseState = new GamePhase(GameStates.BuildOrTrade, null, null);
        var tile = new Tile(ResourceType.Brick, 10, 0, 0);

        phaseState.SetStateToReturnTo(GameStates.BuildOrTrade, tile);

        // Act
        phaseState.ClearRobberState();

        // Assert
        Assert.Null(phaseState.PreviousState);
        Assert.Null(phaseState.OriginalRobberTile);
    }

    [Fact]
    public void ClearRoadBuildingState()
    {
        var phaseState = new GamePhase(GameStates.FirstDevCardRoad, null, null);
        phaseState.StoreStateDevCardRoadBuilding(GameStates.RollOrUseDevCard, 6);

        // Act
        phaseState.ClearRoadBuildingState();

        // Assert
        Assert.Null(phaseState.RoadsPreRoadBuilding);
    }

}