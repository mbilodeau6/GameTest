using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Functions;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;

namespace GameTest.Tests;

public class BotAITests
{
    private GameState CreateBoardForSetupTest(GameStates state)
    {
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        GamePlayHelpers.LinkEdgesAndVertices(gs);
        gs.Players.Clear();
        
        var player1 = new Player("Bot", PlayerColor.Blue, true);
        gs.Players.Add(player1);
        var player2 = new Player("Henry", PlayerColor.Red);
        gs.Players.Add(player2);

        gs.Phase = new GamePhase(state, player1);

        return gs;
    }
    
    [Fact]
    public void Constructor_ValidGameState()
    {
        // Arrange
        GameState gs = CreateBoardForSetupTest(GameStates.SettingUpBoard);

        // Act
        var bai = new BotAI(gs);

        // TODO: Need to find something I can test for after creation. Right now,
        // this just verifies no exception thrown.
        // Assert
        Assert.NotNull(bai);
    }

    [Fact]
    public void Constructor_MissingGameState()
    {
        // Arrange
        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BotAI(null));

        Assert.Equal("Value cannot be null. (Parameter 'gs')", exception.Message);
    }

    [Fact]
    public void Constructor_CurrentPlayerMissingOrNotBot()
    {
        // Arrange
        var gs = new GameState(new Guid());

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new BotAI(gs));

        Assert.Equal("Current player must be identified and must be a Bot.", exception.Message);
    }

    [Fact]
    public void GetSetUpMove_WrongCurrentState()
    {
        var gs = CreateBoardForSetupTest(GameStates.BuildOrTrade);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[0], gs.Players[1]);

        var bai = new BotAI(gs);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            bai.GetSetUpMove());

        Assert.StartsWith("GetSetUp should only be called if in one of the phases. Current phase is", exception.Message);
    }

    [Fact]
    public void GetSetUpMove_FirstSettlement()
    {
        // Arrange
        GameState gs = CreateBoardForSetupTest(GameStates.PlaceFirstSettlement);
        var bai = new BotAI(gs);

        Assert.Equal(0, GamePlayHelpers.CountSettlementsForPlayer(gs, gs.Players[0]));

        // Act
        var move = bai.GetSetUpMove();

        Assert.NotNull(move.VertexMove);
        Assert.Equal(BuildingType.Settlement.ToString(), move.VertexMove.Building);
        Assert.Null(move.EdgeMove);
    }

    [Fact]
    public void GetSetUpMove_FirstRoad()
    {
        // Arrange
        GameState gs = CreateBoardForSetupTest(GameStates.PlaceFirstRoad);
        var bai = new BotAI(gs);

        var brickTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, -1);
        var vertex = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, brickTile, null, null, VertexDirection.NE);
        vertex.BuildSettlement(gs.Players[0]);

        Assert.Equal(0, GamePlayHelpers.CountRoadsForPlayer(gs, gs.Players[0]));

        // Act
        var move = bai.GetSetUpMove();

        Assert.NotNull(move.EdgeMove);
        Assert.NotNull(move.EdgeMove.PlayerId);
        Assert.Equal(gs.Players[0].Id, move.EdgeMove.PlayerId);
        Assert.Null(move.VertexMove);

        var edge = gs.Edges.First(e => e.Id == move.EdgeMove.Id);
        Assert.NotNull(edge);
        Assert.NotEmpty(edge.Vertices);
        Assert.Contains(edge.Vertices, v => (v.Building == BuildingType.Settlement || v.Building == BuildingType.City) && v.Owner.Id == move.EdgeMove.PlayerId);
    }

    [Fact]
    public void GetSetUpMove_SecondSettlement()
    {
        // Arrange
        GameState gs = CreateBoardForSetupTest(GameStates.PlaceSecondSettlement);
        gs.Vertices[0].BuildSettlement(gs.Players[0]);
        gs.Vertices[1].BuildSettlement(gs.Players[1]);

        var bai = new BotAI(gs);

        Assert.Equal(1, GamePlayHelpers.CountSettlementsForPlayer(gs, gs.Players[0]));

        // Act
        var move = bai.GetSetUpMove();

        Assert.NotNull(move.VertexMove);
        Assert.Equal(BuildingType.Settlement.ToString(), move.VertexMove.Building);
        Assert.True(move.VertexMove.Id != gs.Vertices[0].Id);
        Assert.True(move.VertexMove.Id != gs.Vertices[1].Id);
        Assert.Null(move.EdgeMove);
    }

    [Fact]
    public void GetSetUpMove_SecondRoad()
    {
        // Arrange
        GameState gs = CreateBoardForSetupTest(GameStates.PlaceSecondRoad);
        gs.Edges[0].BuildRoad(gs.Players[1]);

        var brickTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, -1);
        var v1 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, brickTile, null, null, VertexDirection.NE);
        v1.BuildSettlement(gs.Players[0]);
        v1.Edges[0].BuildRoad(gs.Players[0]);

        var grainTile = BoardCreationHelpers.GetTileAt(gs.Tiles, -3, -1);
        var v2 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, grainTile, null, null, VertexDirection.NW);
        v2.BuildSettlement(gs.Players[0]);

        var bai = new BotAI(gs);

        Assert.Equal(1, GamePlayHelpers.CountRoadsForPlayer(gs, gs.Players[0]));

        // Act
        var move = bai.GetSetUpMove();

        Assert.NotNull(move.EdgeMove);
        Assert.NotNull(move.EdgeMove.PlayerId);
        Assert.Equal(gs.Players[0].Id, move.EdgeMove.PlayerId);
        Assert.True(move.EdgeMove.Id != gs.Edges[0].Id);
        Assert.Null(move.VertexMove);

        var edge = gs.Edges.First(e => e.Id == move.EdgeMove.Id);
        Assert.NotNull(edge);
        Assert.NotEmpty(edge.Vertices);
        Assert.Contains(edge.Vertices, v => (v.Building == BuildingType.Settlement || v.Building == BuildingType.City) && v.Owner.Id == move.EdgeMove.PlayerId);
    }

    [Fact]
    public void GetPreRollMove_WrongCurrentState()
    {
        var gs = CreateBoardForSetupTest(GameStates.BuildOrTrade);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[0], gs.Players[1]);

        var bai = new BotAI(gs);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            bai.GetPreRollMove());

        Assert.StartsWith("GetPreRollMove should only be called if phase is RollOrUseDevCard. Current phase is", exception.Message);
    }


    // TODO: Need to figure out how to determine when AI should buy dev card
    // vs roll and adjust test to refelct.
    [Fact]
    public void GetPreRollMove_RollDice()
    {
        var gs = CreateBoardForSetupTest(GameStates.RollOrUseDevCard);
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, gs.Players[0], gs.Players[1]);

        var bai = new BotAI(gs);

        var move = bai.GetPreRollMove();

        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
        Assert.False(move.BuyDevelopmentCard);
        Assert.True(move.RollDice);
    }

    [Fact]
    public void GetBuildMove_WrongCurrentState()
    {
        var gs = CreateBoardForSetupTest(GameStates.RollOrUseDevCard);
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, gs.Players[0], gs.Players[1]);

        var bai = new BotAI(gs);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            bai.GetBuildMove());

        Assert.StartsWith("GetBuildMove should only be called if phase is BuildOrTrade. Current phase is", exception.Message);
    }

    // TODO: Will need to adjust when code changed to require resources and proper
    // spacing from other development. Current version of GetBuildMove() just
    // picks the next open spot for a road and settlement.
    // TODO: Also need to figure out when Bot should build each resource, buy dev
    // card, trade, and end turn.
    [Fact]
    public void GetBuildMove_BuildRoadAndSettlement()
    {
        var gs = CreateBoardForSetupTest(GameStates.BuildOrTrade);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[0], gs.Players[1]);

        var bai = new BotAI(gs);

        var move = bai.GetBuildMove();

        Assert.NotNull(move.EdgeMove);
        Assert.Equal(gs.Players[0].Id, move.EdgeMove.PlayerId);
        var selectedEdge = gs.Edges.First(e => e.Id == move.EdgeMove.Id);
        Assert.Null(selectedEdge.Owner);

        Assert.NotNull(move.VertexMove);
        Assert.Equal(gs.Players[0].Id, move.VertexMove.PlayerId);
        var selectedVertex = gs.Vertices.First(v => v.Id == move.VertexMove.Id);
        Assert.Null(selectedVertex.Building);

        Assert.False(move.BuyDevelopmentCard);
        Assert.False(move.RollDice);
    }
}