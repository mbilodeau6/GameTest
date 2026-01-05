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
        Assert.Equal(1072, response.ErrorCode);
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
        GamePlayHelpers.MarkBlockedVertices(gs);
        gs.Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetRedPlayer());
        var buildResponse = GamePlayHelpers.BuildSettlementRequestFromUser(gs, board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Id);
        Assert.True(buildResponse.Success);
        Assert.Equal(BuildingType.Blocked, board.GetVertex(TestVertex.V6).Building);
        Assert.Equal(BuildingType.Blocked, board.GetVertex(TestVertex.V19).Building);
        Assert.Equal(BuildingType.Blocked, board.GetVertex(TestVertex.V4).Building);
        Assert.Equal(GameStates.PlaceFirstRoad, gs.Phase.PhaseState);

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
    public void UndoFromUser_UndoFirstRoad_FirstPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
         board.GetVertex(TestVertex.V5).BuildSettlement(board.GetRedPlayer());
        gs.Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer());
        var buildResponse = GamePlayHelpers.BuildRoadRequestFromUser(gs, board.GetRedPlayer().Id, board.GetEdge(TestEdge.E11).Id);
        Assert.True(buildResponse.Success);
        Assert.Equal(GameStates.PlaceFirstSettlement, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);

        var request = new UndoRequest(board.GetRedPlayer().Id, 0);
        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.PlaceFirstRoad, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(0, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Null(board.GetEdge(TestEdge.E11).Owner);
        Assert.Contains(gs.EventRecord, e => e.Id == 1 && 
            e.Action == EventRecordAction.Undo 
            && e.EventReversed != null && e.EventReversed == 0);
    }

    [Fact]
    public void UndoFromUser_UndoSecondSettlement_LastPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.PlaceSecondSettlement, board.GetRedPlayer(), board.GetBluePlayer());
        var buildResponse = GamePlayHelpers.BuildSettlementRequestFromUser(gs, board.GetRedPlayer().Id, board.GetVertex(TestVertex.V10).Id);
        Assert.True(buildResponse.Success);
        Assert.Equal(GameStates.PlaceSecondRoad, gs.Phase.PhaseState);
        Assert.Equal(2, gs.CountSettlementsForPlayer(board.GetRedPlayer()));
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Ore]);

        var request = new UndoRequest(board.GetRedPlayer().Id, 0);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.PlaceSecondSettlement, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(1, gs.CountSettlementsForPlayer(board.GetRedPlayer()));
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Null(board.GetVertex(TestVertex.V10).Owner);
        Assert.Null(board.GetVertex(TestVertex.V10).Building);
        Assert.Contains(gs.EventRecord, e => e.Id == 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == 0);
    }

    [Fact]
    public void UndoFromUser_UndoSecondRoad_LastPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        var gs = board.GetGameState();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());
        gs.Phase = new GamePhase(GameStates.PlaceSecondRoad, board.GetRedPlayer(), board.GetBluePlayer());
        var buildResponse = GamePlayHelpers.BuildRoadRequestFromUser(gs, board.GetRedPlayer().Id, board.GetEdge(TestEdge.E16).Id);
        Assert.True(buildResponse.Success);
        Assert.Equal(GameStates.PlaceSecondSettlement, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(2, gs.CountRoadsForPlayer(board.GetRedPlayer()));

        var request = new UndoRequest(board.GetRedPlayer().Id, 0);

        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.PlaceSecondRoad, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(1, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Null(board.GetEdge(TestEdge.E16).Owner);
        Assert.Contains(gs.EventRecord, e => e.Id == 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == 0);
    }

    private TestGameBoard CreateBoardThatIsPastSetUpPhase(GameStates startingPhase)
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        var gs = board.GetGameState();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E15).BuildRoad(board.GetBluePlayer());
        board.GetVertex(TestVertex.V16).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E22).BuildRoad(board.GetRedPlayer());
        gs.Phase = new GamePhase(startingPhase, board.GetRedPlayer(), board.GetBluePlayer());
        GamePlayHelpers.MarkBlockedVertices(gs);

        board.GetRedPlayer().AssignResources(ResourceType.Wood, 1);
        board.GetRedPlayer().AssignResources(ResourceType.Wool, 1);
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 1);
        board.GetRedPlayer().AssignResources(ResourceType.Grain, 2);
        board.GetRedPlayer().AssignResources(ResourceType.Ore, 3);
        board.GetBluePlayer().AssignResources(ResourceType.Wood, 2);
        board.GetBluePlayer().AssignResources(ResourceType.Wool, 2);
        board.GetBluePlayer().AssignResources(ResourceType.Ore, 2);

        return board;
    }

    [Fact]
    public void UndoFromUser_Undo3rdRoadBuild()
    {
        var board = CreateBoardThatIsPastSetUpPhase(GameStates.BuildOrTrade);
        var gs = board.GetGameState();
        var buildResponse = GamePlayHelpers.BuildRoadRequestFromUser(gs, board.GetRedPlayer().Id, board.GetEdge(TestEdge.E25).Id);
        var eventIdToUndo = gs.EventRecord.Last().Id;
        Assert.True(buildResponse.Success);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);

        var request = new UndoRequest(board.GetRedPlayer().Id, eventIdToUndo);
        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(2, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(3, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(2, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Null(board.GetEdge(TestEdge.E25).Owner);
        Assert.Contains(gs.EventRecord, e => e.Id == eventIdToUndo + 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdToUndo);
    }

    [Fact]
    public void UndoFromUser_Undo1stRoadBuilding()
    {
        var board = CreateBoardThatIsPastSetUpPhase(GameStates.FirstDevCardRoad);
        var gs = board.GetGameState();
        gs.Phase.StoreStateDevCardRoadBuilding(GameStates.BuildOrTrade, 2);

        GamePlayHelpers.BuildRoadRequestFromUser(gs, board.GetRedPlayer().Id, board.GetEdge(TestEdge.E25).Id);
        var eventIdToUndo = gs.EventRecord.Last().Id;
        GamePlayHelpers.GameLoop(gs);
        Assert.Equal(GameStates.SecondDevCardRoad, gs.Phase.PhaseState);

        var request = new UndoRequest(board.GetRedPlayer().Id, eventIdToUndo);
        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.FirstDevCardRoad, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(2, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(3, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(2, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Null(board.GetEdge(TestEdge.E25).Owner);
        Assert.Contains(gs.EventRecord, e => e.Id == eventIdToUndo + 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdToUndo);
    }

    [Fact]
    public void UndoFromUser_Undo2ndRoadBuilding()
    {
        var board = CreateBoardThatIsPastSetUpPhase(GameStates.SecondDevCardRoad);
        board.GetEdge(TestEdge.E25).BuildRoad(board.GetRedPlayer());
        var gs = board.GetGameState();
        gs.Phase.StoreStateDevCardRoadBuilding(GameStates.BuildOrTrade, 2);
        var buildResponse = GamePlayHelpers.BuildRoadRequestFromUser(gs, board.GetRedPlayer().Id, board.GetEdge(TestEdge.E26).Id);
        Assert.True(buildResponse.Success);
        var eventIdToUndo = gs.EventRecord.Last().Id;
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);

        var request = new UndoRequest(board.GetRedPlayer().Id, eventIdToUndo);
        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.SecondDevCardRoad, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(3, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(3, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(2, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Null(board.GetEdge(TestEdge.E26).Owner);
        Assert.Contains(gs.EventRecord, e => e.Id == eventIdToUndo + 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdToUndo);
    }

    [Fact]
    public void UndoFromUser_Undo5thRoadBuild_TakeAwayLongestRoad()
    {
        var board = CreateBoardThatIsPastSetUpPhase(GameStates.BuildOrTrade);
        board.GetEdge(TestEdge.E23).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E24).BuildRoad(board.GetRedPlayer());
        var gs = board.GetGameState();
        var buildResponse = GamePlayHelpers.BuildRoadRequestFromUser(gs, board.GetRedPlayer().Id, board.GetEdge(TestEdge.E21).Id);
        Assert.True(buildResponse.Success);
        var eventIdLongRoadGained = gs.EventRecord.Last().Id;
        var eventIdRoadBuilding = eventIdLongRoadGained -1 ;
        Assert.Equal(5, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.NotNull(gs.PlayerWithLongestRoad);
        Assert.Equal(board.GetRedPlayer().Id, gs.PlayerWithLongestRoad.Id);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);

        var request = new UndoRequest(board.GetRedPlayer().Id, eventIdLongRoadGained);
        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.Null(gs.PlayerWithLongestRoad);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(4, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(3, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(2, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Null(board.GetEdge(TestEdge.E21).Owner);
        Assert.Contains(gs.EventRecord, e => e.Id == eventIdLongRoadGained + 2 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdLongRoadGained);
        Assert.Contains(gs.EventRecord, e => e.Id == eventIdLongRoadGained + 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdRoadBuilding);
    }

    [Fact]
    public void UndoFromUser_Undo5thRoadBuild_GiveLongestRoadToPrevious()
    {
        var board = CreateBoardThatIsPastSetUpPhase(GameStates.BuildOrTrade);
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E8).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E14).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E23).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E24).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E21).BuildRoad(board.GetRedPlayer());
        var gs = board.GetGameState();
        gs.AssignLongestRoadToPlayer(board.GetBluePlayer());
        var buildResponse = GamePlayHelpers.BuildRoadRequestFromUser(gs, board.GetRedPlayer().Id, board.GetEdge(TestEdge.E20).Id);
        Assert.True(buildResponse.Success);
        var eventIdLongRoadGained = gs.EventRecord.Last().Id;
        var eventIdRoadBuilding = eventIdLongRoadGained -1 ;
        Assert.Equal(5, gs.CountRoadsForPlayer(board.GetBluePlayer()));
        Assert.Equal(6, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.NotNull(gs.PlayerWithLongestRoad);
        Assert.Equal(board.GetRedPlayer().Id, gs.PlayerWithLongestRoad.Id);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);

        var request = new UndoRequest(board.GetRedPlayer().Id, eventIdLongRoadGained);
        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.NotNull(gs.PlayerWithLongestRoad);
        Assert.Equal(board.GetBluePlayer().Id, gs.PlayerWithLongestRoad.Id);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(5, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(3, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(2, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Null(board.GetEdge(TestEdge.E20).Owner);
        Assert.Contains(gs.EventRecord, e => e.Id == eventIdLongRoadGained + 2 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdLongRoadGained);
        Assert.Contains(gs.EventRecord, e => e.Id == eventIdLongRoadGained + 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdRoadBuilding);
    }

    [Fact]
    public void UndoFromUser_Undo3rdSettlementBuild()
    {
        var board = CreateBoardThatIsPastSetUpPhase(GameStates.BuildOrTrade);
        var gs = board.GetGameState();
        board.GetEdge(TestEdge.E25).BuildRoad(board.GetRedPlayer());
        var buildResponse = GamePlayHelpers.BuildSettlementRequestFromUser(gs, board.GetRedPlayer().Id, board.GetVertex(TestVertex.V20).Id);
        var eventIdToUndo = gs.EventRecord.Last().Id;
        Assert.True(buildResponse.Success);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Equal(3,gs.CountSettlementsForPlayer(board.GetRedPlayer()));
        Assert.Equal(3, board.GetRedPlayer().FullVictoryPoints);

        var request = new UndoRequest(board.GetRedPlayer().Id, eventIdToUndo);
        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(2, gs.CountSettlementsForPlayer(board.GetRedPlayer()));
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(3, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(2, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Null(board.GetVertex(TestVertex.V20).Owner);
        Assert.Null(board.GetVertex(TestVertex.V20).Building);
        Assert.Equal(2, board.GetRedPlayer().FullVictoryPoints);
        Assert.Contains(gs.EventRecord, e => e.Id == eventIdToUndo + 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdToUndo);
    }

    [Fact]
    public void UndoFromUser_UndoCityBuild()
    {
        var board = CreateBoardThatIsPastSetUpPhase(GameStates.BuildOrTrade);
        var gs = board.GetGameState();
        var buildResponse = GamePlayHelpers.UpgradeToCityRequestFromUser(gs, board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Id);
        var eventIdToUndo = gs.EventRecord.Last().Id;
        Assert.True(buildResponse.Success);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Equal(1, gs.CountSettlementsForPlayer(board.GetRedPlayer()));
        Assert.Equal(1, gs.CountCitiesForPlayer(board.GetRedPlayer()));
        Assert.Equal(3, board.GetRedPlayer().FullVictoryPoints);

        var request = new UndoRequest(board.GetRedPlayer().Id, eventIdToUndo);
        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(2, gs.CountSettlementsForPlayer(board.GetRedPlayer()));
        Assert.Equal(0, gs.CountCitiesForPlayer(board.GetRedPlayer()));
        Assert.Equal(3, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(2, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Null(board.GetVertex(TestVertex.V20).Owner);
        Assert.Null(board.GetVertex(TestVertex.V20).Building);
        Assert.Equal(2, board.GetRedPlayer().FullVictoryPoints);
        Assert.Contains(gs.EventRecord, e => e.Id == eventIdToUndo + 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdToUndo);
    }

    // TODO: Continue working on tests


    // [Fact]
    // public void UndoFromUser_PlaceSecondRoad()
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