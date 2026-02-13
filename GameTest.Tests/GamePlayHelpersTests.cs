using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace GameTest.Tests;

public class GamePlayHelpersTests
{
    [Fact]
    public void GetVictoryPointsForBuild_ForSettlement()
    {
        // Arrange
        // Act
        // Assert
        Assert.Equal(1, GamePlayHelpers.GetVictoryPointsForBuild(BuildingType.Settlement));
    }

    [Fact]
    public void GetVictoryPointsForBuild_ForCity()
    {
        // Arrange
        // Act
        // Assert
        Assert.Equal(2, GamePlayHelpers.GetVictoryPointsForBuild(BuildingType.City));
    }

    [Fact]
    public void HasResourcesToBuildRoad_SufficientResources()
    {
        // Arrange
        var player = TestHelpers.CreatePlayerWithSufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildRoad(player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasResourcesToBuildRoad_InsufficientResources()
    {
        // Arrange
        var player = TestHelpers.CreatePlayerWithInsufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildRoad(player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    void HasResourcesToBuildSettlment_SufficientResources()
    {
        // Arrange
        var player = TestHelpers.CreatePlayerWithSufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildSettlement(player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HsResourcesToBuildSettlement_InufficientResources()
    {
        // Arrange
        var player = TestHelpers.CreatePlayerWithInsufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildSettlement(player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasResourcesToBuildCity_SufficientResources()
    {
        // Arrange
        var player = TestHelpers.CreatePlayerWithSufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildCity(player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasResourcesToBuildCity_InsufficientResources()
    {
        // Arrange
        var player = TestHelpers.CreatePlayerWithInsufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildCity(player);

        // Assert
        Assert.False(result);
    }


    [Fact]
    public void HasResourcesToBuyDevCard_SufficientResources()
    {
        var player = TestHelpers.CreatePlayerWithSufficientResources();
        Assert.True(GamePlayHelpers.HasResourcesToBuyDevCard(player));        
    }

    [Fact]
    public void HasResourcesToBuyDevCard_InsufficientResources()
    {
        var player = TestHelpers.CreatePlayerWithInsufficientResources();
        Assert.False(GamePlayHelpers.HasResourcesToBuyDevCard(player));        
    }

    [Fact]
    public void CountVictoryPointsForPlayer_IncludeAllDevCardBuckets()
    {
        // Arrage
        var player = Player.CreateTestPlayer("Allen", PlayerColor.Red);
        player.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        player.AssignDevelopmentCard(DevelopmentCardType.RoadBuilding);
        player.MakeNewDevelopmentCardsPlayable();
        player.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);

        // Act
        var count = GamePlayHelpers.CountVictoryPointDevCardsForPlayer(player);

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void EndTurn_MovesToNextPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        GamePlayHelpers.EndTurn(board.GetRedPlayer(), board.GetGameState());

        Assert.True(GamePhaseTests.IsNextPhaseAsExpected(board.GetGameState().Phase, GameStates.RollOrUseDevCard, board.GetBluePlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void EndTurn_ExceptionIfNoCurrentPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        Assert.Null(board.GetGameState().Phase.CurrentPlayer);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.EndTurn(board.GetRedPlayer(), board.GetGameState()));
    }

    [Fact]
    public void EndTurn_ExceptionIfNotPlayersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.EndTurn(board.GetBluePlayer(), board.GetGameState()));
    }

    [Fact]
    public void EndTurn_ExceptionIfNotBuildOrTradePhase()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.EndTurn(board.GetRedPlayer(), board.GetGameState()));
    }

    [Fact]
    public void PlayDevCard_SetsDevCardPlayedState()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetRedPlayer().AssignDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        board.GetRedPlayer().MakeNewDevelopmentCardsPlayable();

        Assert.False(board.GetGameState().Phase.DevCardPlayedThisRound);
        GamePlayHelpers.PlayYearOfPlentyDevCard(board.GetGameState(), board.GetRedPlayer(), new List<ResourceType>() { ResourceType.Wood, ResourceType.Brick});
        Assert.True(board.GetGameState().Phase.DevCardPlayedThisRound);
    }

    [Fact]
    public void EndTurn_ClearsDevCardPlayedState()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetDevCardPlayedThisRound();
        Assert.True(board.GetGameState().Phase.DevCardPlayedThisRound);

        GamePlayHelpers.EndTurn(board.GetRedPlayer(), board.GetGameState());

        Assert.False(board.GetGameState().Phase.DevCardPlayedThisRound);
    }

    [Fact]
    public void PlayDevCard_PreventSecondInSingleTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetRedPlayer().AssignDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        board.GetRedPlayer().AssignDevelopmentCard(DevelopmentCardType.Knight);
        board.GetRedPlayer().MakeNewDevelopmentCardsPlayable();

        Assert.False(board.GetGameState().Phase.DevCardPlayedThisRound);
        GamePlayHelpers.PlayYearOfPlentyDevCard(board.GetGameState(), board.GetRedPlayer(), new List<ResourceType>() { ResourceType.Wood, ResourceType.Brick});
        Assert.True(board.GetGameState().Phase.DevCardPlayedThisRound);
        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayKnightDevCard(board.GetGameState(), board.GetRedPlayer(), board.GetTile(TestTile.T3)));
    }

    [Fact]
    public void BuildRoad_MissingPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), "PP1", board.GetEdge(TestEdge.E2).Id);

