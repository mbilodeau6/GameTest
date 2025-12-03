using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Functions;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;

namespace GameTest.Tests;

public class BotAITests
{
    private static GameState CreateBoardForSetupTest(GameStates state)
    {
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);

        gs.Phase = new GamePhase(state, gs.Players.First(p => p.IsBot == true));

        return gs;
    }

    private static TestGameBoard CreateBoardForBuildTest()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        GamePlayHelpers.MarkBlockedVertices(board.GetGameState());

        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        return board;
    }

    private static Player GetBotPlayer(GameState gs)
    {
        return gs.Players.First(p => p.IsBot);
    }

    private static Player GetHumanPlayer(GameState gs)
    {
        return gs.Players.First(p => !p.IsBot);
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
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, GetBotPlayer(gs), GetHumanPlayer(gs));

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

        Assert.Equal(0, GamePlayHelpers.CountSettlementsForPlayer(gs, GetBotPlayer(gs)));

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
        var vertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, brickTile, null, null, VertexDirection.NE);
        var botPlayer = GetBotPlayer(gs);
        vertex.BuildSettlement(botPlayer);

        Assert.Equal(0, GamePlayHelpers.CountRoadsForPlayer(gs, botPlayer));

        // Act
        var move = bai.GetSetUpMove();

        Assert.NotNull(move.EdgeMove);
        Assert.NotNull(move.EdgeMove.PlayerId);
        Assert.Equal(botPlayer.Id, move.EdgeMove.PlayerId);
        Assert.Null(move.VertexMove);

        var edge = gs.Edges.First(e => e.Id == move.EdgeMove.Id);
        Assert.NotNull(edge);
        Assert.NotEmpty(edge.Vertices);
        Assert.Contains(edge.Vertices, v => GamePlayHelpers.HasBuilding(v) && v.Owner != null && v.Owner.Id == move.EdgeMove.PlayerId);
    }

    [Fact]
    public void GetSetUpMove_SecondSettlement()
    {
        // Arrange
        GameState gs = CreateBoardForSetupTest(GameStates.PlaceSecondSettlement);
        gs.Vertices[0].BuildSettlement(GetBotPlayer(gs));
        gs.Vertices[1].BuildSettlement(GetHumanPlayer(gs));

        var bai = new BotAI(gs);

        Assert.Equal(1, GamePlayHelpers.CountSettlementsForPlayer(gs, GetBotPlayer(gs)));

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
        gs.Edges[0].BuildRoad(GetHumanPlayer(gs));

        var botPlayer = GetBotPlayer(gs);
        var brickTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, -1);
        var v1 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, brickTile, null, null, VertexDirection.NE);
        v1.BuildSettlement(botPlayer);
        v1.Edges[0].BuildRoad(botPlayer);

        var grainTile = BoardCreationHelpers.GetTileAt(gs.Tiles, -3, -1);
        var v2 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, grainTile, null, null, VertexDirection.NW);
        v2.BuildSettlement(botPlayer);

        var bai = new BotAI(gs);

        Assert.Equal(1, GamePlayHelpers.CountRoadsForPlayer(gs, botPlayer));

        // Act
        var move = bai.GetSetUpMove();

        Assert.NotNull(move.EdgeMove);
        Assert.NotNull(move.EdgeMove.PlayerId);
        Assert.Equal(botPlayer.Id, move.EdgeMove.PlayerId);
        Assert.True(move.EdgeMove.Id != gs.Edges[0].Id);
        Assert.Null(move.VertexMove);

        var edge = gs.Edges.First(e => e.Id == move.EdgeMove.Id);
        Assert.NotNull(edge);
        Assert.NotEmpty(edge.Vertices);
        Assert.Contains(edge.Vertices, v => (v.Building == BuildingType.Settlement || v.Building == BuildingType.City)
            && v.Owner != null && v.Owner.Id == move.EdgeMove.PlayerId);
    }

    [Fact]
    public void GetPreRollMove_WrongCurrentState()
    {
        var gs = CreateBoardForSetupTest(GameStates.BuildOrTrade);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, GetBotPlayer(gs), GetHumanPlayer(gs));

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
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, GetBotPlayer(gs), GetHumanPlayer(gs));

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
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, GetBotPlayer(gs), GetHumanPlayer(gs));

        var bai = new BotAI(gs);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            bai.GetBuildMove());

        Assert.StartsWith("GetBuildMove should only be called if phase is BuildOrTrade. Current phase is", exception.Message);
    }

    [Fact]
    public void GetBuildMove_EndTurnIfNoResources()
    {
        var gs = CreateBoardForSetupTest(GameStates.BuildOrTrade);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, GetBotPlayer(gs), GetHumanPlayer(gs));

        var bai = new BotAI(gs);

        var move = bai.GetBuildMove();

        Assert.True(move.EndTurn);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
    }

    // TODO: Need to figure out when Bot should build each resource, buy dev
    // card, trade, and end turn. Current version just builds settlement, if it can.
    // If it can't build a settlement, it builds a road, if it can.
    [Fact]
    public void GetBuildMove_BuildRoadAndSettlement()
    {
        var gs = CreateBoardForSetupTest(GameStates.BuildOrTrade);
        var botPlayer = GetBotPlayer(gs);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, GetHumanPlayer(gs));

        var desertTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 0, 0);
        var brickTile = BoardCreationHelpers.GetTileAt(gs.Tiles, -1, -1);
        var sheepTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 1, -1);
        var vertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, desertTile, brickTile, sheepTile, null);
        vertex.BuildSettlement(botPlayer);
        GamePlayHelpers.MarkBlockedVertices(gs, vertex);

        var edge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, desertTile, sheepTile, null);
        edge.BuildRoad(botPlayer);

        botPlayer.Resources[ResourceType.Brick] = 1;
        botPlayer.Resources[ResourceType.Wood] = 1;

        var bai = new BotAI(gs);

        var move = bai.GetBuildMove();

        Assert.NotNull(move.EdgeMove);
        Assert.Equal(botPlayer.Id, move.EdgeMove.PlayerId);
        var selectedEdge = gs.Edges.First(e => e.Id == move.EdgeMove.Id);
        Assert.Null(selectedEdge.Owner);

        Assert.False(move.BuyDevelopmentCard);
        Assert.False(move.RollDice);
    }

    [Fact]
    public void AnalyzePossibleBankTrades_NotEnoughResources_NoTradePossible()
    {
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();
        botPlayer.AssignResources(ResourceType.Wood, 3);

        var bot = new BotAI(board.GetGameState());

        (var canTrade, var tradeRequest) = bot.AnalyzePossibleBankTrades();

        Assert.False(canTrade);
        Assert.Null(tradeRequest);
    }

    [Fact]
    public void AnalyzePossibleBankTrades_NotEnoughResources_KeepOreForCity()
    {
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();
        botPlayer.AssignResources(ResourceType.Ore, 4);

        var bot = new BotAI(board.GetGameState());

        (var canTrade, var tradeRequest) = bot.AnalyzePossibleBankTrades();

        Assert.False(canTrade);
        Assert.Null(tradeRequest);
    }

    [Fact]
    public void AnalyzePossibleBankTrades_NeedSheepForSettlement_TradeOre()
    {
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();

        // Create city with two roads leading to a spot where a settlement could be built
        board.GetVertex(TestVertex.V3).UpgradeToCity();
        board.GetEdge(TestEdge.E10).BuildRoad(botPlayer);

        botPlayer.AssignResources(ResourceType.Ore, 4);
        botPlayer.AssignResources(ResourceType.Brick, 2);
        botPlayer.AssignResources(ResourceType.Wood, 1);
        botPlayer.AssignResources(ResourceType.Grain, 1);

        var bot = new BotAI(board.GetGameState());

        (var canTrade, var tradeRequest) = bot.AnalyzePossibleBankTrades();

        Assert.True(canTrade);
        Assert.NotNull(tradeRequest);
        Assert.Contains(ResourceType.Ore, tradeRequest.Offer);
        Assert.Contains(ResourceType.Wool, tradeRequest.Request);
    }

    [Fact]
    public void AnalyzePossibleBankTrades_NeedBrickForRoad_TradeOre()
    {
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();
        board.GetVertex(TestVertex.V3).UpgradeToCity();

        botPlayer.AssignResources(ResourceType.Ore, 4);
        botPlayer.AssignResources(ResourceType.Wood, 1);
        botPlayer.AssignResources(ResourceType.Grain, 1);

        var bot = new BotAI(board.GetGameState());

        (var canTrade, var tradeRequest) = bot.AnalyzePossibleBankTrades();

        Assert.True(canTrade);
        Assert.NotNull(tradeRequest);
        Assert.Contains(ResourceType.Ore, tradeRequest.Offer);
        Assert.Contains(ResourceType.Brick, tradeRequest.Request);
    }

    [Fact]
    public void AnalyzePossibleBankTrades_TradeRecommended_TradeWoodNotOre()
    {
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();

        botPlayer.AssignResources(ResourceType.Ore, 5);
        botPlayer.AssignResources(ResourceType.Grain, 1);
        botPlayer.AssignResources(ResourceType.Wood, 5);

        var bot = new BotAI(board.GetGameState());

        (var canTrade, var tradeRequest) = bot.AnalyzePossibleBankTrades();

        Assert.True(canTrade);
        Assert.NotNull(tradeRequest);
        Assert.Contains(ResourceType.Wood, tradeRequest.Offer);
        Assert.DoesNotContain(ResourceType.Ore, tradeRequest.Offer);
        Assert.Equal(4, tradeRequest.Offer[ResourceType.Wood]);
        Assert.Contains(ResourceType.Grain, tradeRequest.Request);
    }

    [Fact]
    public void AnalyzePossibleBankTrades_BetterToBuildRoad_TradeWoolForBrick()
    {
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();

        botPlayer.AssignResources(ResourceType.Ore, 1);
        botPlayer.AssignResources(ResourceType.Grain, 1);
        botPlayer.AssignResources(ResourceType.Wood, 2);
        botPlayer.AssignResources(ResourceType.Wool, 4);

        var bot = new BotAI(board.GetGameState());

        (var canTrade, var tradeRequest) = bot.AnalyzePossibleBankTrades();

        Assert.True(canTrade);
        Assert.NotNull(tradeRequest);
        Assert.Contains(ResourceType.Wool, tradeRequest.Offer);
        Assert.Contains(ResourceType.Brick, tradeRequest.Request);
    }

    [Fact]
    public void AnalyzePossibleBankTrades_BuiltAllCities()
    {
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();

        board.GetVertex(TestVertex.V3).UpgradeToCity();
        board.GetEdge(TestEdge.E10).BuildRoad(botPlayer);
        board.GetVertex(TestVertex.V16).BuildSettlement(botPlayer);
        board.GetVertex(TestVertex.V16).UpgradeToCity();

        botPlayer.AssignResources(ResourceType.Ore, 4);
        botPlayer.AssignResources(ResourceType.Brick, 2);
        botPlayer.AssignResources(ResourceType.Grain, 1);

        var bot = new BotAI(board.GetGameState());

        (var canTrade, var tradeRequest) = bot.AnalyzePossibleBankTrades();

        Assert.True(canTrade);
        Assert.NotNull(tradeRequest);
        Assert.Contains(ResourceType.Ore, tradeRequest.Offer);
        Assert.Contains(ResourceType.Wood, tradeRequest.Request);    
    }

    [Fact]
    public void AnalyzePossibleBankTrades_BuiltAllSettlements()
    {
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();

        board.GetEdge(TestEdge.E10).BuildRoad(botPlayer);
        board.GetVertex(TestVertex.V16).BuildSettlement(botPlayer);
        board.GetEdge(TestEdge.E2).BuildRoad(botPlayer);
        board.GetEdge(TestEdge.E8).BuildRoad(botPlayer);
        board.GetVertex(TestVertex.V10).BuildSettlement(botPlayer);

        botPlayer.AssignResources(ResourceType.Wood, 4);
        botPlayer.AssignResources(ResourceType.Brick, 2);
        botPlayer.AssignResources(ResourceType.Grain, 1);

        var bot = new BotAI(board.GetGameState());

        (var canTrade, var tradeRequest) = bot.AnalyzePossibleBankTrades();

        Assert.True(canTrade);
        Assert.NotNull(tradeRequest);
        Assert.Contains(ResourceType.Wood, tradeRequest.Offer);
        Assert.Contains(ResourceType.Ore, tradeRequest.Request);    
    }

    [Fact]
    public void GetRobberMove_SelectTile()
    {
        // Arrange
        var board = CreateBoardForBuildTest();

        board.GetGameState().Phase.CurrentPlayer = board.GetBluePlayer();
        board.GetGameState().Phase.PhaseState = GameStates.PlaceRobber;

        var bot = new BotAI(board.GetGameState());

        // Act
        var move = bot.GetRobberMove();

        // Assert
        Assert.NotNull(move.TileMove);
        Assert.Equal(board.GetTile(TestTile.T5).Id, move.TileMove.Id);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
        Assert.False(move.RollDice);
    }
}