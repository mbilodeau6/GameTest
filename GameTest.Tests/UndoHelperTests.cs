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
        var request = new BaseRequest(board.GetRedPlayer().Id);

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

        var request = new BaseRequest(board.GetRedPlayer().Id);

        var response = UndoHelpers.UndoFromUser(gs, request);
        
        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void UndoFromUser_LastActionNotRequestingPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E11)));
        var lastEventId = gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceSettlement, board.GetVertex(TestVertex.V3)));
        gs.UndoState.Push(new PreActionState(lastEventId, gs.Phase, null, null));

        var request = new BaseRequest(board.GetRedPlayer().Id);

        var response = UndoHelpers.UndoFromUser(gs, request);
        
        Assert.False(response.Success);
        Assert.Equal(1071, response.ErrorCode);
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

        var request = new BaseRequest(board.GetRedPlayer().Id);

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

        var request = new BaseRequest(board.GetRedPlayer().Id);
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

        var request = new BaseRequest(board.GetRedPlayer().Id);

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

        var request = new BaseRequest(board.GetRedPlayer().Id);

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
        var eventIdToUndo = gs.UndoState.Peek().EventRecordId;
        Assert.True(buildResponse.Success);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);

        var request = new BaseRequest(board.GetRedPlayer().Id);
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
        var eventIdToUndo = gs.UndoState.Peek().EventRecordId;
        GamePlayHelpers.GameLoop(gs);
        Assert.Equal(GameStates.SecondDevCardRoad, gs.Phase.PhaseState);

        var request = new BaseRequest(board.GetRedPlayer().Id);
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
        var eventIdToUndo = gs.UndoState.Peek().EventRecordId;
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);

        var request = new BaseRequest(board.GetRedPlayer().Id);
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
        var eventIdRoadBuilding = gs.UndoState.Peek().EventRecordId;
        var eventIdLongRoadGained = eventIdRoadBuilding + 1 ;
        Assert.Equal(5, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.NotNull(gs.PlayerWithLongestRoad);
        Assert.Equal(board.GetRedPlayer().Id, gs.PlayerWithLongestRoad.Id);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);

        var request = new BaseRequest(board.GetRedPlayer().Id);
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
        Assert.Contains(gs.EventRecord, e => e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdLongRoadGained);
        Assert.Contains(gs.EventRecord, e => e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdRoadBuilding);
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
        var eventIdRoadBuilding = gs.UndoState.Peek().EventRecordId;
        var eventIdLongRoadGained = eventIdRoadBuilding + 1 ;
        Assert.Equal(5, gs.CountRoadsForPlayer(board.GetBluePlayer()));
        Assert.Equal(6, gs.CountRoadsForPlayer(board.GetRedPlayer()));
        Assert.NotNull(gs.PlayerWithLongestRoad);
        Assert.Equal(board.GetRedPlayer().Id, gs.PlayerWithLongestRoad.Id);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);

        var request = new BaseRequest(board.GetRedPlayer().Id);
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
        Assert.Contains(gs.EventRecord, e => e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdRoadBuilding);
        Assert.Contains(gs.EventRecord, e => e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdLongRoadGained);
    }

    [Fact]
    public void UndoFromUser_Undo3rdSettlementBuild()
    {
        var board = CreateBoardThatIsPastSetUpPhase(GameStates.BuildOrTrade);
        var gs = board.GetGameState();
        board.GetEdge(TestEdge.E25).BuildRoad(board.GetRedPlayer());
        var buildResponse = GamePlayHelpers.BuildSettlementRequestFromUser(gs, board.GetRedPlayer().Id, board.GetVertex(TestVertex.V20).Id);
        var eventIdToUndo = gs.UndoState.Peek().EventRecordId;
        Assert.True(buildResponse.Success);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Equal(3,gs.CountSettlementsForPlayer(board.GetRedPlayer()));
        Assert.Equal(3, board.GetRedPlayer().FullVictoryPoints);

        var request = new BaseRequest(board.GetRedPlayer().Id);
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
        var eventIdToUndo = gs.UndoState.Peek().EventRecordId;
        Assert.True(buildResponse.Success);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Grain]);
        Assert.Equal(1, gs.CountSettlementsForPlayer(board.GetRedPlayer()));
        Assert.Equal(1, gs.CountCitiesForPlayer(board.GetRedPlayer()));
        Assert.Equal(3, board.GetRedPlayer().FullVictoryPoints);

        var request = new BaseRequest(board.GetRedPlayer().Id);
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

    [Fact]
    public void UndoFromUser_BankTrade4to1()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 5);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetRedPlayer());
        var tradeRequest = new TradeRequestDTO(board.GetRedPlayer().Id, 
            new Dictionary<ResourceType, int>() { {ResourceType.Wood, 4}}, new Dictionary<ResourceType, int>() { {ResourceType.Brick, 1}});
        var buildResponse = GamePlayHelpers.BankTradeFromUser(gs, tradeRequest);
        var eventIdToUndo = gs.UndoState.Peek().EventRecordId;
        Assert.True(buildResponse.Success);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Brick]);
    
        var request = new BaseRequest(board.GetRedPlayer().Id);
        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.Equal(5, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Contains(gs.EventRecord, e => e.Id == eventIdToUndo + 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdToUndo);
    }

    [Fact]
    public void UndoFromUser_YearOfPlenty()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        board.GetRedPlayer().AssignDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        board.GetRedPlayer().MakeNewDevelopmentCardsPlayable();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetRedPlayer());
        var playRequest = new PlayDevCardRequest(board.GetRedPlayer().Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>() { ResourceType.Wood, ResourceType.Brick }, null);
        var buildResponse = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, playRequest);
        var eventIdToUndo = gs.UndoState.Peek().EventRecordId;
        Assert.True(buildResponse.Success);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Brick]);
    
        var request = new BaseRequest(board.GetRedPlayer().Id);
        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(1, board.GetRedPlayer().DevCardsReadyToPlay.Count(dc => dc == DevelopmentCardType.YearOfPlenty));
        Assert.Contains(gs.EventRecord, e => e.Id == eventIdToUndo + 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == eventIdToUndo);
    }

    [Fact]
    public void UndoFromUser_MultiUndo()
    {
        var board = CreateBoardThatIsPastSetUpPhase(GameStates.BuildOrTrade);
        var gs = board.GetGameState();
        board.GetRedPlayer().AssignDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        board.GetRedPlayer().MakeNewDevelopmentCardsPlayable();
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 0);
        board.GetRedPlayer().RemoveResources(ResourceType.Ore, 2);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetRedPlayer());
        var playRequest = new PlayDevCardRequest(board.GetRedPlayer().Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>() { ResourceType.Ore, ResourceType.Ore }, null);
        var buildResponse = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, playRequest);
        var yopEventIdToUndo = gs.UndoState.Peek().EventRecordId;
        Assert.True(buildResponse.Success);
        Assert.Equal(3, board.GetRedPlayer().Resources[ResourceType.Ore]);
    
        var buildReponse = GamePlayHelpers.UpgradeToCityRequestFromUser(gs, board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Id);
        var cityEventIdToUndo = gs.UndoState.Peek().EventRecordId;
        Assert.True(buildResponse.Success);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.Equal(1, gs.CountCitiesForPlayer(board.GetRedPlayer()));
        Assert.Empty(board.GetRedPlayer().DevCardsReadyToPlay);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Ore]);
    
        var request = new BaseRequest(board.GetRedPlayer().Id);
        var response =  UndoHelpers.UndoFromUser(gs, request);

        Assert.True(response.Success);
        Assert.Equal(0, gs.CountCitiesForPlayer(board.GetRedPlayer()));
        Assert.Equal(3, board.GetRedPlayer().Resources[ResourceType.Ore]);

        request = new BaseRequest(board.GetRedPlayer().Id);
        response =  UndoHelpers.UndoFromUser(gs, request);
        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Ore]);
        Assert.Equal(1, board.GetRedPlayer().DevCardsReadyToPlay.Count(dc => dc == DevelopmentCardType.YearOfPlenty));
        Assert.Contains(gs.EventRecord, e => e.Id == cityEventIdToUndo + 1 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == cityEventIdToUndo);
        Assert.Contains(gs.EventRecord, e => e.Id == yopEventIdToUndo + 3 && e.Action == EventRecordAction.Undo && e.EventReversed != null && e.EventReversed == yopEventIdToUndo);
    }
}