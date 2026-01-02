using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTest.Tests;

public class UndoHelpersTests
{
    [Fact]
    public void UndoFromUser_InvalidStateSettingUpBoard()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.SettingUpBoard, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new UndoRequest(board.GetRedPlayer().Id, 0);

        var response = UndoHelpers.UndoFromUser(gs, request);
        
        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_InvalidStatePlaceRobber()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.RollDice, new GameDice(1, 5)));
        Assert.NotNull(gs.EventRecord);
        Assert.Single(gs.EventRecord);

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response = UndoHelpers.UndoFromUser(gs, request);
        
        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_InvalidEventRecordId()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceSettlement, board.GetVertex(TestVertex.V5)));
        Assert.NotNull(gs.EventRecord);
        Assert.Equal(2, gs.EventRecord.Count);

        var request = new UndoRequest(board.GetRedPlayer().Id, 2);

        var response = UndoHelpers.UndoFromUser(gs, request);
        
        Assert.False(response.Success);
        Assert.Equal(1070, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_LastActionNotRequestingPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceSettlement, board.GetVertex(TestVertex.V3)));

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response = UndoHelpers.UndoFromUser(gs, request);
        
        Assert.False(response.Success);
        Assert.Equal(1071, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_LastActionPlaceRobber()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRobber, 
            board.GetTile(TestTile.T0)));

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1072, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_LastActionRoll()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.RollDice, new GameDice(2, 6)));

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1072, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_LastActionBuyDevCard()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.BuyDevelopmentCard, DevelopmentCardType.VictoryPoint));

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1072, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_LastActionPlayKnight()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlayKnight, 
            board.GetTile(TestTile.T0)));

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1072, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_LastActionMonopoly()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlayMonopoly, 
            new Dictionary<ResourceType, int>() {{ResourceType.Grain, 2}}));

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1072, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_LastActionOfferToTrade()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.OfferToTrade, 
            new Dictionary<ResourceType, int>() {{ResourceType.Grain, 2}}, 
            new Dictionary<ResourceType, int>() {{ResourceType.Ore, 1}},
            null
            ));

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1072, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_LastActionTradeWithPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.TradeWithPlayer, 
            new Dictionary<ResourceType, int>() {{ResourceType.Grain, 2}}, 
            new Dictionary<ResourceType, int>() {{ResourceType.Ore, 1}},
            board.GetBluePlayer()
            ));

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1072, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_LastActionAcceptTrade()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.AcceptTrade));

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1072, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_LastActionRejectTrade()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.RejectTrade));

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1072, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_AttempToUndoEarlyWithoutUndoingLater()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceSettlement, board.GetVertex(TestVertex.V5)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));

        var request = new UndoRequest(board.GetRedPlayer().Id, 0);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1074, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_CantUndoUndo()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceSettlement, board.GetVertex(TestVertex.V5)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.Undo, 0));

        var request = new UndoRequest(board.GetRedPlayer().Id, 1);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1072, response.ErrorCode);
        Assert.Null(response.GameState);
    }


    [Fact]
    public void UndoFromUser_UndoFirstRoad_LastPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceFirstSettlement, board.GetVertex(TestVertex.V3)));
        gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E3)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceFirstSettlement, board.GetVertex(TestVertex.V5)));
        board.GetVertex(TestVertex.V5).BuildSettlement(board.GetRedPlayer());
        GamePlayHelpers.MarkBlockedVertices(gs);
        Assert.Equal(BuildingType.Blocked, board.GetVertex(TestVertex.V6).Building);
        Assert.Equal(BuildingType.Blocked, board.GetVertex(TestVertex.V19).Building);
        Assert.Equal(BuildingType.Blocked, board.GetVertex(TestVertex.V4).Building);

        gs.Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetRedPlayer());

        var request = new UndoRequest(board.GetRedPlayer().Id, 2);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.PlaceFirstSettlement, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.Equal(0, gs.CountSettlementsForPlayer(board.GetRedPlayer()));
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Null(board.GetEdge(TestEdge.E11).Owner);
        Assert.Contains(gs.EventRecord, e => e.Id == 3 && 
            e.Action == EventRecordAction.Undo 
            && e.EventReversed != null && e.EventReversed == 2);
        Assert.Null(board.GetVertex(TestVertex.V6).Building);
        Assert.Null(board.GetVertex(TestVertex.V19).Building);
        Assert.Null(board.GetVertex(TestVertex.V4).Building);
    }

    [Fact]
    public void UndoFromUser_UndoSecondSettlement_FirstPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceFirstSettlement, board.GetVertex(TestVertex.V3)));
        gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E3)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceFirstSettlement, board.GetVertex(TestVertex.V5)));
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        board.GetEdge(TestEdge.E11).BuildRoad(board.GetRedPlayer());
        gs.Phase = new GamePhase(GameStates.PlaceSecondSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        var request = new UndoRequest(board.GetRedPlayer().Id, 3);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.PlaceFirstRoad, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.Equal(0, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Null(board.GetEdge(TestEdge.E11).Owner);
        Assert.Contains(gs.EventRecord, e => e.Id == 4 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == 3);
    }

    [Fact]
    public void UndoFromUser_UndoSecondRoad_FirstPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceSecondSettlement, board.GetVertex(TestVertex.V10), 
            new Dictionary<ResourceType, int>() { {ResourceType.Wool, 1}, {ResourceType.Ore, 1}}));
        board.GetEdge(TestEdge.E15).BuildRoad(board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E15)));
        board.GetVertex(TestVertex.V16).BuildSettlement(board.GetRedPlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceSecondSettlement, board.GetVertex(TestVertex.V16),
            new Dictionary<ResourceType, int>() { {ResourceType.Wood, 1}, {ResourceType.Wool, 1}}));
        board.GetEdge(TestEdge.E22).BuildRoad(board.GetRedPlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E22)));
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());

        var request = new UndoRequest(board.GetRedPlayer().Id, 3);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.PlaceSecondRoad, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(1, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Null(board.GetEdge(TestEdge.E22).Owner);
        Assert.Contains(gs.EventRecord, e => e.Id == 4 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == 3);
    }

    // TODO: Continue working on tests


    // [Fact]
    // public void UndoFromUser_PlaceSecondRoad()
    // {
    //     Assert.True(false);
    // }

    // [Fact]
    // public void UndoFromUser_BuildRoad()
    // {
    //     Assert.True(false);
    // }

    // [Fact]
    // public void UndoFromUser_BuildSettlement()
    // {
    //     Assert.True(false);
    // }

    // [Fact]
    // public void UndoFromUser_BuildCity()
    // {
    //     Assert.True(false);
    // }

    // [Fact]
    // public void UndoFromUser_BankTrade4to1()
    // {
    //     Assert.True(false);
    // }

    // [Fact]
    // public void UndoFromUser_BankTrade2to1()
    // {
    //     Assert.True(false);
    // }

    // [Fact]
    // public void UndoFromUser_YearOfPlenty()
    // {
    //     Assert.True(false);
    // }

    // [Fact]
    // public void UndoFromUser_RoadBuilding()
    // {
    //     Assert.True(false);
    // }

    // [Fact]
    // public void UndoFromUser_EndTurn()
    // {
    //     Assert.True(false);
    // }

    // [Fact]
    // public void UndoFromUser_Discard()
    // {
    //     Assert.True(false);
    // }

    // [Fact]
    // public void UndoFromUser_LastActionAlreadyUndone()
    // {
    //     Assert.True(false);
    // }   
}