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
        Assert.Equal(GameStates.PlaceFirstRoad, dto.PhaseState);
        Assert.False(dto.WaitingForRoll);
        Assert.False(dto.DevCardPlayedThisRound);
        Assert.Null(dto.PreviousState);
        Assert.Null(dto.OriginalRobberTileId);
        Assert.Null(dto.RoadsPreRoadBuilding);
        Assert.Null(dto.PendingTradeResponses);
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
        Assert.Equal(GameStates.PlaceFirstRoad, dto.PhaseState);
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
        Assert.Equal(GameStates.PlaceFirstRoad, dto.PhaseState);
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
        gamePhase.SetWaitingForRoll();
        gamePhase.SetDevCardPlayedThisRound();
        gamePhase.AddPendingTradeResponse(new TradeResponse(p2, TradeResponseType.Original, 
            new Dictionary<ResourceType, int>() {{ResourceType.Ore, 1}}, new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}}));

        GamePhaseDTO gamePhaseDTO = new GamePhaseDTO(gamePhase);

        Assert.Equal(gamePhase.PhaseState, gamePhaseDTO.PhaseState);
        Assert.NotNull(gamePhase.CurrentPlayer);
        Assert.Equal(gamePhase.CurrentPlayer.Id, gamePhaseDTO.CurrentPlayerId);
        Assert.NotNull(gamePhase.EndPlayer);
        Assert.Equal(gamePhase.EndPlayer.Id, gamePhaseDTO.EndPlayerId);
        Assert.Equal(gamePhase.PreviousState, gamePhaseDTO.PreviousState);
        Assert.NotNull(gamePhase.OriginalRobberTile);
        Assert.Equal(gamePhase.OriginalRobberTile.Id, gamePhaseDTO.OriginalRobberTileId);
        Assert.Equal(7, gamePhaseDTO.RoadsPreRoadBuilding);
        Assert.True(gamePhase.WaitingForRoll);
        Assert.True(gamePhase.DevCardPlayedThisRound);
        Assert.NotNull(gamePhaseDTO.PendingTradeResponses);
        Assert.Single(gamePhaseDTO.PendingTradeResponses);  
        Assert.Equal(p2.Id, gamePhaseDTO.PendingTradeResponses[0].PlayerId);
        Assert.Equal(TradeResponseType.Original, gamePhaseDTO.PendingTradeResponses[0].ResponseType);
    }
}
