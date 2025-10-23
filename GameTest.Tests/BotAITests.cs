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

        Assert.Equal(0, GamePlayHelpers.CountRoadsForPlayer(gs, gs.Players[0]));

        // Act
        var move = bai.GetSetUpMove();

        Assert.NotNull(move.EdgeMove);
        Assert.NotNull(move.EdgeMove.PlayerId);
        Assert.Equal(gs.Players[0].Id, move.EdgeMove.PlayerId);
        Assert.Null(move.VertexMove);
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
        gs.Edges[0].BuildRoad(gs.Players[0]);
        gs.Edges[1].BuildRoad(gs.Players[1]);

        var bai = new BotAI(gs);

        Assert.Equal(1, GamePlayHelpers.CountRoadsForPlayer(gs, gs.Players[0]));

        // Act
        var move = bai.GetSetUpMove();

        Assert.NotNull(move.EdgeMove);
        Assert.NotNull(move.EdgeMove.PlayerId);
        Assert.Equal(gs.Players[0].Id, move.EdgeMove.PlayerId);
        Assert.True(move.EdgeMove.Id != gs.Edges[0].Id);
        Assert.True(move.EdgeMove.Id != gs.Edges[1].Id);
        Assert.Null(move.VertexMove);
    }
}