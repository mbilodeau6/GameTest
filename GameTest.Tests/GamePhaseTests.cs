using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
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
        gamePhase.SetWaitingForRoll();
        gamePhase.SetDevCardPlayedThisRound();
        gamePhase.AddPendingTradeResponse(new TradeResponse(p2, TradeResponseType.Original, 
            new Dictionary<ResourceType, int>() {{ResourceType.Ore, 1}}, new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}}));

        GamePhaseDTO dto = new GamePhaseDTO(gamePhase);

        // Act
        GamePhase newGamePhase = new GamePhase(gs, dto);

        // Assert
        Assert.Equal(gamePhase.PhaseState, newGamePhase.PhaseState);
        Assert.NotNull(gamePhase.CurrentPlayer);
        Assert.NotNull(newGamePhase.CurrentPlayer);
        Assert.Equal(gamePhase.CurrentPlayer.Id, newGamePhase.CurrentPlayer.Id);
        Assert.NotNull(gamePhase.EndPlayer);
        Assert.NotNull(newGamePhase.EndPlayer);
        Assert.Equal(gamePhase.EndPlayer.Id, newGamePhase.EndPlayer.Id);
        Assert.NotNull(gamePhase.OriginalRobberTile);
        Assert.NotNull(newGamePhase.OriginalRobberTile);
        Assert.Equal(gamePhase.OriginalRobberTile.Id, newGamePhase.OriginalRobberTile.Id);
        Assert.Equal(gamePhase.PreviousState, newGamePhase.PreviousState);
        Assert.Equal(3, newGamePhase.RoadsPreRoadBuilding);
        Assert.True(newGamePhase.WaitingForRoll);
        Assert.True(newGamePhase.DevCardPlayedThisRound);
        Assert.NotNull(newGamePhase.PendingTradeResponses);
        Assert.Single(newGamePhase.PendingTradeResponses);
        Assert.Equal(gamePhase.PendingTradeResponses[0].Player.Id, newGamePhase.PendingTradeResponses[0].Player.Id);
        Assert.Equal(gamePhase.PendingTradeResponses[0].ResponseType, newGamePhase.PendingTradeResponses[0].ResponseType);
    }

    [Fact]
    public void Constructor_Copy()
    {
        // Arrange
        var p1 = new Player("Tim", PlayerColor.Red);
        var p2 = new Player("Mary", PlayerColor.Blue);
        GamePhase originalGamePhase = new GamePhase(GameStates.PlaceSecondSettlement, p1, p2);
        originalGamePhase.AddPendingTradeResponse(new TradeResponse(p1, TradeResponseType.Original, 
            new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}}, new Dictionary<ResourceType, int>() {{ResourceType.Ore, 1}}));

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
        Assert.Equal(originalGamePhase.WaitingForRoll, newGamePhase.WaitingForRoll);
        Assert.Equal(originalGamePhase.DevCardPlayedThisRound, newGamePhase.DevCardPlayedThisRound);
        Assert.NotNull(originalGamePhase.PendingTradeResponses);
        Assert.Single(originalGamePhase.PendingTradeResponses);
        Assert.NotNull(newGamePhase.PendingTradeResponses);
        Assert.Single(newGamePhase.PendingTradeResponses);
        Assert.Equal(originalGamePhase.PendingTradeResponses[0].Player.Id, newGamePhase.PendingTradeResponses[0].Player.Id);

        // Change original and make sure new not changed
        originalGamePhase.PhaseState = GameStates.PlaceSecondRoad;
        originalGamePhase.CurrentPlayer = p2;
        originalGamePhase.EndPlayer = p1;
        originalGamePhase.SetWaitingForRoll();

        Assert.Equal(p1.Id, newGamePhase.CurrentPlayer.Id);
        Assert.Equal(p2.Id, newGamePhase.EndPlayer.Id);
        Assert.Equal(GameStates.PlaceSecondSettlement, newGamePhase.PhaseState);
        Assert.False(newGamePhase.WaitingForRoll);
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

    [Fact]
    public void ClearDevCardPlayState()
    {
        var phaseState = new GamePhase(GameStates.BuildOrTrade, null, null);
        phaseState.SetDevCardPlayedThisRound();
        Assert.True(phaseState.DevCardPlayedThisRound);

        // Act
        phaseState.ClearDevCardPlayState();

        // Assert
        Assert.False(phaseState.DevCardPlayedThisRound);
    }

    [Fact]
    public void SetWaitingForRoll()
    {
        var phaseState = new GamePhase(GameStates.FirstDevCardRoad, null, null);
        Assert.False(phaseState.WaitingForRoll);

        phaseState.SetWaitingForRoll();
        Assert.True(phaseState.WaitingForRoll);
    }

    [Fact]
    public void ClearWaitingForRoll()
    {
        var phaseState = new GamePhase(GameStates.FirstDevCardRoad, null, null);
        phaseState.SetWaitingForRoll();
        Assert.True(phaseState.WaitingForRoll);

        phaseState.ClearWaitingForRoll();
        Assert.False(phaseState.WaitingForRoll);
    }

    // TODO: Should be private. Need to refactor additional routines in GamePlayHelpers
    public static bool IsNextPhaseAsExpected(GamePhase nextPhase, GameStates expectedState, Player expectedCurrent, Player? expectedEnd = null)
    {
        var result = nextPhase.PhaseState.Equals(expectedState)
            && nextPhase.CurrentPlayer != null
            && nextPhase.CurrentPlayer.Id.Equals(expectedCurrent.Id);

        if (expectedEnd != null)
            return result && nextPhase.EndPlayer != null 
                && nextPhase.EndPlayer.Id.Equals(expectedEnd.Id);
        else
            return result;
    }

    private static List<Player> CreateListOfPlayersForGetNextPlayerTests()
    {
        List<Player> players = new List<Player>();
        players.Add(new Player("Michael", PlayerColor.Red));
        players.Add(new Player("Jason", PlayerColor.Blue));
        players.Add(new Player("Jeff", PlayerColor.White));

        return players;
    }

    [Fact]
    public void GetNextPlayer_FirstOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var gamePhase = new GamePhase(GameStates.RollOrUseDevCard);
        var nextPlayer = gamePhase.GetNextPlayer(players[0], players);

        Assert.Equal(players[1], nextPlayer);
    }

    [Fact]
    public void GetNextPlayer_SecondOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var gamePhase = new GamePhase(GameStates.RollOrUseDevCard);
        var nextPlayer = gamePhase.GetNextPlayer(players[1], players);

        Assert.Equal(players[2], nextPlayer);
    }

    [Fact]
    public void GetNextPlayer_ThreeOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var gamePhase = new GamePhase(GameStates.RollOrUseDevCard);
        var nextPlayer =gamePhase.GetNextPlayer(players[2], players);

        Assert.Equal(players[0], nextPlayer);
    }

    [Fact]
    public void GetPreviousPlayer_FirstOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var gamePhase = new GamePhase(GameStates.RollOrUseDevCard);
        var nextPlayer = gamePhase.GetPreviousPlayer(players[0], players);

        Assert.Equal(players[2], nextPlayer);
    }

    [Fact]
    public void GetPreviousPlayer_SecondOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var gamePhase = new GamePhase(GameStates.RollOrUseDevCard);
        var nextPlayer = gamePhase.GetPreviousPlayer(players[1], players);

        Assert.Equal(players[0], nextPlayer);
    }

    [Fact]
    public void GetPreviousPlayer_ThreeOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var gamePhase = new GamePhase(GameStates.RollOrUseDevCard);
        var nextPlayer = gamePhase.GetPreviousPlayer(players[2], players);

        Assert.Equal(players[1], nextPlayer);
    }

    [Fact]
    public void GetNextPhase_SettingUpBoard_Invalid()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.SettingUpBoard);

        Assert.Throws<InvalidOperationException>( () => board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, 0, board.GetGameState().RobberTile));
    }

    [Fact]
    public void GetNextPhase_PlaceFirstSettlement_NoSettlementStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceFirstSettlement_MoveToPlaceFirstRoad()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 1, 0, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceFirstRoad_NoRoadStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer());

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 1, 0, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceFirstRoad_MoveToNextPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer());

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 1, 1, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceFirstSettlement, board.GetBluePlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceFirstRoad_MoveToPlaceSecondSettlement()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetBluePlayer(), board.GetBluePlayer());

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 1, 1, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceSecondSettlement, board.GetBluePlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceSecondSettlement_No2ndStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondSettlement, board.GetBluePlayer(), board.GetRedPlayer());

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 1, 1, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceSecondSettlement, board.GetBluePlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceSecondSettlement_MoveToPlaceSecondRoad()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondSettlement, board.GetBluePlayer(), board.GetRedPlayer());

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 2, 1, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceSecondRoad, board.GetBluePlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceSecondRoad_No2ndStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondRoad, board.GetBluePlayer(), board.GetRedPlayer());

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 2, 1, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceSecondRoad, board.GetBluePlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceSecondRoad_MoveToNextPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondRoad, board.GetBluePlayer(), board.GetRedPlayer());

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 2, 2, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceSecondSettlement, board.GetRedPlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceSecondRoad_MoveToRollOrUseDevCard()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondRoad, board.GetRedPlayer(), board.GetRedPlayer());

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 2, 2, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_RollOrUseDevCard_NoRollStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetWaitingForRoll();

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_RollOrUseDevCard_MoveToBuildOrTrade()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetWaitingForRoll();

        do
            GamePlayHelpers.RollDice(board.GetGameState(), true);
        while (board.GetGameState().Dice.GetCombinedValue() == 7);

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, board.GetGameState().Dice.GetCombinedValue(), board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_RollOrUseDevCard_MoveToPlaceRobber()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetWaitingForRoll();

        do
            GamePlayHelpers.RollDice(board.GetGameState(), true);
        while (board.GetGameState().Dice.GetCombinedValue() != 7);

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, board.GetGameState().Dice.GetCombinedValue(), board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer()));
        Assert.Equal(GameStates.BuildOrTrade, phase.PreviousState);
    }

    [Fact]
    public void GetNextPhase_RollOrUseDevCard_MoveToDiscardCardsCurrentPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetWaitingForRoll();
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 5);
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 3);

        do
            GamePlayHelpers.RollDice(board.GetGameState(), true);
        while (board.GetGameState().Dice.GetCombinedValue() != 7);

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, board.GetGameState().Dice.GetCombinedValue(), board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.DiscardCards, board.GetRedPlayer(), board.GetRedPlayer()));
        Assert.Equal(GameStates.BuildOrTrade, phase.PreviousState);
    }

    [Fact]
    public void GetNextPhase_RollOrUseDevCard_MoveToDiscardCardsNextPlayer() // Because current player doesn't have to discard
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetWaitingForRoll();
        board.GetBluePlayer().AssignResources(ResourceType.Ore, 3);
        board.GetBluePlayer().AssignResources(ResourceType.Wool, 3);
        board.GetBluePlayer().AssignResources(ResourceType.Grain, 3);

        do
            GamePlayHelpers.RollDice(board.GetGameState(), true);
        while (board.GetGameState().Dice.GetCombinedValue() != 7);

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, board.GetGameState().Dice.GetCombinedValue(), board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.DiscardCards, board.GetBluePlayer(), board.GetRedPlayer()));
        Assert.Equal(GameStates.BuildOrTrade, phase.PreviousState);
    }

    [Fact]
    public void GetNextPhase_DiscardCards_BothNeedToDiscard()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetWaitingForRoll();
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 5);
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 3);
        board.GetBluePlayer().AssignResources(ResourceType.Wool, 4);
        board.GetBluePlayer().AssignResources(ResourceType.Brick, 4);

        do
            GamePlayHelpers.RollDice(board.GetGameState(), true);
        while (board.GetGameState().Dice.GetCombinedValue() != 7);

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, board.GetGameState().Dice.GetCombinedValue(), board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.DiscardCards, board.GetRedPlayer(), board.GetRedPlayer()));
        Assert.Equal(GameStates.BuildOrTrade, phase.PreviousState);
    }

    [Fact]
    public void GetNextPhase_DiscardCardsCurrentPlayer_MoveToPlaceRobber() // Because other player doesn't have to discard
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetWaitingForRoll();

        do
            GamePlayHelpers.RollDice(board.GetGameState(), true);
        while (board.GetGameState().Dice.GetCombinedValue() != 7);

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, board.GetGameState().Dice.GetCombinedValue(), board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer()));
        Assert.Equal(GameStates.BuildOrTrade, phase.PreviousState);
    }

    [Fact]
    public void GetNextPhase_DiscardCardsOtherPlayer_MoveToPlaceRobber()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetWaitingForRoll();
        board.GetBluePlayer().AssignResources(ResourceType.Wool, 4);
        board.GetBluePlayer().AssignResources(ResourceType.Brick, 4);
        do
            GamePlayHelpers.RollDice(board.GetGameState(), true);
        while (board.GetGameState().Dice.GetCombinedValue() != 7);

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, board.GetGameState().Dice.GetCombinedValue(), board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.DiscardCards, board.GetBluePlayer(), board.GetRedPlayer()));
        Assert.Equal(GameStates.BuildOrTrade, phase.PreviousState);

        board.GetBluePlayer().RemoveResources(ResourceType.Wool, 4);

        phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, board.GetGameState().Dice.GetCombinedValue(), board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer()));
        Assert.Equal(GameStates.BuildOrTrade, phase.PreviousState);
    }

    [Fact]
    public void GetNextPhase_PlaceRobber_RobberNotMovedStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetStateToReturnTo(GameStates.RollOrUseDevCard, board.GetGameState().RobberTile);

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer()));
        Assert.Equal(GameStates.RollOrUseDevCard, board.GetGameState().Phase.PreviousState);
    }

    [Fact]
    public void GetNextPhase_PlaceRobber_MoveToRollOrUseDevCard()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetStateToReturnTo(GameStates.RollOrUseDevCard, board.GetGameState().RobberTile);
        board.GetGameState().SetRobberTile(board.GetTile(TestTile.T5));

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, 0, board.GetGameState().RobberTile);
        board.GetGameState().Phase.ClearRobberState();

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer()));
        Assert.Null(board.GetGameState().Phase.PreviousState);
        Assert.Null(board.GetGameState().Phase.OriginalRobberTile);
        Assert.Equal(board.GetTile(TestTile.T5).Id, board.GetGameState().RobberTile.Id);
    }

    [Fact]
    public void GetNextPhase_PlaceRobber_MoveToBuildOrTrade()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetStateToReturnTo(GameStates.BuildOrTrade, board.GetGameState().RobberTile);
        board.GetGameState().SetRobberTile(board.GetTile(TestTile.T5));

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 0, 0, board.GetGameState().RobberTile);
        board.GetGameState().Phase.ClearRobberState();

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer()));
        Assert.Null(board.GetGameState().Phase.PreviousState);
        Assert.Null(board.GetGameState().Phase.OriginalRobberTile);
        Assert.Equal(board.GetTile(TestTile.T5).Id, board.GetGameState().RobberTile.Id);
    }

    private TradeResponse CreateOriginalWSingleResource(Player player, 
        ResourceType offerType, int offerCount, ResourceType requestType, int requestCount)
    {
        return new TradeResponse(player, TradeResponseType.Original, 
            new Dictionary<ResourceType, int>() {{offerType, offerCount}}, 
            new Dictionary<ResourceType, int>() {{requestType, requestCount}});
    }

    [Fact]
    public void GetNextPhase_BuildOrTrade_MoveToRespondToTrade()
    {
        // Arrange
        var players = new List<Player>();
        var p1 = new Player("Tim", PlayerColor.Red);
        players.Add(p1);
        var gamePhase = new GamePhase(GameStates.BuildOrTrade, p1, p1);
        gamePhase.AddPendingTradeResponse(CreateOriginalWSingleResource(p1, ResourceType.Wood, 1, ResourceType.Brick, 1));

        // Act
        var nextPhase = gamePhase.GetNextPhase(players, 2, 2, 8, null!);

        // Assert
        Assert.Equal(GameStates.RespondToTrade, nextPhase.PhaseState);
        Assert.NotNull(nextPhase.CurrentPlayer);
        Assert.Equal(p1.Id, nextPhase.CurrentPlayer.Id);
    }

    [Fact]
    public void GetNextPhase_RespondToTrade_StayPut()
    {
        // Arrange
        var players = new List<Player>();
        var p1 = new Player("Tim", PlayerColor.Red);
        players.Add(p1);
        var p2 = new Player("Tony", PlayerColor.White);
        players.Add(p2);
        var gamePhase = new GamePhase(GameStates.RespondToTrade, p1, p2);
        gamePhase.AddPendingTradeResponse(CreateOriginalWSingleResource(p1, ResourceType.Wood, 1, ResourceType.Brick, 1));
        gamePhase.AddPendingTradeResponse(new TradeResponse(p2, TradeResponseType.Accept, null, null));

        // Act
        var nextPhase = gamePhase.GetNextPhase(players, 2, 2, 8, null!);

        // Assert
        Assert.Equal(GameStates.RespondToTrade, nextPhase.PhaseState);
        Assert.NotNull(nextPhase.CurrentPlayer);
        Assert.Equal(p1.Id, nextPhase.CurrentPlayer.Id);
    }

    [Fact]
    public void GetNextPhase_RespondToTrade_MoveToBuildOrTrade()
    {
        // Arrange
        var players = new List<Player>();
        var p1 = new Player("Tim", PlayerColor.Red);
        players.Add(p1);
        var p2 = new Player("Tony", PlayerColor.White);
        players.Add(p2);
        var gamePhase = new GamePhase(GameStates.RespondToTrade, p1, p2);
        gamePhase.AddPendingTradeResponse(CreateOriginalWSingleResource(p1, ResourceType.Wood, 1, ResourceType.Brick, 1));
        gamePhase.AddPendingTradeResponse(new TradeResponse(p2, TradeResponseType.Accept, null, null));
        gamePhase.ClearPendingTradeResponses();

        // Act
        var nextPhase = gamePhase.GetNextPhase(players, 2, 2, 8, null!);

        // Assert
        Assert.Equal(GameStates.BuildOrTrade, nextPhase.PhaseState);
        Assert.NotNull(nextPhase.CurrentPlayer);
        Assert.Equal(p1.Id, nextPhase.CurrentPlayer.Id);
    }

    private TestGameBoard CreateGameStateForRoadBuildingPhaseTesting(GameStates startingState)
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(startingState, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E17).BuildRoad(board.GetRedPlayer());
        board.GetRedPlayer().AssignDevelopmentCard(DevelopmentCardType.RoadBuilding);
        board.GetRedPlayer().MakeNewDevelopmentCardsPlayable();
        GamePlayHelpers.PlayRoadBuildingDevCard(board.GetGameState(), board.GetRedPlayer());

        return board;
    }

    // TODO: This is really testing that GetNextPhase() stays as the phase transition
    // is being done by the call to GamePlayHelpers.PlayRoadBuildingDevCard().
    // Need to review and make sure this makes sense. If yes, should change to 
    // a PlayRoadBuildingDevCard test or integration test.
    [Fact]
    public void GetNextPhase_BuildOrTrade_MoveToFirstDevCardRoad()
    {
        // Arrange
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.BuildOrTrade);

        // Act
        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 2, 0, board.GetGameState().RobberTile);

        // Assert
        Assert.Equal(GameStates.FirstDevCardRoad, phase.PhaseState);
        Assert.Equal(GameStates.BuildOrTrade, phase.PreviousState);
        Assert.Equal(2, phase.RoadsPreRoadBuilding);
    }

    // TODO: This is really testing that GetNextPhase() stays as the phase transition
    // is being done by the call to GamePlayHelpers.PlayRoadBuildingDevCard().
    // Need to review and make sure this makes sense. If yes, should change to 
    // a PlayRoadBuildingDevCard test or integration test.
    [Fact]
    public void GetNextPhase_RollOrUseDevCard_MoveToFirstDevCardRoad()
    {
        // Arrange
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.RollOrUseDevCard);

        // Act
        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 2, 0, board.GetGameState().RobberTile);

        // Assert
        Assert.Equal(GameStates.FirstDevCardRoad, phase.PhaseState);
        Assert.Equal(GameStates.RollOrUseDevCard, phase.PreviousState);
        Assert.Equal(2, phase.RoadsPreRoadBuilding);
    }

    [Fact]
    public void GetNextPhase_FirstDevCardRoad_MoveToSecondDevCardRoad()
    {
        // Arrange
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.RollOrUseDevCard);

        // Act
        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 3, 0, board.GetGameState().RobberTile);

        Assert.Equal(GameStates.SecondDevCardRoad, phase.PhaseState);
        Assert.Equal(GameStates.RollOrUseDevCard, phase.PreviousState);
        Assert.Equal(2, phase.RoadsPreRoadBuilding);
    }

    [Fact]
    public void GetNextPhase_SecondDevCardRoad_StayBecauseConditionsNotSatisfied()
    {
        // Arrange
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.RollOrUseDevCard);
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E16));
        GamePlayHelpers.GameLoop(board.GetGameState());
        Assert.Equal(GameStates.SecondDevCardRoad, board.GetGameState().Phase.PhaseState);

        // Act
        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 3, 0, board.GetGameState().RobberTile);

        Assert.Equal(GameStates.SecondDevCardRoad, phase.PhaseState);
        Assert.Equal(GameStates.RollOrUseDevCard, phase.PreviousState);
        Assert.Equal(2, phase.RoadsPreRoadBuilding);
    }

    [Fact]
    public void GetNextPhase_SecondDevCardRoad_MoveToBuildOrTrade()
    {
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.BuildOrTrade);
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E16));
        GamePlayHelpers.GameLoop(board.GetGameState());
        Assert.Equal(GameStates.SecondDevCardRoad, board.GetGameState().Phase.PhaseState);
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E18));

        // Act
        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 4, 0, board.GetGameState().RobberTile);

        Assert.Equal(GameStates.BuildOrTrade, phase.PhaseState);
        Assert.Null(phase.PreviousState);
        Assert.Null(phase.RoadsPreRoadBuilding);
    }

    [Fact]
    public void GetNextPhase_SecondDevCardRoad_MoveToRollOrUseDevCard()
    {
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.RollOrUseDevCard);
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E16));
        GamePlayHelpers.GameLoop(board.GetGameState());
        Assert.Equal(GameStates.SecondDevCardRoad, board.GetGameState().Phase.PhaseState);
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E18));

        // Act
        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 0, 4, 0, board.GetGameState().RobberTile);

        Assert.Equal(GameStates.RollOrUseDevCard, phase.PhaseState);
        Assert.Null(phase.PreviousState);
        Assert.Null(phase.RoadsPreRoadBuilding);
    }

    [Fact]
    public void GetNextPhase_BuildOrTrade_MoveToGameOverWin()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetGameState().Settings.VictoryPointsToWin, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V3).UpgradeToCity();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V10).UpgradeToCity();
        board.GetVertex(TestVertex.V1).BuildSettlement(board.GetRedPlayer());
        board.GetGameState().UpdatePlayerVictoryPoints(board.GetRedPlayer());

        var phase = board.GetGameState().Phase.GetNextPhase(board.GetGameState().Players, 1, 0, 0, board.GetGameState().RobberTile);

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.GameOver, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    // TODO: Seems like moving from any state to GameOver due to the resignation
    // of all other players would be from explicit calls from the users vs
    // something that would happen due to a call to GetNextPhase.

    [Fact]
    public void PlayerHasWon_DefaultThreshold_Not_Met()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetGameState().Settings.VictoryPointsToWin, board.GetRedPlayer(), board.GetBluePlayer());
        Assert.True(board.InExpectedState(GameStates.RollOrUseDevCard, board.GetRedPlayer()));
        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V3).UpgradeToCity();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V1).BuildSettlement(board.GetRedPlayer());
        board.GetGameState().UpdatePlayerVictoryPoints(board.GetRedPlayer());

        Assert.False(board.GetGameState().Phase.PlayerHasWon(board.GetRedPlayer()));
    }

    [Fact]
    public void PlayerHasWon_DefaultThreshold_Met()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetGameState().Settings.VictoryPointsToWin, board.GetRedPlayer(), board.GetBluePlayer());
        Assert.True(board.InExpectedState(GameStates.RollOrUseDevCard, board.GetRedPlayer()));
        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V3).UpgradeToCity();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V10).UpgradeToCity();
        board.GetVertex(TestVertex.V1).BuildSettlement(board.GetRedPlayer());
        board.GetGameState().UpdatePlayerVictoryPoints(board.GetRedPlayer());

        Assert.True(board.GetGameState().Phase.PlayerHasWon(board.GetRedPlayer()));
    }

    [Fact]
    public void AddPendingTradeResponse_NoOriginal_ThrowsException()
    {
        var gamePhase = new GamePhase(GameStates.BuildOrTrade, null, null);

        Assert.Throws<InvalidOperationException>(() => gamePhase.AddPendingTradeResponse(
            new TradeResponse(new Player("TestPlayer", PlayerColor.Green), TradeResponseType.Accept, null ,null)));
    }

    private void AddOriginalTradeRequest(GamePhase gamePhase)
    {
        var offer = new Dictionary<ResourceType, int>
        {
            { ResourceType.Brick, 2 },
            { ResourceType.Wood, 1 }
        };

        var request = new Dictionary<ResourceType, int>
        {
            { ResourceType.Ore, 2 }
        };
        
        var originalTrade = new TradeResponse(new Player("CurrentPlayer", PlayerColor.Orange), TradeResponseType.Original, offer, request);
        gamePhase.AddPendingTradeResponse(originalTrade);
    }

    [Fact]
    public void AddPendingTradeResponse_NullTradeResponse_ThrowsException()
    {
        var gamePhase = new GamePhase(GameStates.BuildOrTrade, null, null);
        AddOriginalTradeRequest(gamePhase);

        Assert.Throws<ArgumentNullException>(() => gamePhase.AddPendingTradeResponse(null!));
    }

    [Fact]
    public void AddPendingTradeResponse_FirstForPlayer()
    {
        // Arrange
        var player = new Player("TestPlayer", PlayerColor.Green); 
        var gamePhase = new GamePhase(GameStates.BuildOrTrade, null, null);
        var tradeResponse = new TradeResponse(player, TradeResponseType.Accept, null ,null);
        AddOriginalTradeRequest(gamePhase);

        // Act
        gamePhase.AddPendingTradeResponse(tradeResponse);

        // Assert
        Assert.NotNull(gamePhase.PendingTradeResponses);
        Assert.Equal(2, gamePhase.PendingTradeResponses.Count);
        Assert.Contains(tradeResponse, gamePhase.PendingTradeResponses);
    }

    [Fact]
    public void AddPendingTradeResponse_SecondForPlayer()
    {
        // Arrange
        var player1 = new Player("TestPlayer", PlayerColor.Green); 
        var player2 = new Player("AnotherPlayer", PlayerColor.Blue);
        var gamePhase = new GamePhase(GameStates.BuildOrTrade, null, null);
        var tradeResponse = new TradeResponse(player1, TradeResponseType.Accept, null ,null);
        AddOriginalTradeRequest(gamePhase);
        gamePhase.AddPendingTradeResponse(tradeResponse);
        tradeResponse = new TradeResponse(player2, TradeResponseType.Reject, null ,null);
        gamePhase.AddPendingTradeResponse(tradeResponse);

        // Act
        tradeResponse = new TradeResponse(player1, TradeResponseType.Counter, 
            new Dictionary<ResourceType, int>() {{ ResourceType.Wood, 1 }}, new Dictionary<ResourceType, int>() {{ ResourceType.Brick, 1 }});
        gamePhase.AddPendingTradeResponse(tradeResponse);

        // Assert
        Assert.NotNull(gamePhase.PendingTradeResponses);
        Assert.Equal(3, gamePhase.PendingTradeResponses.Count);
        Assert.Single(gamePhase.PendingTradeResponses, p => p.Player.Id.Equals(player1.Id));
    }

    [Fact]
    public void AddPendingTradeResponse_SendSecondOriginal_ThrowsException()
    {
        // Arrange
        var gamePhase = new GamePhase(GameStates.BuildOrTrade, null, null);
        AddOriginalTradeRequest(gamePhase);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            gamePhase.AddPendingTradeResponse(
                new TradeResponse(new Player("AnotherPlayer", PlayerColor.Blue), 
                TradeResponseType.Original, new Dictionary<ResourceType, int>() {{ResourceType.Wool, 1}}, 
                    new Dictionary<ResourceType, int>() {{ResourceType.Grain, 1}})));
    }

    [Fact]
    public void ClearPendingTradeResponses()
    {
        // Arrange
        var player = new Player("TestPlayer", PlayerColor.Green); 
        var gamePhase = new GamePhase(GameStates.BuildOrTrade, null, null);
        var tradeResponse = new TradeResponse(player, TradeResponseType.Accept, null ,null);
        AddOriginalTradeRequest(gamePhase);
        gamePhase.AddPendingTradeResponse(tradeResponse);

        // Act
        gamePhase.ClearPendingTradeResponses();

        // Assert
        Assert.Null(gamePhase.PendingTradeResponses);
    }
}