        Assert.False(response.Success);
        Assert.Equal(1012, response.ErrorCode);
        Assert.Null(response.GameState);
        Assert.Empty(response.PossibleActions);
    }

    [Fact]
    public void BuildRoad_NotPlayersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, board.GetEdge(TestEdge.E2).Id);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
    }

    [Fact]
    public void BuildRoad_FailIfWrongPhase()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, board.GetEdge(TestEdge.E2).Id);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
    }

    [Fact]
    public void BuildRoad_BuildOrTradePhase_BuildsRoadNoPhaseChange()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetRedPlayer());

        board.GetRedPlayer().Resources[ResourceType.Wood] = 1;
        board.GetRedPlayer().Resources[ResourceType.Brick] = 1;

        // Act
        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, board.GetEdge(TestEdge.E2).Id);

        // Assert
        Assert.True(response.Success);
        var e2 = board.GetEdge(TestEdge.E2);
        Assert.NotNull(e2.Owner);
        Assert.Equal(board.GetRedPlayer().Id, e2.Owner.Id);
    }

    [Fact]
    public void BuildRoad_PlaceFirstRoadPhase_BuildsRoadAndPhaseChange()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetBluePlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetBluePlayer());

        // Act
        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetBluePlayer().Id, board.GetEdge(TestEdge.E2).Id);

        // Assert
        Assert.True(response.Success);
        var e2 = board.GetEdge(TestEdge.E2);
        Assert.NotNull(e2.Owner);
        Assert.Equal(board.GetBluePlayer().Id, e2.Owner.Id);
        Assert.True(GamePhaseTests.IsNextPhaseAsExpected(board.GetGameState().Phase, GameStates.PlaceSecondSettlement, board.GetBluePlayer()));
        Assert.NotNull(response.PossibleActions);
        Assert.Equal(2, response.PossibleActions.Count);
        Assert.Contains(response.PossibleActions, a => a.Action == PlayerAction.PlaceSettlement);
        var placeSettlementAction = response.PossibleActions.First(a => a.Action == PlayerAction.PlaceSettlement);
        Assert.NotNull(placeSettlementAction.VertexIds);
        Assert.NotEmpty(placeSettlementAction.VertexIds);
        Assert.Contains(response.PossibleActions, a => a.Action == PlayerAction.Undo);
    }

    [Fact]
    public void BuildRoad_NotAdjacentToBuilding()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetBluePlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetBluePlayer());

        // Act - Build road on non-adjacent edge.
        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetBluePlayer().Id, board.GetEdge(TestEdge.E1).Id);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(1015, response.ErrorCode);
    }

    [Fact]
    public void BuildRoad_NotAdjacentToBuildingOfRightPlayer()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetBluePlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetRedPlayer());

        // Act - Try to build blue road next to red vertex
        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetBluePlayer().Id, board.GetEdge(TestEdge.E2).Id);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(1015, response.ErrorCode);
    }

    [Fact]
    public void BuildSettlement_MissingPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(board.GetGameState(), "PP1", board.GetVertex(TestVertex.V2).Id);

        Assert.False(response.Success);
        Assert.Equal(1012, response.ErrorCode);
    }

    [Fact]
    public void BuildSettlement_NotPlayersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Id);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
    }

    // TODO: Need to add additional BuildSettlement tests.

    [Fact]
    public void BuildSettlement_FailIfWrongPhase()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Id);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
    }

    [Fact]
    public void BuildSettlement_BuildOrTradePhase_BuildsSettlementNoPhaseChange()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetEdge(TestEdge.E8).BuildRoad(board.GetRedPlayer());

        var vertex = board.GetVertex(TestVertex.V2);
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 1);
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 1);
        board.GetRedPlayer().AssignResources(ResourceType.Wool, 1);
        board.GetRedPlayer().AssignResources(ResourceType.Grain, 2);

        // Act
        var response = GamePlayHelpers.BuildSettlementRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, vertex.Id);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(vertex.Owner);
        Assert.NotNull(vertex.Building);
        Assert.Equal(board.GetRedPlayer().Id, vertex.Owner.Id);
        Assert.Equal(BuildingType.Settlement, vertex.Building);
        Assert.NotNull(response.PossibleActions);
        Assert.Equal(3, response.PossibleActions.Count);
        Assert.Contains(response.PossibleActions, a => a.Action == PlayerAction.TradeWithPlayers);
        Assert.Contains(response.PossibleActions, a => a.Action == PlayerAction.EndTurn);
        Assert.Contains(response.PossibleActions, a => a.Action == PlayerAction.Undo);
    }

    [Fact]
    public void BuildRoad_PlaceFirstSettlementPhase_BuildsSettlementAndPhaseChange()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(gs, board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Id);

        Assert.True(response.Success);
        Assert.NotNull(board.GetVertex(TestVertex.V5).Building);
        Assert.Equal(BuildingType.Settlement, board.GetVertex(TestVertex.V5).Building);
        Assert.NotNull(board.GetVertex(TestVertex.V5).Owner);
        Assert.Equal(board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Owner.Id);
        Assert.Equal(GameStates.PlaceFirstRoad, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(gs.Phase.CurrentPlayer.Id, board.GetRedPlayer().Id);
    }

    [Fact]
    public void GameLoop_DropOutIfNotBotsTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        GamePlayHelpers.GameLoop(board.GetGameState());

        Assert.NotNull(board.GetGameState().Phase.CurrentPlayer);
        Assert.False(board.GetGameState().Phase.CurrentPlayer.IsBot);
    }

    [Fact]
    public void GameLoop_BuildRoadAndSettlement()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var botPlayer = board.GetBluePlayer();
        var humanPlayer = board.GetRedPlayer();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, humanPlayer);

        var roadCountBefore = gs.CountRoadsForPlayer(botPlayer);
        var settlementCountBefore = gs.CountSettlementsForPlayer(botPlayer);

        botPlayer.Resources[ResourceType.Brick] = 2;
        botPlayer.Resources[ResourceType.Grain] = 2;
        botPlayer.Resources[ResourceType.Wool] = 1;
        botPlayer.Resources[ResourceType.Wood] = 2;

        GamePlayHelpers.GameLoop(gs);

        Assert.Equal(roadCountBefore + 1, gs.CountRoadsForPlayer(botPlayer));
        Assert.Equal(settlementCountBefore + 1, gs.CountSettlementsForPlayer(botPlayer));
    }

    [Fact]
    public void GameLoop_BotPlacingRobberOnTileWith2Opponents()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(false);
        var gs = board.GetGameState();
        var orangePlayer = Player.CreateTestPlayer("WallE", PlayerColor.Orange, true);
        gs.Players.Add(orangePlayer);
        board.GetVertex(TestVertex.V5).UpgradeToCity();
        board.GetRedPlayer().AssignResources(ResourceType.Ore, 4);
        board.GetBluePlayer().AssignResources(ResourceType.Grain, 3);
        gs.Phase = new GamePhase(GameStates.PlaceRobber, orangePlayer, board.GetRedPlayer());
        gs.Phase.SetStateToReturnTo(GameStates.RollOrUseDevCard, board.GetTile(TestTile.T6));

        GamePlayHelpers.GameLoop(gs);

        Assert.NotNull(gs.EventRecord);
        Assert.Single(gs.EventRecord, e => e.Action == EventRecordAction.StealResource);
        var orangeResources = AIHelpers.ConvertResourceDictToList(orangePlayer.Resources);
        Assert.Single(orangeResources);
        Assert.True(orangeResources[0] == ResourceType.Grain || orangeResources[0] == ResourceType.Ore);
    }

    [Fact]
    public void LinkEdgesAndVertices_AroundCenter()
    {
        // Arrange
        var gs = new GameState(new Guid(), "UT");

        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        gs.Tiles.Add(t1);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        gs.Tiles.Add(t2);
        var t3 = new Tile(ResourceType.Wood, 3, 2, 0);
        gs.Tiles.Add(t3);
        var t4 = new Tile(ResourceType.Ore, 8, 4, 0);
        gs.Tiles.Add(t4);
        var t5 = new Tile(ResourceType.Brick, 10, 3, -1);
        gs.Tiles.Add(t5);

        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gs);

        var edge12 = gs.Edges.First(e => e.Tiles.Contains(t1) && e.Tiles.Contains(t2));
        Assert.NotNull(edge12);
        var edge1NW = gs.Edges.First(e => e.Tiles.Count() == 1 && e.Tiles.Contains(t1) && e.Direction == HexDirection.NW);
        Assert.NotNull(edge1NW);
        var edge2W = gs.Edges.First(e => e.Tiles.Count() == 1 && e.Tiles.Contains(t2) && e.Direction == HexDirection.W);
        Assert.NotNull(edge2W);
        var edge13 = gs.Edges.First(e => e.Tiles.Contains(t1) && e.Tiles.Contains(t3));
        Assert.NotNull(edge13);
        var edge23 = gs.Edges.First(e => e.Tiles.Contains(t2) && e.Tiles.Contains(t3));
        Assert.NotNull(edge23);
        var edge34 = gs.Edges.First(e => e.Tiles.Contains(t3) && e.Tiles.Contains(t4));
        Assert.NotNull(edge34);
        var edge35 = gs.Edges.First(e => e.Tiles.Contains(t3) && e.Tiles.Contains(t5));
        Assert.NotNull(edge35);
        var edge45 = gs.Edges.First(e => e.Tiles.Contains(t4) && e.Tiles.Contains(t5));
        Assert.NotNull(edge45);
        var edge4NE = gs.Edges.First(e => e.Tiles.Count() == 1 && e.Tiles.Contains(t4) && e.Direction == HexDirection.NE);
        Assert.NotNull(edge4NE);
        var edge4E = gs.Edges.First(e => e.Tiles.Count() == 1 && e.Tiles.Contains(t4) && e.Direction == HexDirection.E);
        Assert.NotNull(edge4E);

        var vertex123 = gs.Vertices.First(v => v.Tiles.Contains(t1) && v.Tiles.Contains(t2) && v.Tiles.Contains(t3));
        Assert.NotNull(vertex123);
        var vertex12 = gs.Vertices.First(v => v.Tiles.Count == 2 && v.Tiles.Contains(t1) && v.Tiles.Contains(t2));
        Assert.NotNull(vertex12);
        var vertex34 = gs.Vertices.First(v => v.Tiles.Count == 2 && v.Tiles.Contains(t3) && v.Tiles.Contains(t4));
        Assert.NotNull(vertex34);
        var vertex345 = gs.Vertices.First(v => v.Tiles.Contains(t3) && v.Tiles.Contains(t4) && v.Tiles.Contains(t5));
        Assert.NotNull(vertex345);
        var vertex4SE = gs.Vertices.First(v => v.Tiles.Count == 1 && v.Tiles.Contains(t4) && v.Direction == VertexDirection.SE);
        Assert.NotNull(vertex4SE);
        var vertex4NE = gs.Vertices.First(v => v.Tiles.Count == 1 && v.Tiles.Contains(t4) && v.Direction == VertexDirection.NE);

        // Act
        BoardCreationHelpers.LinkEdgesAndVertices(gs);

        // Assert
        Assert.Equal(2, edge12.Vertices.Count());
        Assert.Contains(vertex12, edge12.Vertices);
        Assert.Contains(vertex123, edge12.Vertices);

        Assert.Equal(3, vertex12.Edges.Count());
        Assert.Contains(edge1NW, vertex12.Edges);
        Assert.Contains(edge2W, vertex12.Edges);
        Assert.Contains(edge12, vertex12.Edges);

        Assert.Equal(3, vertex123.Edges.Count());
        Assert.Contains(edge12, vertex123.Edges);
        Assert.Contains(edge23, vertex123.Edges);
        Assert.Contains(edge13, vertex123.Edges);

        Assert.Equal(3, vertex345.Edges.Count());
        Assert.Contains(edge34, vertex345.Edges);
        Assert.Contains(edge35, vertex345.Edges);
        Assert.Contains(edge45, vertex345.Edges);

        Assert.Equal(2, edge4E.Vertices.Count());
        Assert.Contains(vertex4NE, edge4E.Vertices);
        Assert.Contains(vertex4SE, edge4E.Vertices);

        Assert.Equal(2, vertex4NE.Edges.Count());
        Assert.Contains(edge4NE, vertex4NE.Edges);
        Assert.Contains(edge4E, vertex4NE.Edges);
    }

    [Fact]
    public void GetEdgeFromTileInfo_2Tile()
    {
        // Arrange
        GameState gs = new GameState(new Guid(), "UT");
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var e1 = new Edge(t1, HexDirection.NW);
        gs.Edges.Add(e1);
        var e2 = new Edge(t1, HexDirection.E);
        gs.Edges.Add(e2);
        var e3 = new Edge(t1, t2);
        gs.Edges.Add(e3);
        var e4 = new Edge(t2, HexDirection.W);
        gs.Edges.Add(e4);

        // Act
        var edge = gs.GetEdgeFromTileInfo(t1, t2, null);

        // Assert
        Assert.Equal(e3.Id, edge.Id);
    }

    [Fact]
    public void GetEdgeFromTileInfo_1Tile()
    {
        // Arrange
        GameState gs = new GameState(new Guid(), "UT");
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var e1 = new Edge(t1, HexDirection.NW);
        gs.Edges.Add(e1);
        var e2 = new Edge(t1, HexDirection.E);
        gs.Edges.Add(e2);
        var e3 = new Edge(t1, t2);
        gs.Edges.Add(e3);
        var e4 = new Edge(t2, HexDirection.W);
        gs.Edges.Add(e4);

        // Act
        var edge = gs.GetEdgeFromTileInfo(t1, null, HexDirection.E);

        // Assert
        Assert.Equal(e2.Id, edge.Id);
    }

    [Fact]
    public void GetEdgeFromTileInfo_IgnoreDirIfMoreThan1Tile()
    {
        // Arrange
        GameState gs = new GameState(new Guid(), "UT");
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);

        var e1 = new Edge(t1, HexDirection.W);
        gs.Edges.Add(e1);
        var e2 = new Edge(t1, t2);
        gs.Edges.Add(e2);

        // Act & Assert
        var edge = gs.GetEdgeFromTileInfo(t1, t2, HexDirection.E);

        Assert.Equal(e2.Id, edge.Id);
    }

    [Fact]
    public void GetEdgeFromTileInfo_EdgeNotFound()
    {
        // Arrange
        GameState gs = new GameState(new Guid(), "UT");
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Wood, 11, -2, 0);

        var e1 = new Edge(t1, t2);
        gs.Edges.Add(e1);
        var e2 = new Edge(t1, t3);
        gs.Edges.Add(e2);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
                gs.GetEdgeFromTileInfo(t2, t3, null));

        Assert.Equal("Sequence contains no matching element", exception.Message);
    }

    [Fact]
    public void GetVertexFromTileInfo_IgnoreDirIfMoreThan1Tile()
    {
        // Arrange
        GameState gs = new GameState(new Guid(), "UT");
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2, t3);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, VertexDirection.SW);

        var vertex = gs.GetVertexFromTileInfo(t1, t2, t3, VertexDirection.N);

        Assert.Equal(v1.Id, vertex.Id);
    }

    [Fact]
    public void GetVertexFromTileInfo_VertexNotFound()
    {
        // Arrange
        GameState gs = new GameState(new Guid(), "UT");
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, t3);
        gs.Vertices.Add(v2);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
                gs.GetVertexFromTileInfo(t1, t2, t3, null));

        Assert.Equal("Sequence contains no matching element", exception.Message);
    }

    [Fact]
    public void GetVertexFromTileInfo_3Tiles()
    {
        // Arrange
        GameState gs = new GameState(new Guid(), "UT");
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, t3);
        gs.Vertices.Add(v2);
        var v3 = new Vertex(t1, t2, t3);
        gs.Vertices.Add(v3);

        // Act & Assert
        var vertex = gs.GetVertexFromTileInfo(t1, t2, t3, null);

        Assert.Equal(vertex.Id, v3.Id);
    }

    [Fact]
    public void GetVertexFromTileInfo_2Tiles3rdMissing()
    {
        // Arrange
        GameState gs = new GameState(new Guid(), "UT");
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, t3);
        gs.Vertices.Add(v2);
        var v3 = new Vertex(t1, t2, t3);
        gs.Vertices.Add(v3);

        // Act & Assert
        var vertex = gs.GetVertexFromTileInfo(t1, t3, null, null);

        Assert.Equal(vertex.Id, v2.Id);
    }

    [Fact]
    public void GetVertexFromTileInfo_2Tiles2ndMissing()
    {
        // Arrange
        GameState gs = new GameState(new Guid(), "UT");
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, t3);
        gs.Vertices.Add(v2);
        var v3 = new Vertex(t1, t2, t3);
        gs.Vertices.Add(v3);

        // Act & Assert
        var vertex = gs.GetVertexFromTileInfo(t1, null, t3, null);

        Assert.Equal(vertex.Id, v2.Id);
    }

    [Fact]
    public void GetVertexFromTileInfo_1Tiles()
    {
        // Arrange
        GameState gs = new GameState(new Guid(), "UT");
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, t3);
        gs.Vertices.Add(v2);
        var v3 = new Vertex(t1, VertexDirection.S);
        gs.Vertices.Add(v3);

        // Act & Assert
        var vertex = gs.GetVertexFromTileInfo(t1, null, null, VertexDirection.S);

        Assert.Equal(vertex.Id, v3.Id);
    }

    [Fact]
    public void MarkBlockedVertices_CitiesAndSettlements()
    {
        // Find tiles on vertices I will build or test
        // TODO: Change to use a test board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);
        var t9 = gs.GetTileAt(4, 0);
        var t10 = gs.GetTileAt(3, -1);
        var t17 = gs.GetTileAt(2, 0);
        var t3 = gs.GetTileAt(-4, 0);
        var t4 = gs.GetTileAt(-3, 1);
        var t14 = gs.GetTileAt(-2, 0);
        var t15 = gs.GetTileAt(-1, 1);
        var t18 = gs.GetTileAt(1, -1);
        var t8 = gs.GetTileAt(3, 1);
        var t16 = gs.GetTileAt(1, 1);
        var t5 = gs.GetTileAt(-2, 2);
        var t2 = gs.GetTileAt(-3, -1);

        // build on vertices
        var v53 = gs.GetVertexFromTileInfo(t9, null, null, VertexDirection.NE);
        v53.BuildSettlement(gs.Players[1]);
        v53.UpgradeToCity();

        var v43 = gs.GetVertexFromTileInfo(t9, t10, t17, null);
        v43.BuildSettlement(gs.Players[0]);

        var v19 = gs.GetVertexFromTileInfo(t3, t4, null, null);
        v19.BuildSettlement(gs.Players[1]);

        // Act
        GamePlayHelpers.MarkBlockedVertices(gs);

        // Assert
        // Verify that vertices that should be blocked by above buildings is blocked
        var v52 = gs.GetVertexFromTileInfo(t9, t10, null, null);
        Assert.Equal(BuildingType.Blocked, v52.Building);
        var v54 = gs.GetVertexFromTileInfo(t9, null, null, VertexDirection.SE);
        Assert.Equal(BuildingType.Blocked, v54.Building);
        var v42 = gs.GetVertexFromTileInfo(t10, t17, t18, null);
        Assert.Equal(BuildingType.Blocked, v42.Building);
        var v39 = gs.GetVertexFromTileInfo(t8, t9, t17, null);
        Assert.Equal(BuildingType.Blocked, v39.Building);
        var v18 = gs.GetVertexFromTileInfo(t3, t4, t14, null);
        Assert.Equal(BuildingType.Blocked, v18.Building);
        var v20 = gs.GetVertexFromTileInfo(t3, null, null, VertexDirection.SW);
        Assert.Equal(BuildingType.Blocked, v20.Building);
        var v25 = gs.GetVertexFromTileInfo(t4, null, null, VertexDirection.SW);
        Assert.Equal(BuildingType.Blocked, v25.Building);

        // Verify that vertices not blocked by above buildings is free (i.e. null)
        var v51 = gs.GetVertexFromTileInfo(t10, null, null, VertexDirection.NE);
        Assert.Null(v51.Building);
        var v40 = gs.GetVertexFromTileInfo(t8, t9, null, null);
        Assert.Null(v40.Building);
        var v34 = gs.GetVertexFromTileInfo(t8, t16, t17, null);
        Assert.Null(v34.Building);
        var v21 = gs.GetVertexFromTileInfo(t3, null, null, VertexDirection.NW);
        Assert.Null(v21.Building);
        var v24 = gs.GetVertexFromTileInfo(t4, t5, null, null);
        Assert.Null(v24.Building);
        var v15 = gs.GetVertexFromTileInfo(t2, t3, t14, null);
        Assert.Null(v15.Building);
        var v22 = gs.GetVertexFromTileInfo(t4, t14, t15, null);
        Assert.Null(v22.Building);

    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_AdjacentToRoad()
    {
        // TODO: Change to use a Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        var woodTile = gs.GetTileAt(2, 0);
        var oreTile = gs.GetTileAt(4, 0);
        var woolTile = gs.GetTileAt(3, 1);

        var testEdge = gs.GetEdgeFromTileInfo(woodTile, woolTile, null);
        var road = gs.GetEdgeFromTileInfo(oreTile, woolTile, null);
        road.BuildRoad(gs.Players[0]);

        Assert.True(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_AdjacentToSettlement()
    {
        // TODO: Change to use a Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        var woodTile = gs.GetTileAt(2, 0);
        var oreTile = gs.GetTileAt(4, 0);
        var woolTile = gs.GetTileAt(3, 1);

        var testEdge = gs.GetEdgeFromTileInfo(woodTile, woolTile, null);
        var settlement = gs.GetVertexFromTileInfo(oreTile, woolTile, woodTile, null);
        settlement.BuildSettlement(gs.Players[0]);

        Assert.True(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_AdjacentToCity()
    {
        // TODO: Change to use a Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        var woodTile = gs.GetTileAt(2, 0);
        var oreTile = gs.GetTileAt(4, 0);
        var woolTile = gs.GetTileAt(3, 1);

        var testEdge = gs.GetEdgeFromTileInfo(woodTile, woolTile, null);
        var city = gs.GetVertexFromTileInfo(oreTile, woolTile, woodTile, null);
        city.BuildSettlement(gs.Players[0]);
        city.UpgradeToCity();

        Assert.True(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_NotAdjacentToAnything()
    {
        // TODO: Change to use a Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        var woodTile = gs.GetTileAt(2, 0);
        var oreTile = gs.GetTileAt(4, 0);
        var woolTile = gs.GetTileAt(3, 1);

        var testEdge = gs.GetEdgeFromTileInfo(woodTile, woolTile, null);
        var road = gs.GetEdgeFromTileInfo(woolTile, null, HexDirection.E);
        road.BuildRoad(gs.Players[0]);

        Assert.False(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_NotAdjacentToRightPlayer()
    {
        // TODO: Change to use a Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        var woodTile = gs.GetTileAt(2, 0);
        var oreTile = gs.GetTileAt(4, 0);
        var woolTile = gs.GetTileAt(3, 1);

        var testEdge = gs.GetEdgeFromTileInfo(woodTile, woolTile, null);
        var road = gs.GetEdgeFromTileInfo(oreTile, woolTile, null);
        road.BuildRoad(gs.Players[1]);

        Assert.False(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_NotAdjacentIfRoadSplitByOtherPlayer()
    {
        // TODO: Change to use a Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        var woodTile = gs.GetTileAt(2, 0);
        var oreTile = gs.GetTileAt(4, 0);
        var woolTile = gs.GetTileAt(3, 1);

        var testEdge = gs.GetEdgeFromTileInfo(woodTile, woolTile, null);
        var road = gs.GetEdgeFromTileInfo(oreTile, woolTile, null);
        road.BuildRoad(gs.Players[0]);

        var settlement = gs.GetVertexFromTileInfo(oreTile, woolTile, woodTile, null);
        settlement.BuildSettlement(gs.Players[1]);

        Assert.False(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsVertexAdjacentToPlayerRoad_NotAdjacentToAnything()
    {
        // TODO: Change to use a Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        var woodTile = gs.GetTileAt(2, 0);
        var oreTile = gs.GetTileAt(4, 0);
        var woolTile = gs.GetTileAt(3, 1);

        var testVertex = gs.GetVertexFromTileInfo(woodTile, oreTile, woolTile, null);
        var road = gs.GetEdgeFromTileInfo(oreTile, null, HexDirection.NE);
        road.BuildRoad(gs.Players[0]);

        Assert.False(GamePlayHelpers.IsVertexAdjacentToPlayerRoad(gs, testVertex, gs.Players[0]));
    }

    [Fact]
    public void IsVertexAdjacentToPlayerRoad_AdjacentToRoad()
    {
        // TODO: Change to use a Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        var woodTile = gs.GetTileAt(2, 0);
        var oreTile = gs.GetTileAt(4, 0);
        var woolTile = gs.GetTileAt(3, 1);

        var testVertex = gs.GetVertexFromTileInfo(woodTile, oreTile, woolTile, null);
        var road = gs.GetEdgeFromTileInfo(oreTile, woodTile, null);
        road.BuildRoad(gs.Players[0]);

        Assert.True(GamePlayHelpers.IsVertexAdjacentToPlayerRoad(gs, testVertex, gs.Players[0]));
    }

    private TradeRequestDTO CreateTradeRequestDTO(Player player, 
        ResourceType offerType, int offerCount, ResourceType requestType, int requestCount)
    {
        return new TradeRequestDTO(player.Id, 
            new Dictionary<ResourceType, int>() {{ offerType, offerCount}}, 
            new Dictionary<ResourceType, int>() {{ requestType, requestCount}});
    }

    private TradeRequest CreateTradeRequest(Player player, 
        ResourceType offerType, int offerCount, ResourceType requestType, int requestCount)
    {
        return new TradeRequest(player, 
            new Dictionary<ResourceType, int>() {{ offerType, offerCount}}, 
            new Dictionary<ResourceType, int>() {{ requestType, requestCount}});
    }

    [Fact]
    public void BankTradeFromUser_Accepted()
    {
        // TODO: Should change to Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        gs.Phase.PhaseState = GameStates.BuildOrTrade;
        gs.Phase.CurrentPlayer = gs.Players[0];

        gs.Players[0].AssignResources(ResourceType.Wood, 5);
        var tradeDTO = CreateTradeRequestDTO(gs.Players[0], ResourceType.Wood, 4, ResourceType.Brick, 1);

        var response = GamePlayHelpers.BankTradeFromUser(gs, tradeDTO);

        Assert.True(response.Success);
        Assert.Equal(1, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(1, gs.Players[0].Resources[ResourceType.Brick]);
        Assert.NotNull(response.PossibleActions);
        Assert.Equal(3, response.PossibleActions.Count);
    }

    [Fact]
    public void BankTradeFromUser_Rejected_WrongState()
    {
        // TODO: Should change to Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        gs.Phase.PhaseState = GameStates.RollOrUseDevCard;

        gs.Players[0].AssignResources(ResourceType.Wood, 5);
        var tradeDTO = CreateTradeRequestDTO(gs.Players[0], ResourceType.Wood, 4, ResourceType.Brick, 1);

        var response = GamePlayHelpers.BankTradeFromUser(gs, tradeDTO);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Equal(5, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(0, gs.Players[0].Resources[ResourceType.Brick]);
    }

    [Fact]
    public void BankTradeFromUser_Rejected_DoesNotHaveEnoughResources()
    {
        // TODO: Should change to Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        gs.Phase.PhaseState = GameStates.BuildOrTrade;
        gs.Phase.CurrentPlayer = gs.Players[0];


        gs.Players[0].AssignResources(ResourceType.Wood, 2);
        var tradeDTO = CreateTradeRequestDTO(gs.Players[0], ResourceType.Wood, 4, ResourceType.Brick, 1);

        var response = GamePlayHelpers.BankTradeFromUser(gs, tradeDTO);

        Assert.False(response.Success);
        Assert.Equal(1006, response.ErrorCode);
        Assert.Equal(2, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(0, gs.Players[0].Resources[ResourceType.Brick]);
    }

    [Fact]
    public void BankTrade_Accepted()
    {
        // TODO: Should change to Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        gs.Players[0].AssignResources(ResourceType.Wood, 5);
        var tradeRequest = CreateTradeRequest(gs.Players[0], ResourceType.Wood, 4, ResourceType.Brick, 1);

        var response = GamePlayHelpers.BankTrade(new GameState(new Guid(), "UT"), tradeRequest);

        Assert.True(response.Success);
        Assert.Equal(1, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(1, gs.Players[0].Resources[ResourceType.Brick]);
    }

    [Fact]
    public void BankTrade_Rejected()
    {
        // TODO: Should change to Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        gs.Players[0].AssignResources(ResourceType.Wood, 3);
        var tradeRequest = CreateTradeRequest(gs.Players[0], ResourceType.Wood, 2, ResourceType.Brick, 1);

        var response = GamePlayHelpers.BankTrade(new GameState(new Guid(), "UT"), tradeRequest);

        Assert.False(response.Success);
        Assert.Equal(1008, response.ErrorCode);
        Assert.Equal(3, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(0, gs.Players[0].Resources[ResourceType.Brick]);
    }

    [Fact]
    public void BankTradeFromUser_Rejected_BankDoesNotHaveRequestedResource()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var player = board.GetRedPlayer();
        var gs = board.GetGameState();
        player.AssignResources(ResourceType.Wood, 5);
        gs.AssignResourcesToPlayer(board.GetBluePlayer(), ResourceType.Brick, 19);
        Assert.Equal(0, gs.GetBankResourceCount(ResourceType.Brick));
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, player);

        var tradeRequest = CreateTradeRequestDTO(player, ResourceType.Wood, 4, ResourceType.Brick, 1);
        var response = GamePlayHelpers.BankTradeFromUser(gs, tradeRequest);

        Assert.False(response.Success);
        Assert.Equal(1076, response.ErrorCode);
        Assert.Equal(5, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(0, gs.Players[0].Resources[ResourceType.Brick]);
        Assert.Equal(0, gs.GetBankResourceCount(ResourceType.Brick));
    }

    [Fact]
    public void BankTrade_Rejected_BankDoesNotHaveRequestedResource()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var player = board.GetRedPlayer();
        var gs = board.GetGameState();
        player.AssignResources(ResourceType.Wood, 5);
        gs.AssignResourcesToPlayer(board.GetBluePlayer(), ResourceType.Brick, 19);
        Assert.Equal(0, gs.GetBankResourceCount(ResourceType.Brick));

        var tradeRequest = CreateTradeRequest(player, ResourceType.Wood, 4, ResourceType.Brick, 1);
        var response = GamePlayHelpers.BankTrade(gs, tradeRequest);

        Assert.False(response.Success);
        Assert.Equal(1076, response.ErrorCode);
        Assert.Equal(5, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(0, gs.Players[0].Resources[ResourceType.Brick]);
        Assert.Equal(0, gs.GetBankResourceCount(ResourceType.Brick));
    }

    [Fact]
    public void PopulatePlayerPorts_Valid()
    {
        // Arrange
        // TODO: Should change to Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        var bot = gs.Players.First(p => p.IsBot);
        var human = gs.Players.First(p => !p.IsBot);

        var grain12Tile = gs.GetTileAt(-3, -1);
        var grain9Tile = gs.GetTileAt(-4, 0);
        var brick8Tile = gs.GetTileAt(-3, 1);
        var ore8Tile = gs.GetTileAt(4, 0);


        gs.GetVertexFromTileInfo(grain12Tile, grain9Tile, null, null).BuildSettlement(human);
        var v1 = gs.GetVertexFromTileInfo(grain9Tile, brick8Tile, null, null);
        v1.BuildSettlement(human);
        v1.UpgradeToCity();

        gs.GetVertexFromTileInfo(ore8Tile, null, null, VertexDirection.NE).BuildSettlement(bot);

        // Act
        GamePlayHelpers.PopulatePlayerPorts(gs);

        // Assert
        Assert.Equal(2, human.Ports.Count);
        Assert.Contains(PortType.Wood, human.Ports);
        Assert.Contains(PortType.Brick, human.Ports);
        Assert.Single(bot.Ports);
        Assert.Contains(PortType.ThreeToOne, bot.Ports);
    }

    [Fact]
    public void RollDice_InvalidState_Exception()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase.PhaseState = GameStates.BuildOrTrade;
        board.GetGameState().Phase.CurrentPlayer = board.GetRedPlayer();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.RollDice(board.GetGameState()));
    }

    private static GameState CreateGameForRobberTesting(GameStates previousState)
    {
        var gs = new GameState(new Guid(), "UT");
        var player1 = Player.CreateTestPlayer("Time", PlayerColor.Red);
        gs.Players.Add(player1);
        var player2 = Player.CreateTestPlayer("Mary", PlayerColor.Blue);
        gs.Players.Add(player2);

        var tile1 = new Tile(ResourceType.Wood, 3, 0, 0);
        gs.Tiles.Add(tile1);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        gs.Tiles.Add(tile2);

        gs.Phase.CurrentPlayer = player1;
        gs.Phase.PhaseState = GameStates.PlaceRobber;
        gs.Phase.SetStateToReturnTo(previousState, tile1);

        return gs;        
    }

    [Fact]
    public void PlaceRobberForUser_InvalidState()
    {
        var gs = CreateGameForRobberTesting(GameStates.RollOrUseDevCard);
        gs.Phase.PhaseState = GameStates.RollOrUseDevCard;

        var response = GamePlayHelpers.PlaceRobberForUser(gs, gs.Players[0].Id, gs.Tiles[0].Id);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
    }

    [Fact]
    public void PlaceRobberForUser_NotPlayersTurn()
    {
        var gs = CreateGameForRobberTesting(GameStates.RollOrUseDevCard);

        var response = GamePlayHelpers.PlaceRobberForUser(gs, gs.Players[1].Id, gs.Tiles[0].Id);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
    }

    [Fact]
    public void PlaceRobberForUser_UnknownTile()
    {
        var gs = CreateGameForRobberTesting(GameStates.RollOrUseDevCard);
        var tile = new Tile(ResourceType.Ore, 5, -2, 0);

        var response = GamePlayHelpers.PlaceRobberForUser(gs, gs.Players[0].Id, tile.Id);

        Assert.False(response.Success);
        Assert.Equal(1032, response.ErrorCode);
    }

    [Fact]
    public void PlaceRobberForUser_RobberNotMoved()
    {
        var gs = CreateGameForRobberTesting(GameStates.RollOrUseDevCard);

        var response = GamePlayHelpers.PlaceRobberForUser(gs, gs.Players[0].Id, gs.Phase.OriginalRobberTile.Id);

        Assert.False(response.Success);
        Assert.Equal(1033, response.ErrorCode);
    }

    [Fact]
    public void PlaceRobberForUser_ValidMove()
    {
        var gs = CreateGameForRobberTesting(GameStates.BuildOrTrade);

        var response = GamePlayHelpers.PlaceRobberForUser(gs, gs.Players[0].Id, gs.Tiles[1].Id);

        Assert.True(response.Success);
        Assert.Equal(gs.Tiles[1].Id, gs.RobberTile.Id);
        Assert.Null(gs.Phase.PreviousState);
        Assert.NotNull(response.PossibleActions);
        Assert.NotEmpty(response.PossibleActions);
    }

    [Fact]
    public void PlaceRobber_ValidMove()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var human = board.GetRedPlayer();
        var bot = board.GetBluePlayer();
        bot.AssignResources(ResourceType.Ore, 2);
        var botTile = board.GetTile(TestTile.T2);

        board.GetGameState().Phase.CurrentPlayer = human;
        board.GetGameState().Phase.PhaseState = GameStates.PlaceRobber;
        board.GetGameState().Phase.SetStateToReturnTo(GameStates.BuildOrTrade, board.GetGameState().RobberTile);
        board.GetGameState().Phase.SetTargetPlayers(new List<Player>() {board.GetBluePlayer()});

        Assert.Contains(ResourceType.Ore, human.Resources);
        Assert.Equal(0, human.Resources[ResourceType.Ore]);
        Assert.Contains(ResourceType.Ore, bot.Resources);
        Assert.Equal(2, bot.Resources[ResourceType.Ore]);

        // Act
        GamePlayHelpers.PlaceRobber(board.GetGameState(), human, botTile);
        GamePlayHelpers.GameLoop(board.GetGameState());

        // Assert
        Assert.Equal(botTile.Id, board.GetGameState().RobberTile.Id);
        Assert.Null(board.GetGameState().Phase.PreviousState);
        Assert.Equal(GameStates.BuildOrTrade, board.GetGameState().Phase.PhaseState);
        Assert.Contains(ResourceType.Ore, human.Resources);
        Assert.Equal(1, human.Resources[ResourceType.Ore]);
        Assert.Contains(ResourceType.Ore, bot.Resources);
        Assert.Equal(1, bot.Resources[ResourceType.Ore]);
    }

    private static GameState CreateGameForBuyDevCardTesting(GameStates currentState)
    {
        var gs = new GameState(new Guid(), "UT");
        var player1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        gs.Players.Add(player1);
        player1.AssignResources(ResourceType.Ore, 1);
        player1.AssignResources(ResourceType.Grain, 2);
        player1.AssignResources(ResourceType.Wool, 1);
        var player2 = Player.CreateTestPlayer("Mary", PlayerColor.Blue, true);
        gs.Players.Add(player2);

        var tile1 = new Tile(ResourceType.Wood, 3, 0, 0);
        gs.Tiles.Add(tile1);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        gs.Tiles.Add(tile2);

        gs.Phase.CurrentPlayer = player1;
        gs.Phase.PhaseState = currentState;

        return gs;        
    }


    [Fact]
    public void BuyDevCardFromUser_NotCurrentPlayer()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var bot = gs.Players.First(p => p.IsBot);
        var human = gs.Players.First(p => !p.IsBot);
        Assert.Equal(human.Id, gs.Phase.CurrentPlayer.Id);
        Assert.Empty(bot.DevCardsPurchasedThisRound);
        Assert.Equal(0, bot.DevelopmentCardCount);

        var response = GamePlayHelpers.BuyDevCardFromUser(gs, bot.Id);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void BuyDevCardFromUser_WrongState()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.RollOrUseDevCard);
        var human = gs.Players.First(p => !p.IsBot);
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Equal(0, human.DevelopmentCardCount);

        var response = GamePlayHelpers.BuyDevCardFromUser(gs, human.Id);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Equal(0, human.DevelopmentCardCount);
    }

    [Fact]
    public void BuyDevCardFromUser_InsufficientResources()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var human = gs.Players.First(p => !p.IsBot);
        human.RemoveResources(ResourceType.Wool, 1); // Take away all wool.
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Equal(0, human.DevelopmentCardCount);

        var response = GamePlayHelpers.BuyDevCardFromUser(gs, human.Id);

        Assert.False(response.Success);
        Assert.Equal(1017, response.ErrorCode);
        Assert.Null(response.GameState);
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Equal(0, human.DevelopmentCardCount);
    }

    [Fact]
    public void BuyDevCardFromUser_Valid()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var human = gs.Players.First(p => !p.IsBot);
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Equal(0, human.DevelopmentCardCount);
        var grainCount = human.Resources[ResourceType.Grain];
        var woolCount = human.Resources[ResourceType.Wool];
        var oreCount = human.Resources[ResourceType.Ore];

        var response = GamePlayHelpers.BuyDevCardFromUser(gs, human.Id);

        Assert.True(response.Success);
        Assert.Equal(0, response.ErrorCode);
        Assert.NotNull(response.GameState);
        Assert.Single(human.DevCardsPurchasedThisRound);
        Assert.Equal(1, human.DevelopmentCardCount);
        Assert.Empty(human.DevCardsPlayed);
        Assert.Empty(human.DevCardsReadyToPlay);
        Assert.Equal(grainCount - 1, human.Resources[ResourceType.Grain]);
        Assert.Equal(woolCount - 1, human.Resources[ResourceType.Wool]);
        Assert.Equal(oreCount - 1, human.Resources[ResourceType.Ore]);
        Assert.NotNull(response.PossibleActions);
        Assert.NotEmpty(response.PossibleActions);
    }

    [Fact]
    public void BuyDevCard_InvalidState()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.RollOrUseDevCard);
        var human = gs.Players.First(p => !p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.BuyDevCard(gs, human));
    }

    [Fact]
    public void BuyDevCard_NotCurrentUser()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var bot = gs.Players.First(p => p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.BuyDevCard(gs, bot));
    }

    [Fact]
    public void BuyDevCard_InsufficientResources()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var human = gs.Players.First(p => !p.IsBot);
        human.RemoveResources(ResourceType.Grain, 2); // remove all grain

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.BuyDevCard(gs, human));
    }

    [Fact]
    public void BuyDevCard_Valid()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var human = gs.Players.First(p => !p.IsBot);
        var grainCount = human.Resources[ResourceType.Grain];
        var woolCount = human.Resources[ResourceType.Wool];
        var oreCount = human.Resources[ResourceType.Ore];

        GamePlayHelpers.BuyDevCard(gs, human);

        Assert.Single(human.DevCardsPurchasedThisRound);
        Assert.Equal(1, human.DevelopmentCardCount);
        Assert.Empty(human.DevCardsPlayed);
        Assert.Empty(human.DevCardsReadyToPlay);
        Assert.Equal(grainCount - 1, human.Resources[ResourceType.Grain]);
        Assert.Equal(woolCount - 1, human.Resources[ResourceType.Wool]);
        Assert.Equal(oreCount - 1, human.Resources[ResourceType.Ore]);
    }

    [Fact]
    public void EndTurn_MovesDevCardsToReadyToPlayState()
    {
        // Arrange
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var human = gs.Players.First(p => !p.IsBot);

        GamePlayHelpers.BuyDevCard(gs, human);
        Assert.Single(human.DevCardsPurchasedThisRound);
        Assert.Equal(1, human.DevelopmentCardCount);

        // Act
        GamePlayHelpers.EndTurn(human, gs, true);

        // Assert
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Single(human.DevCardsReadyToPlay);
        Assert.Equal(1, human.DevelopmentCardCount);
    }

    private static GameState CreateGameForPlayDevCardTesting(GameStates currentState, DevelopmentCardType desiredType)
    {
        var gs = new GameState(new Guid(), "UT");
        var player1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        gs.Players.Add(player1);
        var player2 = Player.CreateTestPlayer("Mary", PlayerColor.Blue, true);
        gs.Players.Add(player2);
        var tile1 = new Tile(ResourceType.Wood, 3, 0, 0);
        gs.Tiles.Add(tile1);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        gs.Tiles.Add(tile2);
        gs.SetRobberTile(tile1);
        gs.Phase.CurrentPlayer = player1;

        // Assign some resources so that we can test monopoly
        player1.AssignResources(ResourceType.Wood, 1);
        player1.AssignResources(ResourceType.Brick, 2);
        player2.AssignResources(ResourceType.Wood, 2);
        player2.AssignResources(ResourceType.Ore, 1);

        // Build one settlement for each player so that resources and be stolen when knight placed
        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gs);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);
        var v1 = gs.GetVertexFromTileInfo(tile1, null, null, VertexDirection.SW);
        v1.BuildSettlement(player1);
        var v2 = gs.GetVertexFromTileInfo(tile2, null, null, VertexDirection.NE);
        v2.BuildSettlement(player2);
        GamePlayHelpers.MarkBlockedVertices(gs);

        // Customize test board for the test being done
        player1.AssignDevelopmentCard(desiredType);
        player1.MakeNewDevelopmentCardsPlayable();
        gs.Phase.PhaseState = currentState;

        return gs;        
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.PlaceRobber, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly, new List<ResourceType>() { ResourceType.Wood}, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var bot = gs.Players.First(p => p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(bot.Id, DevelopmentCardType.Monopoly, new List<ResourceType>() { ResourceType.Wood}, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_NoResourceRequested()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly, new List<ResourceType>(), null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1037, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_NullResourceRequested()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly, null, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1037, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_MultipleResourcesRequested()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly, 
            new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick}, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1037, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_RequestDesert()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly, 
            new List<ResourceType>()  { ResourceType.Desert}, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1038, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_NoMonopolyCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly, 
            new List<ResourceType>()  { ResourceType.Wood }, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1039, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_AlreadyPlayedDevCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        gs.Phase.SetDevCardPlayedThisRound();
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly, 
            new List<ResourceType>()  { ResourceType.Wood }, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1043, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        var monopolyCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Monopoly);
        var woodCount = human.Resources[ResourceType.Wood];
        Assert.True(woodCount > 0);
        var botWoodCount = bot.Resources[ResourceType.Wood];
        Assert.True(botWoodCount > 0);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly, 
            new List<ResourceType>()  { ResourceType.Wood }, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(monopolyCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Monopoly));
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Empty(human.DevCardsPlayed);
        Assert.Equal(woodCount + botWoodCount, human.Resources[ResourceType.Wood]);
        Assert.Equal(0, bot.Resources[ResourceType.Wood]);
        Assert.NotNull(response.PossibleActions);
        Assert.NotEmpty(response.PossibleActions);
    }

    [Fact]
    public void PlayMonopolyDevCard_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.SettingUpBoard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayMonopolyDevCard(gs, human, ResourceType.Wood));
    }

    [Fact]
    public void PlayMonopolyDevCard_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var bot = gs.Players.First(p => p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayMonopolyDevCard(gs, bot, ResourceType.Wood));
    }

    [Fact]
    public void PlayMonopolyDevCard_PlayerDoesntHaveDevCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayMonopolyDevCard(gs, human, ResourceType.Wood));
    }

    [Fact]
    public void PlayMonopolyDevCard_RequestDesert()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayMonopolyDevCard(gs, human, ResourceType.Desert));
    }

    [Fact]
    public void PlayMonopolyDevCard_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        var monopolyCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Monopoly);
        var woodCount = human.Resources[ResourceType.Wood];
        Assert.True(woodCount > 0);
        var botWoodCount = bot.Resources[ResourceType.Wood];
        Assert.True(botWoodCount > 0);
        var countDevCardsInDeck = gs.DevelopmentCards.Count;

        GamePlayHelpers.PlayMonopolyDevCard(gs, human, ResourceType.Wood);

        Assert.Equal(monopolyCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Monopoly));
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Empty(human.DevCardsPlayed);
        Assert.Equal(woodCount + botWoodCount, human.Resources[ResourceType.Wood]);
        Assert.Equal(0, bot.Resources[ResourceType.Wood]);
        Assert.Equal(countDevCardsInDeck + 1, gs.DevelopmentCards.Count);
        Assert.Equal(DevelopmentCardType.Monopoly, gs.DevelopmentCards.Last());
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.PlaceSecondSettlement, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var bot = gs.Players.First(p => p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(bot.Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_TooFewResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>()  { ResourceType.Wood }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1040, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_NoResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>(), null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1040, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_ResourcesSelectedNull()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, 
            null, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1040, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_TooManyResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick, ResourceType.Brick }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1040, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_InvalidFirstResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>()  { ResourceType.Desert, ResourceType.Wood }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1038, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_WrongDevCardPlayed()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly, 
            new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(9999, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_PlayerDoesntHaveDevCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1039, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_AlreadyPlayedDevCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.YearOfPlenty);
        gs.Phase.SetDevCardPlayedThisRound();
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1043, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_BankDoesntHaveResources()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        var countWood = human.Resources[ResourceType.Wood];
        var countBrick = human.Resources[ResourceType.Brick];
        var countYearOfPlenty = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty);
        gs.AssignResourcesToPlayer(bot, ResourceType.Wood, 19); // Take all wood from bank

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1076, response.ErrorCode);
        Assert.Null(response.GameState);
        Assert.Equal(countWood, human.Resources[ResourceType.Wood]);
        Assert.Equal(countBrick, human.Resources[ResourceType.Brick]);
        Assert.Equal(countYearOfPlenty, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty));
    }

    [Fact]
    public void PlayYearOfPlentyDevCard_BankDoesntHaveResources()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        var countWood = human.Resources[ResourceType.Wood];
        var countBrick = human.Resources[ResourceType.Brick];
        var countYearOfPlenty = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty);
        gs.AssignResourcesToPlayer(bot, ResourceType.Wood, 19); // Take all wood from bank

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayYearOfPlentyDevCard(gs, human, 
            new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick }));

        Assert.Equal(countWood, human.Resources[ResourceType.Wood]);
        Assert.Equal(countBrick, human.Resources[ResourceType.Brick]);
        Assert.Equal(countYearOfPlenty, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty));
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);
        var countWood = human.Resources[ResourceType.Wood];
        var countBrick = human.Resources[ResourceType.Brick];
        var countYearOfPlenty = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty);
        var countDevCardsInDeck = gs.DevelopmentCards.Count;

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, 
            new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.True(response.Success);
        Assert.Equal(0, response.ErrorCode);
        Assert.NotNull(response.GameState);
        Assert.Equal(countWood + 1, human.Resources[ResourceType.Wood]);
        Assert.Equal(countBrick + 1, human.Resources[ResourceType.Brick]);
        Assert.Equal(countYearOfPlenty - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty));
        Assert.NotNull(response.PossibleActions);
        Assert.NotEmpty(response.PossibleActions);
        Assert.Equal(countDevCardsInDeck + 1, gs.DevelopmentCards.Count);
        Assert.Equal(DevelopmentCardType.YearOfPlenty, gs.DevelopmentCards.Last());
    }

    [Fact]
    public void PlayYearOfPlentyDevCard_TooFewResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        var request = new List<ResourceType>() { ResourceType.Wood };

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayYearOfPlentyDevCard(gs, human, request));
    }

    [Fact]
    public void PlayYearOfPlentyDevCard_TooManyResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        var request = new List<ResourceType>() { ResourceType.Wood, ResourceType.Wood, ResourceType.Ore };

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayYearOfPlentyDevCard(gs, human, request));
    }

    [Fact]
    public void PlayYearOfPlentyDevCard_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var bot = gs.Players.First(p => p.IsBot);

        var request = new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick };

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayYearOfPlentyDevCard(gs, bot, request));
    }

    [Fact]
    public void PlayYearOfPlentyDevCard_RequestTwoOfSameResource()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);
        var grainCount = human.Resources[ResourceType.Grain];

        var request = new List<ResourceType>() { ResourceType.Grain, ResourceType.Grain};

        GamePlayHelpers.PlayYearOfPlentyDevCard(gs, human, request);

        Assert.Equal(grainCount + 2, human.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void PlayYearOfPlentyDevCard_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);
        var countWood = human.Resources[ResourceType.Wood];
        var countBrick = human.Resources[ResourceType.Brick];
        var countYearOfPlenty = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty);

        var request = new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick };

        GamePlayHelpers.PlayYearOfPlentyDevCard(gs, human, request);

        Assert.Equal(countWood + 1, human.Resources[ResourceType.Wood]);
        Assert.Equal(countBrick + 1, human.Resources[ResourceType.Brick]);
        Assert.Equal(countYearOfPlenty - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty));
    }

    [Fact]
    public void PlayRoadBuildingDevCardFromUser_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.SettingUpBoard, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.RoadBuilding, null, null);

        var response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayRoadBuildingDevCardFromUser_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.RoadBuilding);
        var bot = gs.Players.First(p => p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(bot.Id, DevelopmentCardType.RoadBuilding, null, null);

        var response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayRoadBuildingDevCardFromUser_WrongDevCardPlayed()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, null, null);

        var response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(9999, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayRoadBuildingDevCardFromUser_PlayDoesntHaveDevCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.RoadBuilding, null, null);

        var response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1039, response.ErrorCode);
        Assert.Null(response.GameState);
    }
    
    [Fact]
    public void PlayRoadBuildingDevCardFromUser_AlreadyPlayedDevCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.RoadBuilding);
        gs.Phase.SetDevCardPlayedThisRound();
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.RoadBuilding, null, null);

        var response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1043, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayRoadBuildingDevCardFromUser_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);
        var roadBuildingCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.RoadBuilding, null, null);

        var response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);

        Assert.True(response.Success);
        Assert.Equal(roadBuildingCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding));
        Assert.Equal(GameStates.FirstDevCardRoad ,gs.Phase.PhaseState);
        Assert.NotNull(response.PossibleActions);
        Assert.Equal(2, response.PossibleActions.Count);
        Assert.Contains(response.PossibleActions, a => a.Action == PlayerAction.PlaceRoad);
        Assert.Contains(response.PossibleActions, a => a.Action == PlayerAction.Undo);
    }

    [Fact]
    public void PlayRoadBuildingDevCard_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.SettingUpBoard, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);

        Assert.Throws<InvalidOperationException>( () => GamePlayHelpers.PlayRoadBuildingDevCard(gs, human));
    }

    [Fact]
    public void PlayRoadBuildingDevCard_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);
        var roadBuildingCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding);
        var countDevCardsInDeck = gs.DevelopmentCards.Count;

        GamePlayHelpers.PlayRoadBuildingDevCard(gs, human);

        Assert.Equal(roadBuildingCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding));
        Assert.Equal(GameStates.FirstDevCardRoad ,gs.Phase.PhaseState);
        Assert.Equal(countDevCardsInDeck + 1, gs.DevelopmentCards.Count);
        Assert.Equal(DevelopmentCardType.RoadBuilding, gs.DevelopmentCards.Last());
    }

    [Fact]
    public void PlayKnightDevCardFromUser_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.PlaceRobber, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        var t1 = gs.GetTileAt(2, 0);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight, null, t1.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var bot = gs.Players.First(p => p.IsBot);
        var t1 = gs.GetTileAt(2, 0);

        PlayDevCardRequest request = new PlayDevCardRequest(bot.Id, DevelopmentCardType.Knight, null, t1.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_TileIdMissing()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight, null, null);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1041, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_TileIdInvalid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight, null, "TT1");

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1032, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_RobberAlreadyOnTile()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight, null, gs.RobberTile.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1033, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_PlayerDoesNotHaveKnight()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);
        var t1 = gs.GetTileAt(2, 0);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight, null, t1.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1039, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_WrongDevCardPlayed()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        var t1 = gs.GetTileAt(2, 0);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty, null, t1.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(9999, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_AlreadyPlayedDevCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Knight);
        gs.Phase.SetDevCardPlayedThisRound();
        var human = gs.Players.First(p => !p.IsBot);
        var t1 = gs.GetTileAt(2, 0);
        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight, null, t1.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1043, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        var readyKnightCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Knight);
        var playedKnightCount = human.DevCardsPlayed.Count(d => d == DevelopmentCardType.Knight);
        var t1 = gs.GetTileAt(2, 0);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight, null, t1.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.True(response.Success);
        Assert.Equal(readyKnightCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding));
        Assert.Equal(playedKnightCount + 1, human.DevCardsPlayed.Count(d => d == DevelopmentCardType.Knight));
        Assert.Equal(GameStates.BuildOrTrade ,gs.Phase.PhaseState);
        Assert.NotNull(response.PossibleActions);
        Assert.NotEmpty(response.PossibleActions);
    }

    [Fact]
    public void PlayKnightDevCard_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var bot = gs.Players.First(p => p.IsBot);
        var t1 = gs.GetTileAt(2, 0);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayKnightDevCard(gs, bot, t1));
    }

    [Fact]
    public void PlayKnightDevCard_RobberAlreadyOnTile()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        var t1 = gs.GetTileAt(2, 0);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayKnightDevCard(gs, human, gs.RobberTile));
    }

    [Fact]
    public void PlayKnightDevCard_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        var readyKnightCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Knight);
        var playedKnightCount = human.DevCardsPlayed.Count(d => d == DevelopmentCardType.Knight);
        var t1 = gs.GetTileAt(2, 0);
        var humanResourceCount = human.ResourceCount;
        var botResourceCount = human.ResourceCount;

        GamePlayHelpers.PlayKnightDevCard(gs, human, t1);

        Assert.Equal(readyKnightCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding));
        Assert.Equal(playedKnightCount + 1, human.DevCardsPlayed.Count(d => d == DevelopmentCardType.Knight));
        Assert.Equal(GameStates.BuildOrTrade ,gs.Phase.PhaseState);
        Assert.Equal(humanResourceCount + 1, human.ResourceCount);
        Assert.Equal(botResourceCount - 1, bot.ResourceCount);
        Assert.Null(gs.PlayerWithLargestArmy);
    }

    [Fact]
    public void PlayKnightDevCard_GainLargestArmy()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        human.AssignDevelopmentCard(DevelopmentCardType.Knight);
        human.AssignDevelopmentCard(DevelopmentCardType.Knight);
        human.AssignDevelopmentCard(DevelopmentCardType.Monopoly);
        human.MakeNewDevelopmentCardsPlayable();
        human.PlayDevelopmentCard(DevelopmentCardType.Knight);
        human.PlayDevelopmentCard(DevelopmentCardType.Monopoly);
        human.PlayDevelopmentCard(DevelopmentCardType.Knight);
        gs.UpdatePlayerVictoryPoints(human);
        Assert.Null(gs.PlayerWithLargestArmy);
        Assert.Equal(1, human.FullVictoryPoints);

        human.AssignDevelopmentCard(DevelopmentCardType.Knight);
        human.MakeNewDevelopmentCardsPlayable();
        GamePlayHelpers.PlayKnightDevCard(gs, human, gs.GetTileAt(2,0));
        
        Assert.NotNull(gs.PlayerWithLargestArmy);
        Assert.Equal(human.Id, gs.PlayerWithLargestArmy.Id);
        Assert.Equal(3, human.FullVictoryPoints);

        var largestArmyEvent = gs.EventRecord.FirstOrDefault(e => e.Action == EventRecordAction.GainedLargestArmy);
        Assert.NotNull(largestArmyEvent);
        Assert.Equal(largestArmyEvent.PlayerId, human.Id);
    }

    private GameState CreateGameWhereBotLargestArmy()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);

        bot.AssignDevelopmentCard(DevelopmentCardType.Knight);
        bot.AssignDevelopmentCard(DevelopmentCardType.Knight);
        bot.AssignDevelopmentCard(DevelopmentCardType.Knight);
        bot.MakeNewDevelopmentCardsPlayable();
        bot.PlayDevelopmentCard(DevelopmentCardType.Knight);
        bot.PlayDevelopmentCard(DevelopmentCardType.Knight);
        gs.Phase.CurrentPlayer = bot;

        GamePlayHelpers.PlayKnightDevCard(gs, bot, gs.GetTileAt(2, 0));

        human.AssignDevelopmentCard(DevelopmentCardType.Knight);
        human.AssignDevelopmentCard(DevelopmentCardType.Knight);
        human.MakeNewDevelopmentCardsPlayable();
        human.PlayDevelopmentCard(DevelopmentCardType.Knight);
        human.PlayDevelopmentCard(DevelopmentCardType.Knight);
        gs.Phase.CurrentPlayer = human;
        gs.Phase.ClearDevCardPlayState();

        return gs;
    }

    [Fact]
    public void PlayKnightDevCard_SomeoneAlreadyHasLargestArmy()
    {
        var gs = CreateGameWhereBotLargestArmy();
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        Assert.NotNull(gs.PlayerWithLargestArmy);
        Assert.Equal(gs.PlayerWithLargestArmy.Id, bot.Id);

        GamePlayHelpers.PlayKnightDevCard(gs, human, gs.GetTileAt(0, 0));

        Assert.NotNull(gs.PlayerWithLargestArmy);
        Assert.Equal(gs.PlayerWithLargestArmy.Id, bot.Id);
    }

    [Fact]
    public void PlayKnightDevCard_TakeOverLargestArmy()
    {
        var gs = CreateGameWhereBotLargestArmy();
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        human.AssignDevelopmentCard(DevelopmentCardType.Knight);
        human.MakeNewDevelopmentCardsPlayable();
        human.PlayDevelopmentCard(DevelopmentCardType.Knight);
        Assert.Equal(3, bot.FullVictoryPoints);
        Assert.Equal(0, human.FullVictoryPoints);

        GamePlayHelpers.PlayKnightDevCard(gs, human, gs.GetTileAt(0, 0));

        Assert.NotNull(gs.PlayerWithLargestArmy);
        Assert.Equal(gs.PlayerWithLargestArmy.Id, human.Id);
        Assert.Equal(1, bot.FullVictoryPoints);
        Assert.Equal(3, human.FullVictoryPoints);

        Assert.True(gs.EventRecord.Count(e => e.Action == EventRecordAction.GainedLargestArmy) >= 2);
        var largestArmyEvent = gs.EventRecord.LastOrDefault(e => e.Action == EventRecordAction.GainedLargestArmy);
        Assert.NotNull(largestArmyEvent);
        Assert.Equal(largestArmyEvent.PlayerId, human.Id);
    }

    [Fact]
    public void PlaceSecondRoad_OnFirstPlaceRoad_Invalid()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard(true);
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E8).BuildRoad(board.GetBluePlayer());
        board.GetVertex(TestVertex.V4).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E10).BuildRoad(board.GetRedPlayer());
        board.GetVertex(TestVertex.V6).BuildSettlement(board.GetRedPlayer());

        board.GetGameState().Phase.CurrentPlayer = board.GetRedPlayer();
        board.GetGameState().Phase.EndPlayer = board.GetBluePlayer();
        board.GetGameState().Phase.PhaseState = GameStates.PlaceSecondRoad;

        // Act
        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, board.GetEdge(TestEdge.E22).Id);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(1042, response.ErrorCode);
    }

    [Fact]
    public void FindVertexWithoutRoads_OnlyOneSettelment_HasNoRoad()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());

        // Act
        var vertex = GamePlayHelpers.FindSettlementWithNoRoads(board.GetGameState(), board.GetRedPlayer());

        // Assert
        Assert.NotNull(vertex);
        Assert.NotEmpty(vertex.Edges);
        Assert.Equal(board.GetVertex(TestVertex.V10).Id, vertex.Id);
        Assert.True(vertex.Edges.All(e => e.Owner == null));
    }

    [Fact]
    public void FindVertexWithoutRoads_OnlyOneSettelment_HasRoad()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E8).BuildRoad(board.GetRedPlayer());
        board.GetVertex(TestVertex.V5).BuildSettlement(board.GetBluePlayer()); // Opponents settlment

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.FindSettlementWithNoRoads(board.GetGameState(), board.GetRedPlayer()));
    }

    [Fact]
    public void FindVertexWithoutRoads_TwoSettelments_OneWithoutRoad()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E8).BuildRoad(board.GetRedPlayer());
        board.GetVertex(TestVertex.V5).BuildSettlement(board.GetBluePlayer()); // Opponents settlment
        board.GetEdge(TestEdge.E5).BuildRoad(board.GetBluePlayer()); // Oppenents settlement
        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetRedPlayer());

        // Act
        var vertex = GamePlayHelpers.FindSettlementWithNoRoads(board.GetGameState(), board.GetRedPlayer());

        // Assert
        Assert.NotNull(vertex);
        Assert.NotEmpty(vertex.Edges);
        Assert.Equal(board.GetVertex(TestVertex.V3).Id, vertex.Id);
        Assert.True(vertex.Edges.All(e => e.Owner == null));    
    }

    [Fact]
    public void FindVertexWithoutRoads_TwoSettelments_BothHaveRoad()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E8).BuildRoad(board.GetRedPlayer());
        board.GetVertex(TestVertex.V5).BuildSettlement(board.GetBluePlayer()); // Opponents settlment
        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetRedPlayer()); 

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.FindSettlementWithNoRoads(board.GetGameState(), board.GetRedPlayer()));
    }

    [Fact]
    public void FindVertexWithoutRoads_TwoSettelmentsWithoutRoad_Exception()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V5).BuildSettlement(board.GetBluePlayer()); // Opponents settlment
        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetRedPlayer());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.FindSettlementWithNoRoads(board.GetGameState(), board.GetRedPlayer()));
    }

    private TestGameBoard CreateBoardWithBlueHavingLength4Road()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E8).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E15).BuildRoad(board.GetBluePlayer());

        return board;        
    }

    [Fact]
    public void BuildRoad_GainLongestRoad()
    {
        var board = CreateBoardWithBlueHavingLength4Road();
        Assert.Null(board.GetGameState().PlayerWithLongestRoad);
        Assert.Equal(4, board.GetGameState().GetLongestRoadLength(board.GetBluePlayer()));
        Assert.Equal(1, board.GetBluePlayer().FullVictoryPoints);

        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetBluePlayer(), board.GetEdge(TestEdge.E10));

        Assert.NotNull(board.GetGameState().PlayerWithLongestRoad);
        Assert.Equal(board.GetBluePlayer().Id, board.GetGameState().PlayerWithLongestRoad.Id);
        Assert.Equal(5, board.GetGameState().GetLongestRoadLength(board.GetBluePlayer()));
        Assert.Equal(5, board.GetBluePlayer().LongRoadLength);
        Assert.Equal(3, board.GetBluePlayer().FullVictoryPoints);

        var longestRoadEvent = board.GetGameState().EventRecord.FirstOrDefault(e => e.Action == EventRecordAction.GainedLongestRoad);
        Assert.NotNull(longestRoadEvent);
        Assert.Equal(board.GetBluePlayer().Id, longestRoadEvent.PlayerId);
    }

    private TestGameBoard CreateBoardWithBlueLength5RoadRedLength4()
    {
        var board = CreateBoardWithBlueHavingLength4Road();
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetBluePlayer(), board.GetEdge(TestEdge.E10));
        
        board.GetEdge(TestEdge.E5).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E6).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E7).BuildRoad(board.GetRedPlayer());

        return board;        
    }

    [Fact]
    public void BuildRoad_OtherAlreadyHasLongestRoad()
    {
        var board = CreateBoardWithBlueLength5RoadRedLength4();
        Assert.NotNull(board.GetGameState().PlayerWithLongestRoad);
        Assert.Equal(board.GetBluePlayer().Id, board.GetGameState().PlayerWithLongestRoad.Id);

        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E30));

        Assert.Equal(5, board.GetGameState().GetLongestRoadLength(board.GetRedPlayer()));
        Assert.Equal(5, board.GetBluePlayer().LongRoadLength);
        Assert.NotNull(board.GetGameState().PlayerWithLongestRoad);
        Assert.Equal(board.GetBluePlayer().Id, board.GetGameState().PlayerWithLongestRoad.Id);
    }

    [Fact]
    public void BuildRoad_BuildLongerRoadToGainLongest()
    {
        var board = CreateBoardWithBlueLength5RoadRedLength4();
        Assert.NotNull(board.GetGameState().PlayerWithLongestRoad);
        Assert.Equal(board.GetBluePlayer().Id, board.GetGameState().PlayerWithLongestRoad.Id);
        Assert.Equal(3, board.GetBluePlayer().FullVictoryPoints);
        Assert.Equal(1, board.GetRedPlayer().FullVictoryPoints);
        

        board.GetEdge(TestEdge.E24).BuildRoad(board.GetRedPlayer());
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E30));

        Assert.Equal(6, board.GetGameState().GetLongestRoadLength(board.GetRedPlayer()));
        Assert.Equal(6, board.GetRedPlayer().LongRoadLength);
        Assert.NotNull(board.GetGameState().PlayerWithLongestRoad);
        Assert.Equal(board.GetRedPlayer().Id, board.GetGameState().PlayerWithLongestRoad.Id);
        Assert.Equal(1, board.GetBluePlayer().FullVictoryPoints);
        Assert.Equal(3, board.GetRedPlayer().FullVictoryPoints);

        Assert.True(board.GetGameState().EventRecord.Count(e => e.Action == EventRecordAction.GainedLongestRoad) >= 2);
        var longestRoadEvent = board.GetGameState().EventRecord.LastOrDefault(e => e.Action == EventRecordAction.GainedLongestRoad);
        Assert.NotNull(longestRoadEvent);
        Assert.Equal(board.GetRedPlayer().Id, longestRoadEvent.PlayerId);

    }

    [Fact]
    public void DiscardCardRequestFromUser_InvalidState()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.SettingUpBoard, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetRedPlayer().AssignResources(ResourceType.Brick, 8);
        var cardsToDiscard = new List<ResourceType>();
        for (int i = 0; i < 4; i++)
            cardsToDiscard.Add(ResourceType.Brick);

        var response = GamePlayHelpers.DiscardCardRequestFromUser(gs, new DiscardRequest(board.GetRedPlayer().Id, cardsToDiscard));

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
    }

    [Fact]
    public void DiscardCardRequestFromUser_NotUsersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.DiscardCards, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetBluePlayer().AssignResources(ResourceType.Brick, 8);
        var cardsToDiscard = new List<ResourceType>();
        for (int i = 0; i < 4; i++)
            cardsToDiscard.Add(ResourceType.Brick);

        var response = GamePlayHelpers.DiscardCardRequestFromUser(gs, new DiscardRequest(board.GetBluePlayer().Id, cardsToDiscard));

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
    }


    [Fact]
    public void DiscardCardRequestFromUser_DiscardingTooFew()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.DiscardCards, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetRedPlayer().AssignResources(ResourceType.Brick, 8);
        var cardsToDiscard = new List<ResourceType>();
        for (int i = 0; i < 3; i++)
            cardsToDiscard.Add(ResourceType.Brick);

        var response = GamePlayHelpers.DiscardCardRequestFromUser(gs, new DiscardRequest(board.GetRedPlayer().Id, cardsToDiscard));

        Assert.False(response.Success);
        Assert.Equal(1045, response.ErrorCode);
    }

    [Fact]
    public void DiscardCardRequestFromUser_DiscardingTooMany()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.DiscardCards, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetRedPlayer().AssignResources(ResourceType.Brick, 9);
        var cardsToDiscard = new List<ResourceType>();
        for (int i = 0; i < 5; i++)
            cardsToDiscard.Add(ResourceType.Brick);

        var response = GamePlayHelpers.DiscardCardRequestFromUser(gs, new DiscardRequest(board.GetRedPlayer().Id, cardsToDiscard));

        Assert.False(response.Success);
        Assert.Equal(1045, response.ErrorCode);
    }

    [Fact]
    public void DiscardCardRequestFromUser_DoNotHaveSelectedResources()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.DiscardCards, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetRedPlayer().AssignResources(ResourceType.Brick, 3);
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 5);
        var cardsToDiscard = new List<ResourceType>();
        for (int i = 0; i < 4; i++)
            cardsToDiscard.Add(ResourceType.Brick);

        var response = GamePlayHelpers.DiscardCardRequestFromUser(gs, new DiscardRequest(board.GetRedPlayer().Id, cardsToDiscard));

        Assert.False(response.Success);
        Assert.Equal(1017, response.ErrorCode);
    }

    [Fact]
    public void DiscardCardRequestFromUser_Valid()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.DiscardCards, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetRedPlayer().AssignResources(ResourceType.Brick, 4);
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 5);
        var cardsToDiscard = new List<ResourceType>();
        for (int i = 0; i < 4; i++)
            cardsToDiscard.Add(ResourceType.Brick);

        var response = GamePlayHelpers.DiscardCardRequestFromUser(gs, new DiscardRequest(board.GetRedPlayer().Id, cardsToDiscard));

        Assert.True(response.Success);
        Assert.Contains(ResourceType.Brick, board.GetRedPlayer().Resources);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Contains(ResourceType.Wood, board.GetRedPlayer().Resources);
        Assert.Equal(5, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.NotNull(response.PossibleActions);
        Assert.Single(response.PossibleActions);
        Assert.Contains(response.PossibleActions, a => a.Action == PlayerAction.PlaceRobber);
        Assert.Equal(23, gs.GetBankResourceCount(ResourceType.Brick));
    }

    [Fact]
    public void DiscardCards_InvalidState()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.SettingUpBoard, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetRedPlayer().AssignResources(ResourceType.Brick, 8);
        var cardsToDiscard = new List<ResourceType>();
        for (int i = 0; i < 4; i++)
            cardsToDiscard.Add(ResourceType.Brick);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.DiscardCards(gs, board.GetRedPlayer(), cardsToDiscard));
    }

    [Fact]
    public void DiscardCards_NotUsersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.DiscardCards, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetBluePlayer().AssignResources(ResourceType.Brick, 8);
        var cardsToDiscard = new List<ResourceType>();
        for (int i = 0; i < 4; i++)
            cardsToDiscard.Add(ResourceType.Brick);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.DiscardCards(gs, board.GetBluePlayer(), cardsToDiscard));
    }

    [Fact]
    public void DiscardCards_DiscardingTooFew()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.DiscardCards, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetRedPlayer().AssignResources(ResourceType.Brick, 8);
        var cardsToDiscard = new List<ResourceType>();
        for (int i = 0; i < 3; i++)
            cardsToDiscard.Add(ResourceType.Brick);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.DiscardCards(gs, board.GetRedPlayer(), cardsToDiscard));
    }

    [Fact]
    public void DiscardCards_DoNotHaveSelectedResources()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.DiscardCards, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetRedPlayer().AssignResources(ResourceType.Brick, 3);
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 5);
        var cardsToDiscard = new List<ResourceType>();
        for (int i = 0; i < 4; i++)
            cardsToDiscard.Add(ResourceType.Brick);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.DiscardCards(gs, board.GetRedPlayer(), cardsToDiscard));
    }

    [Fact]
    public void DiscardCards_UserDoesntNeedToDiscard()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.DiscardCards, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetRedPlayer().AssignResources(ResourceType.Brick, 7);
        var cardsToDiscard = new List<ResourceType>();
        for (int i = 0; i < 3; i++)
            cardsToDiscard.Add(ResourceType.Brick);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.DiscardCards(gs, board.GetRedPlayer(), cardsToDiscard));
    }

    [Fact]
    public void DiscardCardRequest_Valid()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.DiscardCards, board.GetRedPlayer(), board.GetBluePlayer());

        board.GetRedPlayer().AssignResources(ResourceType.Brick, 4);
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 5);
        var cardsToDiscard = new List<ResourceType>() { ResourceType.Brick, ResourceType.Brick, ResourceType.Wood, ResourceType.Wood };

        GamePlayHelpers.DiscardCards(gs, board.GetRedPlayer(), cardsToDiscard);

        Assert.Contains(ResourceType.Brick, board.GetRedPlayer().Resources);
        Assert.Equal(2, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Contains(ResourceType.Wood, board.GetRedPlayer().Resources);
        Assert.Equal(3, board.GetRedPlayer().Resources[ResourceType.Wood]);
        Assert.Equal(5, board.GetRedPlayer().ResourceCount);
    }

    [Fact]
    public void OpenTradeFromUser_InvalidState()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        var request = CreateTradeRequestDTO(board.GetRedPlayer(), ResourceType.Brick, 1, ResourceType.Grain, 1);

        var response = GamePlayHelpers.OpenTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void OpenTradeFromUser_NotPlayersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = CreateTradeRequestDTO(board.GetBluePlayer(), ResourceType.Brick, 1, ResourceType.Grain, 1);

        var response = GamePlayHelpers.OpenTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void OpenTradeFromUser_MissingOffer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new TradeRequestDTO(board.GetRedPlayer().Id, null!, new Dictionary<ResourceType, int>() {{ResourceType.Grain, 1}});

        var response = GamePlayHelpers.OpenTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1048, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void OpenTradeFromUser_MissingRequest()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new TradeRequestDTO(board.GetRedPlayer().Id, new Dictionary<ResourceType, int>() {{ResourceType.Grain, 1}}, null!);

        var response = GamePlayHelpers.OpenTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1048, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void OpenTradeFromUser_EmptyOffer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new TradeRequestDTO(board.GetRedPlayer().Id, new Dictionary<ResourceType, int>(), new Dictionary<ResourceType, int>() {{ResourceType.Grain, 1}});

        var response = GamePlayHelpers.OpenTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1048, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void OpenTradeFromUser_EmptyRequest()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new TradeRequestDTO(board.GetRedPlayer().Id, new Dictionary<ResourceType, int>() {{ResourceType.Grain, 1}}, new Dictionary<ResourceType, int>());

        var response = GamePlayHelpers.OpenTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1048, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void OpenTradeFromUser_OfferMatchesRequest()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = CreateTradeRequestDTO(board.GetRedPlayer(), ResourceType.Brick, 3, ResourceType.Brick, 3);

        var response = GamePlayHelpers.OpenTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1049, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void OpenTradeFromUser_DontHaveCardsOffered()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetRedPlayer().AssignResources(ResourceType.Ore, 2);
        board.GetRedPlayer().AssignResources(ResourceType.Grain, 1);

        var request = CreateTradeRequestDTO(board.GetRedPlayer(), ResourceType.Brick, 1, ResourceType.Wood, 1);

        var response = GamePlayHelpers.OpenTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1006, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void OpenTradeFromUser_Valid()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetRedPlayer().AssignResources(ResourceType.Ore, 4);
        board.GetRedPlayer().AssignResources(ResourceType.Grain, 1);

        var request = CreateTradeRequestDTO(board.GetRedPlayer(), ResourceType.Ore, 1, ResourceType.Grain, 1);

        var response = GamePlayHelpers.OpenTradeFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.NotNull(gs.Phase.PendingTradeResponses);
        Assert.NotEmpty(gs.Phase.PendingTradeResponses);
        Assert.Equal(1, gs.Phase.PendingTradeResponses.Count(t => t.ResponseType == TradeResponseType.Original));
        var storedTrade = gs.Phase.PendingTradeResponses.First(t => t.ResponseType == TradeResponseType.Original);
        Assert.Equal(board.GetRedPlayer().Id, storedTrade.Player.Id);
        Assert.NotNull(storedTrade.Offer);
        Assert.Single(storedTrade.Offer);
        Assert.Contains(ResourceType.Ore, storedTrade.Offer);
        Assert.Equal(1, storedTrade.Offer[ResourceType.Ore]);
        Assert.NotNull(storedTrade.Request);
        Assert.Single(storedTrade.Request);
        Assert.Contains(ResourceType.Grain, storedTrade.Request);
        Assert.Equal(1, storedTrade.Request[ResourceType.Grain]);
        Assert.NotNull(response.PossibleActions);
        Assert.Single(response.PossibleActions);
        Assert.Contains(response.PossibleActions, a => a.Action == PlayerAction.RejectAllOffers);
    }

    [Fact]
    public void OpenTrade_NotPlayersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = CreateTradeRequestDTO(board.GetBluePlayer(), ResourceType.Brick, 1, ResourceType.Grain, 1);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.OpenTrade(gs, board.GetBluePlayer(), request.Offer, request.Request));
    }

    [Fact]
    public void OpenTrade_MissingOffer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new TradeRequestDTO(board.GetRedPlayer().Id, null!, new Dictionary<ResourceType, int>() {{ResourceType.Grain, 1}});

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.OpenTrade(gs, board.GetRedPlayer(), request.Offer, request.Request));
    }

    [Fact]
    public void OpenTrade_MissingRequest()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new TradeRequestDTO(board.GetRedPlayer().Id, new Dictionary<ResourceType, int>() {{ResourceType.Grain, 1}}, null!);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.OpenTrade(gs, board.GetRedPlayer(), request.Offer, request.Request));
    }

    private TradeResponseDTO CreateTradeResponseDTO(Player player, TradeResponseType responseType,
        ResourceType offerType, int offerCount, ResourceType requestType, int requestCount)
    {
        return new TradeResponseDTO(player.Id, responseType, new Dictionary<ResourceType, int>() { {offerType, offerCount}}, new Dictionary<ResourceType, int>() { {requestType, requestCount}});
    }

    [Fact]
    public void RespondToTradeFromUser_InvalidState()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var tradeResponse = new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Accept, null!, null!);

        var response = GamePlayHelpers.RespondToTradeFromUser(gs, tradeResponse);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RespondToTradeFromUser_CantOfferOnYourOwnTrade()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var tradeResponse = new TradeResponseDTO(board.GetRedPlayer().Id, TradeResponseType.Accept, null!, null!);

        var response = GamePlayHelpers.RespondToTradeFromUser(gs, tradeResponse);

        Assert.False(response.Success);
        Assert.Equal(1050, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RespondToTradeFromUser_CantUseOriginalType()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var tradeResponse = new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Original, null!, null!);

        var response = GamePlayHelpers.RespondToTradeFromUser(gs, tradeResponse);

        Assert.False(response.Success);
        Assert.Equal(1051, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RespondToTradeFromUser_CounterWithNoOffer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var tradeResponse = new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Counter, null!, new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}});

        var response = GamePlayHelpers.RespondToTradeFromUser(gs, tradeResponse);

        Assert.False(response.Success);
        Assert.Equal(1052, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RespondToTradeFromUser_CounterWithEmptyOffer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var tradeResponse = new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Counter, new Dictionary<ResourceType, int>(), new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}});

        var response = GamePlayHelpers.RespondToTradeFromUser(gs, tradeResponse);

        Assert.False(response.Success);
        Assert.Equal(1052, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RespondToTradeFromUser_CounterWithNoRequest()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var tradeResponse = new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Counter, new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}}, null!);

        var response = GamePlayHelpers.RespondToTradeFromUser(gs, tradeResponse);

        Assert.False(response.Success);
        Assert.Equal(1052, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RespondToTradeFromUser_CounterWithEmptyRequest()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var tradeResponse = new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Counter, new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}}, new Dictionary<ResourceType, int>());

        var response = GamePlayHelpers.RespondToTradeFromUser(gs, tradeResponse);

        Assert.False(response.Success);
        Assert.Equal(1052, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RespondToTradeFromUser_CounterOfferMatchesRequest()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var tradeResponse = CreateTradeResponseDTO(board.GetBluePlayer(), TradeResponseType.Counter, ResourceType.Wool, 2, ResourceType.Wool, 2);

        var response = GamePlayHelpers.RespondToTradeFromUser(gs, tradeResponse);

        Assert.False(response.Success);
        Assert.Equal(1049, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RespondToTradeFromUser_CounterDoesntHaveOfferedCards()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetBluePlayer().AssignResources(ResourceType.Wool, 1);
        board.GetBluePlayer().AssignResources(ResourceType.Wood, 1);

        var tradeResponse = CreateTradeResponseDTO(board.GetBluePlayer(), TradeResponseType.Counter, ResourceType.Wool, 2, ResourceType.Ore, 1);

        var response = GamePlayHelpers.RespondToTradeFromUser(gs, tradeResponse);

        Assert.False(response.Success);
        Assert.Equal(1006, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RespondToTradeFromUser_DoesntHaveOfferedCards()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.Phase.AddPendingTradeResponse(new TradeResponse(board.GetRedPlayer(), TradeResponseType.Original, 
            new Dictionary<ResourceType, int>() {{ResourceType.Ore, 1}}, 
            new Dictionary<ResourceType, int>() {{ResourceType.Grain, 1}}));
        board.GetBluePlayer().AssignResources(ResourceType.Wool, 1);
        board.GetBluePlayer().AssignResources(ResourceType.Wood, 1);

        var tradeResponse = new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Accept, new Dictionary<ResourceType, int>(), new Dictionary<ResourceType, int>());

        var response = GamePlayHelpers.RespondToTradeFromUser(gs, tradeResponse);

        Assert.False(response.Success);
        Assert.Equal(1017, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RespondToTradeFromUser_Valid()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 1);
        GamePlayHelpers.OpenTrade(gs, board.GetRedPlayer(), new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}}, new Dictionary<ResourceType, int>() {{ResourceType.Grain, 1}});
        gs.Phase.PhaseState = GameStates.RespondToTrade;
        board.GetBluePlayer().AssignResources(ResourceType.Wool, 1);
        board.GetBluePlayer().AssignResources(ResourceType.Wood, 1);

        var tradeResponse = CreateTradeResponseDTO(board.GetBluePlayer(), TradeResponseType.Counter, ResourceType.Wool, 1, ResourceType.Ore, 1);

        var response = GamePlayHelpers.RespondToTradeFromUser(gs, tradeResponse);

        Assert.True(response.Success);
        Assert.NotNull(gs);
        Assert.NotNull(gs.Phase.PendingTradeResponses);
        Assert.Single(gs.Phase.PendingTradeResponses.Where(r => r.ResponseType != TradeResponseType.Original));
        var pendingTradeResponse = gs.Phase.PendingTradeResponses.First(r => r.ResponseType != TradeResponseType.Original);
        Assert.Equal(board.GetBluePlayer().Id, pendingTradeResponse.Player.Id);
        Assert.Equal(TradeResponseType.Counter, pendingTradeResponse.ResponseType);
        Assert.NotNull(pendingTradeResponse.Offer);
        Assert.Single(pendingTradeResponse.Offer);
        Assert.True(pendingTradeResponse.Offer.ContainsKey(ResourceType.Wool));
        Assert.Equal(1, pendingTradeResponse.Offer[ResourceType.Wool]);
        Assert.NotNull(pendingTradeResponse.Request);
        Assert.Single(pendingTradeResponse.Request);
        Assert.True(pendingTradeResponse.Request.ContainsKey(ResourceType.Ore));
        Assert.Equal(1, pendingTradeResponse.Request[ResourceType.Ore]);
        Assert.NotNull(response.PossibleActions);
        Assert.Contains(response.PossibleActions, a => a.Action == PlayerAction.AcceptTrade);
        Assert.Contains(response.PossibleActions, a => a.Action == PlayerAction.RejectAllOffers);
     }

    [Fact]
    public void RespondToTrade_CantUseOriginalType()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var tradeResponse = new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Original, null!, null!);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.RespondToTrade(gs, board.GetBluePlayer(), tradeResponse));
    }

    [Fact]
    public void RespondToTrade_CounterWithNoOffer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var tradeResponse = new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Counter, null!, new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}});

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.RespondToTrade(gs, board.GetBluePlayer(), tradeResponse));
    }

    [Fact]
    public void RespondToTrade_CounterWithEmptyOffer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var tradeResponse = new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Counter, new Dictionary<ResourceType, int>(), new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}});

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.RespondToTrade(gs, board.GetBluePlayer(), tradeResponse));
    }

    [Fact]
    public void RespondToTrade_DoesntHaveOfferedCards()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 1);
        GamePlayHelpers.OpenTrade(gs, board.GetRedPlayer(), new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}}, new Dictionary<ResourceType, int>() {{ResourceType.Grain, 1}});
        gs.Phase.PhaseState = GameStates.RespondToTrade;
        board.GetBluePlayer().AssignResources(ResourceType.Wool, 1);
        board.GetBluePlayer().AssignResources(ResourceType.Wood, 1);

        var tradeResponse = CreateTradeResponseDTO(board.GetBluePlayer(), TradeResponseType.Counter, ResourceType.Wool, 2, ResourceType.Ore, 1);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.RespondToTrade(gs, board.GetBluePlayer(), tradeResponse));
    }

    private TestGameBoard CreateTestBoardForTradeAcceptanceTesting()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetRedPlayer().AssignResources(ResourceType.Wool, 1);
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 1);
        GamePlayHelpers.OpenTrade(gs, board.GetRedPlayer(), new Dictionary<ResourceType, int>() {{ResourceType.Wool, 1}}, new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}});
        gs.Phase.PhaseState = GameStates.RespondToTrade;
        board.GetBluePlayer().AssignResources(ResourceType.Brick, 1);
        board.GetBluePlayer().AssignResources(ResourceType.Grain, 1);

        return board;        
    }

    [Fact]
    public void AcceptOfferFromUser_InvalidState()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new AcceptTradeDTO(board.GetRedPlayer().Id, board.GetBluePlayer().Id);

        var response = GamePlayHelpers.AcceptTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AcceptOfferFromUser_NotPlayersTurn()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        var request = new AcceptTradeDTO(board.GetBluePlayer().Id, board.GetRedPlayer().Id);

        var response = GamePlayHelpers.AcceptTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AcceptOfferFromUser_CantAcceptOwnTrade()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        var request = new AcceptTradeDTO(board.GetRedPlayer().Id, board.GetRedPlayer().Id);

        var response = GamePlayHelpers.AcceptTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1053, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AcceptOfferFromUser_AcceptedPlayerIdIsInvalid()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        var request = new AcceptTradeDTO(board.GetRedPlayer().Id, "PP1");

        var response = GamePlayHelpers.AcceptTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1056, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AcceptOfferFromUser_PlayerDidntRespond()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        var request = new AcceptTradeDTO(board.GetRedPlayer().Id, board.GetBluePlayer().Id);

        var response = GamePlayHelpers.AcceptTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1054, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AcceptOfferFromUser_PlayerRejectedTrade()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        GamePlayHelpers.RespondToTrade(gs, board.GetBluePlayer(), new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Reject, null!, null!));
        var request = new AcceptTradeDTO(board.GetRedPlayer().Id, board.GetBluePlayer().Id);

        var response = GamePlayHelpers.AcceptTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1055, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AcceptOfferFromUser_PlayerDoesntHaveCounterRequest()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        // Counter with a request for ore, which player doesn't have
        GamePlayHelpers.RespondToTrade(gs, board.GetBluePlayer(), new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Counter, 
            new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}}, new Dictionary<ResourceType, int>() {{ResourceType.Ore, 1}}));
        var request = new AcceptTradeDTO(board.GetRedPlayer().Id, board.GetBluePlayer().Id);

        var response = GamePlayHelpers.AcceptTradeFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1017, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AcceptOfferFromUser_Valid()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        GamePlayHelpers.RespondToTrade(gs, board.GetBluePlayer(), new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Accept, null!, null!));
        var request = new AcceptTradeDTO(board.GetRedPlayer().Id, board.GetBluePlayer().Id);

        var response = GamePlayHelpers.AcceptTradeFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.NotNull(response.GameState.Phase);
        Assert.Equal(GameStates.BuildOrTrade, response.GameState.Phase.PhaseState);
        Assert.Equal(board.GetRedPlayer().Id, response.GameState.Phase.CurrentPlayerId);
        Assert.Null(response.GameState.Phase.PendingTradeResponses);
        Assert.Equal(1, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Equal(0, board.GetRedPlayer().Resources[ResourceType.Wool]);
        Assert.Equal(1, board.GetBluePlayer().Resources[ResourceType.Wool]);
        Assert.Equal(0, board.GetBluePlayer().Resources[ResourceType.Brick]);
        Assert.NotNull(response.PossibleActions);
        Assert.NotEmpty(response.PossibleActions);
    }

    [Fact]
    public void AcceptOffer_NotPlayersTurn()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        var request = new AcceptTradeDTO(board.GetBluePlayer().Id, board.GetRedPlayer().Id);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.AcceptTrade(gs, board.GetBluePlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void AcceptOffer_CantAcceptOwnTrade()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        var request = new AcceptTradeDTO(board.GetRedPlayer().Id, board.GetRedPlayer().Id);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.AcceptTrade(gs, board.GetRedPlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void AcceptOffer_PlayerDidntRespond()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        var request = new AcceptTradeDTO(board.GetRedPlayer().Id, board.GetBluePlayer().Id);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.AcceptTrade(gs, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void AcceptOffer_PlayerRejectedTrade()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        GamePlayHelpers.RespondToTrade(gs, board.GetBluePlayer(), new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Reject, null!, null!));
        var request = new AcceptTradeDTO(board.GetRedPlayer().Id, board.GetBluePlayer().Id);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.AcceptTrade(gs, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void AcceptOffer_PlayerDoesntHaveCounterRequest()
    {
        var board = CreateTestBoardForTradeAcceptanceTesting();
        var gs = board.GetGameState();
        // Counter with a request for ore, which player doesn't have
        GamePlayHelpers.RespondToTrade(gs, board.GetBluePlayer(), new TradeResponseDTO(board.GetBluePlayer().Id, TradeResponseType.Counter, 
            new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}}, new Dictionary<ResourceType, int>() {{ResourceType.Ore, 1}}));
        var request = new AcceptTradeDTO(board.GetRedPlayer().Id, board.GetBluePlayer().Id);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.AcceptTrade(gs, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void RejectAllOffersFromUser_InvalidState()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new BaseRequest(board.GetRedPlayer().Id);

        var response = GamePlayHelpers.RejectAllOffersFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RejectAllOffersFromUser_NotPlayersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new BaseRequest(board.GetBluePlayer().Id);

        var response = GamePlayHelpers.RejectAllOffersFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void RejectAllOffersFromUser_Valid()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new BaseRequest(board.GetRedPlayer().Id);

        var response = GamePlayHelpers.RejectAllOffersFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.Null(gs.Phase.PendingTradeResponses);
        Assert.NotNull(response.PossibleActions);
        Assert.NotEmpty(response.PossibleActions);
    }

    [Fact]
    public void RejectAllOffers_InvalidState()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new BaseRequest(board.GetRedPlayer().Id);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.RejectAllOffers(gs, board.GetRedPlayer()));
    }

    [Fact] 
    public void SelectTargetFromUser_InvalidState()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new SelectTargetRequest(board.GetRedPlayer().Id, board.GetBluePlayer().Id);

        var response = GamePlayHelpers.SelectTargetFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact] 
    public void SelectTargetFromUser_NotPlayersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.SelectTarget, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new SelectTargetRequest(board.GetBluePlayer().Id, board.GetBluePlayer().Id);

        var response = GamePlayHelpers.SelectTargetFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void SelectTargetFromUser_NotValidPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        var gs = board.GetGameState();
        var orangePlayer = Player.CreateTestPlayer("Tim", PlayerColor.Orange);
        gs.Phase = new GamePhase(GameStates.SelectTarget, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new SelectTargetRequest(board.GetRedPlayer().Id, orangePlayer.Id);

        var response = GamePlayHelpers.SelectTargetFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1012, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact] 
    public void SelectTargetFromUser_NotValidTarget()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        var gs = board.GetGameState();
        var orangePlayer = Player.CreateTestPlayer("Tim", PlayerColor.Orange);
        gs.Players.Add(orangePlayer);
        gs.Phase = new GamePhase(GameStates.SelectTarget, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new SelectTargetRequest(board.GetRedPlayer().Id, orangePlayer.Id);

        var response = GamePlayHelpers.SelectTargetFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1064, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact] 
    public void SelectTargetFromUser_Valid()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        var gs = board.GetGameState();
        var orangePlayer = Player.CreateTestPlayer("Tim", PlayerColor.Orange);
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 1);
        gs.Players.Add(orangePlayer);
        board.GetVertex(TestVertex.V1).BuildSettlement(orangePlayer);
        orangePlayer.AssignResources(ResourceType.Brick, 2);
        gs.Phase = new GamePhase(GameStates.SelectTarget, board.GetRedPlayer(), board.GetBluePlayer());
        gs.SetRobberTile(board.GetTile(TestTile.T0));
        gs.Phase.SetStateToReturnTo(GameStates.BuildOrTrade, board.GetTile(TestTile.T6));
        gs.Phase.SetTargetPlayers(new List<Player>() {board.GetBluePlayer(), orangePlayer});

        var request = new SelectTargetRequest(board.GetRedPlayer().Id, orangePlayer.Id);

        var response = GamePlayHelpers.SelectTargetFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.NotNull(response.GameState.Phase);
        Assert.Equal(GameStates.BuildOrTrade, response.GameState.Phase.PhaseState);
        Assert.Null(response.GameState.Phase.TargetPlayerIds);
        Assert.Contains(ResourceType.Brick, board.GetRedPlayer().Resources);
        Assert.Equal(2, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Contains(ResourceType.Brick, orangePlayer.Resources);
        Assert.Equal(1, orangePlayer.Resources[ResourceType.Brick]);
    }

    [Fact] 
    public void SelectTarget_NotValidTarget()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        var gs = board.GetGameState();
        var orangePlayer = Player.CreateTestPlayer("Tim", PlayerColor.Orange);
        gs.Players.Add(orangePlayer);
        gs.Phase = new GamePhase(GameStates.SelectTarget, board.GetRedPlayer(), board.GetBluePlayer());
        var request = new SelectTargetRequest(board.GetRedPlayer().Id, orangePlayer.Id);

        Assert.Throws<InvalidOperationException>( () => GamePlayHelpers.SelectTarget(gs, board.GetRedPlayer(), orangePlayer));
    }

    [Fact] 
    public void SelectTarget_Valid()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        var gs = board.GetGameState();
        var orangePlayer = Player.CreateTestPlayer("Tim", PlayerColor.Orange);
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 1);
        gs.Players.Add(orangePlayer);
        board.GetVertex(TestVertex.V1).BuildSettlement(orangePlayer);
        orangePlayer.AssignResources(ResourceType.Brick, 2);
        gs.Phase = new GamePhase(GameStates.SelectTarget, board.GetRedPlayer(), board.GetBluePlayer());
        gs.SetRobberTile(board.GetTile(TestTile.T0));
        gs.Phase.SetStateToReturnTo(GameStates.BuildOrTrade, board.GetTile(TestTile.T6));
        gs.Phase.SetTargetPlayers(new List<Player>() {board.GetBluePlayer(), orangePlayer});

        GamePlayHelpers.SelectTarget(gs, board.GetRedPlayer(), orangePlayer);

        Assert.Contains(ResourceType.Brick, board.GetRedPlayer().Resources);
        Assert.Equal(2, board.GetRedPlayer().Resources[ResourceType.Brick]);
        Assert.Contains(ResourceType.Brick, orangePlayer.Resources);
        Assert.Equal(1, orangePlayer.Resources[ResourceType.Brick]);
    }

    [Fact]
    public void AddPlayerToGame_InvalidState()
    {
        var gs = new GameState(new Guid(), "UT");
        gs.Phase = new GamePhase(GameStates.PlaceFirstSettlement);

        var response = GamePlayHelpers.AddPlayerToGame(gs, new AddPlayerRequest {PlayerName = "Henry", PreferredColor = PlayerColor.Orange});

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AddPlayerToGame_AlreadyAtMaxPlayers()
    {
        var gs = new GameState(new Guid(), "UT");
        while (gs.Players.Count < gs.Settings.MaxPlayers)
            gs.Players.Add(Player.CreateTestPlayer("Test", PlayerColor.White));

        var response = GamePlayHelpers.AddPlayerToGame(gs, new AddPlayerRequest {PlayerName = "Henry", PreferredColor = PlayerColor.Orange});

        Assert.False(response.Success);
        Assert.Equal(1060, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AddPlayerToGame_EmptyName()
    {
        var gs = new GameState(new Guid(), "UT");

        var response = GamePlayHelpers.AddPlayerToGame(gs, new AddPlayerRequest {PlayerName = "  ", IsBot = false, PreferredColor = PlayerColor.Orange});

        Assert.False(response.Success);
        Assert.Equal(1061, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AddPlayerToGame_InvalidCharInName()
    {
        var gs = new GameState(new Guid(), "UT");

        var response = GamePlayHelpers.AddPlayerToGame(gs, new AddPlayerRequest {PlayerName = "H<nry", IsBot = false, PreferredColor = PlayerColor.Orange});

        Assert.False(response.Success);
        Assert.Equal(1061, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AddPlayerToGame_NameTooLong()
    {
        var gs = new GameState(new Guid(), "UT");

        var response = GamePlayHelpers.AddPlayerToGame(gs, new AddPlayerRequest {PlayerName = "Michael Bilodeau", IsBot = false, PreferredColor = PlayerColor.Orange});

        Assert.False(response.Success);
        Assert.Equal(1061, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AddPlayerToGame_NameAlreadyUsed()
    {
        var gs = new GameState(new Guid(), "UT");
        gs.Players.Add(Player.CreateTestPlayer("Test", PlayerColor.White));

        var response = GamePlayHelpers.AddPlayerToGame(gs, new AddPlayerRequest {PlayerName = "Test", IsBot = false, PreferredColor = PlayerColor.Orange});

        Assert.False(response.Success);
        Assert.Equal(1058, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AddPlayerToGame_ColorAlreadyUsed()
    {
        var gs = new GameState(new Guid(), "UT");
        gs.Players.Add(Player.CreateTestPlayer("Test", PlayerColor.Orange));

        var response = GamePlayHelpers.AddPlayerToGame(gs, new AddPlayerRequest {PlayerName = "Henry", IsBot = false, PreferredColor = PlayerColor.Orange});

        Assert.False(response.Success);
        Assert.Equal(1059, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void AddPlayerToGame_Bot()
    {
        var gs = new GameState(new Guid(), "UT");
        var countBefore = gs.Players.Count;

        var response = GamePlayHelpers.AddPlayerToGame(gs, new AddPlayerRequest {PreferredColor = PlayerColor.Orange});

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(countBefore + 1, response.GameState.Players.Count);
        var newBot = response.GameState.Players.FirstOrDefault(p => p.Color == PlayerColor.Orange);
        Assert.NotNull(newBot);
        Assert.True(newBot.IsBot);
        Assert.NotEmpty(newBot.Name);
    }

    [Fact]
    public void AddPlayerToGame_Human()
    {
        var gs = new GameState(new Guid(), "UT");
        var countBefore = gs.Players.Count;

        var response = GamePlayHelpers.AddPlayerToGame(gs, new AddPlayerRequest {PlayerName = "Henry", IsBot = false, PreferredColor = PlayerColor.Orange});

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(countBefore + 1, response.GameState.Players.Count);
        var newPlayer = response.GameState.Players.FirstOrDefault(p => p.Name.ToLowerInvariant().Equals("henry"));
        Assert.NotNull(newPlayer);
        Assert.False(newPlayer.IsBot);
        Assert.Equal(PlayerColor.Orange, newPlayer.Color);
    }

    [Fact]
    public void StartGame_NotEnoughPlayers()
    {
        GameState gs = new GameState(new Guid(), "UT");
        Assert.Equal(GameStates.SettingUpBoard, gs.Phase.PhaseState);

        gs.Players.Add(Player.CreateTestPlayer("Tim", PlayerColor.Red));

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.StartGame(gs));
    }

    [Fact]
    public void StartGame_CurrentPlayerSet()
    {
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        Assert.Equal(GameStates.SettingUpBoard, gs.Phase.PhaseState);

        gs.Players.Add(Player.CreateTestPlayer("Tim", PlayerColor.Red));
        gs.Players.Add(Player.CreateTestPlayer("WallE", PlayerColor.Blue, true));

        GamePlayHelpers.StartGame(gs);

        Assert.Equal(GameStates.PlaceFirstSettlement, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.NotNull(gs.RobberTile);
        Assert.NotEmpty(gs.EventRecord);
        Assert.Single(gs.EventRecord, e => e.Action == EventRecordAction.PlaceRobber);
        var initialSetup = gs.EventRecord.FirstOrDefault(e => e.Action == EventRecordAction.InitialSetUp);
        Assert.NotNull(initialSetup);
        Assert.NotNull(initialSetup.PlayerLineup);
        Assert.True(initialSetup.PlayerLineup.Count >= 2);
    }

    [Fact]
    public void GetOpponentsOnTile_NoBuildings()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var players = GamePlayHelpers.GetOpponentsOnTile(board.GetGameState(), board.GetTile(TestTile.T1));
        
        Assert.NotNull(players);
        Assert.Empty(players);
    }

    [Fact]
    public void GetOpponentsOnTile_NoOpponents()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var players = GamePlayHelpers.GetOpponentsOnTile(board.GetGameState(), board.GetTile(TestTile.T5));
        
        Assert.NotNull(players);
        Assert.Empty(players);
    }

    [Fact]
    public void GetOpponentsOnTile_TwoOpponents()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        var orangePlayer = Player.CreateTestPlayer("Winston", PlayerColor.Orange);
        board.GetGameState().Players.Add(orangePlayer);
        board.GetVertex(TestVertex.V1).BuildSettlement(orangePlayer);
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        var players = GamePlayHelpers.GetOpponentsOnTile(board.GetGameState(), board.GetTile(TestTile.T0));

        Assert.NotNull(players);
        Assert.NotEmpty(players);
        Assert.Equal(2, players.Count);
    }

    // HandleTradeTimeoutIfNeeded tests

    [Fact]
    public void HandleTradeTimeoutIfNeeded_ReturnsFalse_WhenNotInRespondToTradeState()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());

        var result = GamePlayHelpers.HandleTradeTimeoutIfNeeded(gs);

        Assert.False(result);
    }

    [Fact]
    public void HandleTradeTimeoutIfNeeded_ReturnsFalse_WhenNoTradeStartTime()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        // No TradeStartTime set

        var result = GamePlayHelpers.HandleTradeTimeoutIfNeeded(gs);

        Assert.False(result);
    }

    [Fact]
    public void HandleTradeTimeoutIfNeeded_ReturnsFalse_WhenNoPendingTradeResponses()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.Phase.SetTradeStartTime(DateTime.UtcNow.AddSeconds(-60));
        // PendingTradeResponses is null

        var result = GamePlayHelpers.HandleTradeTimeoutIfNeeded(gs);

        Assert.False(result);
    }

    [Fact]
    public void HandleTradeTimeoutIfNeeded_ReturnsFalse_WhenNoHumansWaiting()
    {
        // All players are bots - no timeout needed for bot-only games
        var bot1 = Player.CreateTestPlayer("Bot1", PlayerColor.Red, isBot: true);
        var bot2 = Player.CreateTestPlayer("Bot2", PlayerColor.Blue, isBot: true);
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Players.Clear();
        gs.Players.Add(bot1);
        gs.Players.Add(bot2);
        gs.Phase = new GamePhase(GameStates.RespondToTrade, bot1, bot2);
        gs.Phase.SetTradeStartTime(DateTime.UtcNow.AddSeconds(-60));
        gs.Phase.AddPendingTradeResponse(new TradeResponse(bot1, TradeResponseType.Original,
            new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } },
            new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } }));

        var result = GamePlayHelpers.HandleTradeTimeoutIfNeeded(gs);

        Assert.False(result);
    }

    [Fact]
    public void HandleTradeTimeoutIfNeeded_ReturnsFalse_WhenNotTimedOut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.Phase.SetTradeStartTime(DateTime.UtcNow.AddSeconds(-5)); // Only 5 seconds ago
        gs.Phase.AddPendingTradeResponse(new TradeResponse(board.GetRedPlayer(), TradeResponseType.Original,
            new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } },
            new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } }));

        var result = GamePlayHelpers.HandleTradeTimeoutIfNeeded(gs);

        Assert.False(result);
        Assert.Equal(GameStates.RespondToTrade, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.PendingTradeResponses);
    }

    [Fact]
    public void HandleTradeTimeoutIfNeeded_ReturnsTrue_WhenTimedOutWithHumanWaiting()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.Phase.SetTradeStartTime(DateTime.UtcNow.AddSeconds(-31)); // 31 seconds ago - timed out
        gs.Phase.AddPendingTradeResponse(new TradeResponse(board.GetRedPlayer(), TradeResponseType.Original,
            new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } },
            new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } }));

        var result = GamePlayHelpers.HandleTradeTimeoutIfNeeded(gs);

        Assert.True(result);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.Null(gs.Phase.PendingTradeResponses);
        Assert.Null(gs.Phase.TradeStartTime);
    }

    [Fact]
    public void HandleTradeTimeoutIfNeeded_ReturnsFalse_WhenAllHumansHaveResponded()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());
        gs.Phase.SetTradeStartTime(DateTime.UtcNow.AddSeconds(-60));
        gs.Phase.AddPendingTradeResponse(new TradeResponse(board.GetRedPlayer(), TradeResponseType.Original,
            new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } },
            new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } }));
        // Blue player has already responded
        gs.Phase.AddPendingTradeResponse(new TradeResponse(board.GetBluePlayer(), TradeResponseType.Reject,
            new Dictionary<ResourceType, int>(),
            new Dictionary<ResourceType, int>()));

        var result = GamePlayHelpers.HandleTradeTimeoutIfNeeded(gs);

        Assert.False(result);
    }
}
