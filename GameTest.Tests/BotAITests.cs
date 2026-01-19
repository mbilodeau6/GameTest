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
        // TODO: Should change to Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

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
        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // TODO: Need to find something I can test for after creation. Right now,
        // this just verifies no exception thrown.
        // Assert
        Assert.NotNull(bai);
    }

    [Fact]
    public void Constructor_MissingGameState()
    {
        // Arrange
        var bot = Player.CreateTestPlayer("bot", PlayerColor.Blue, true);

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BotAI(null!, bot));

        Assert.Equal("Value cannot be null. (Parameter 'gs')", exception.Message);
    }

    [Fact]
    public void Constructor_MissingBot()
    {
        // Arrange
        var gs = new GameState(new Guid(), "UT");

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BotAI(gs, null!));

        Assert.Equal("Value cannot be null. (Parameter 'bot')", exception.Message);
    }

    [Fact]
    public void Constructor_PlayerNotBot()
    {
        // Arrange
        var gs = new GameState(new Guid(), "UT");
        var human = Player.CreateTestPlayer("human", PlayerColor.Red, false);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new BotAI(gs, human));

        Assert.Equal("Player must be a Bot.", exception.Message);
    }

    [Fact]
    public void GetSetUpMove_WrongCurrentState()
    {
        var gs = CreateBoardForSetupTest(GameStates.BuildOrTrade);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, GetBotPlayer(gs), GetHumanPlayer(gs));

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            bai.GetSetUpMove());
    }

    [Fact]
    public void GetSetUpMove_FirstSettlement()
    {
        // Arrange
        GameState gs = CreateBoardForSetupTest(GameStates.PlaceFirstSettlement);
        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        Assert.Equal(0, gs.CountSettlementsForPlayer(GetBotPlayer(gs)));

        // Act
        var move = bai.GetSetUpMove();

        Assert.NotNull(move.VertexMove);
        Assert.Equal(BuildingType.Settlement, move.VertexMove.Building);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.SelectedPlayer);
    }

    [Fact]
    public void GetSetUpMove_FirstRoad()
    {
        // Arrange
        GameState gs = CreateBoardForSetupTest(GameStates.PlaceFirstRoad);
        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        var brickTile = gs.GetTileAt(3, -1);
        var vertex = gs.GetVertexFromTileInfo(brickTile, null, null, VertexDirection.NE);
        var botPlayer = GetBotPlayer(gs);
        vertex.BuildSettlement(botPlayer);

        Assert.Equal(0, gs.CountRoadsForPlayer(botPlayer));

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
        Assert.Null(move.SelectedPlayer);
    }

    [Fact]
    public void GetSetUpMove_SecondSettlement()
    {
        // Arrange
        GameState gs = CreateBoardForSetupTest(GameStates.PlaceSecondSettlement);
        gs.Vertices[0].BuildSettlement(GetBotPlayer(gs));
        gs.Vertices[1].BuildSettlement(GetHumanPlayer(gs));

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        Assert.Equal(1, gs.CountSettlementsForPlayer(GetBotPlayer(gs)));

        // Act
        var move = bai.GetSetUpMove();

        Assert.NotNull(move.VertexMove);
        Assert.Equal(BuildingType.Settlement, move.VertexMove.Building);
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
        var brickTile = gs.GetTileAt(3, -1);
        var v1 = gs.GetVertexFromTileInfo(brickTile, null, null, VertexDirection.NE);
        v1.BuildSettlement(botPlayer);
        v1.Edges[0].BuildRoad(botPlayer);

        var grainTile = gs.GetTileAt(-3, -1);
        var v2 = gs.GetVertexFromTileInfo(grainTile, null, null, VertexDirection.NW);
        v2.BuildSettlement(botPlayer);

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        Assert.Equal(1, gs.CountRoadsForPlayer(botPlayer));

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

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            bai.GetPreRollMove());

        Assert.StartsWith("GetPreRollMove should only be called if phase is RollOrUseDevCard. Current phase is", exception.Message);
    }


    [Fact]
    public void GetPreRollMove_RollDice_NoDevCards()
    {
        var gs = CreateBoardForSetupTest(GameStates.RollOrUseDevCard);
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, GetBotPlayer(gs), GetHumanPlayer(gs));

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        var move = bai.GetPreRollMove();

        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
        Assert.False(move.BuyDevelopmentCard);
        Assert.True(move.RollDice);
        Assert.Null(move.PlayDevelopmentCard);
        Assert.Null(move.SelectedPlayer);
    }

    [Fact]
    public void GetPreRollMove_PlayKnight_RobberOnBotTile()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, botPlayer, board.GetRedPlayer());

        // Give bot a Knight ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Knight);

        // Robber starts on desert (T6). Move it to a tile with bot's settlement (V3 borders T0, T3, T2)
        gs.SetRobberTile(board.GetTile(TestTile.T0));
        Assert.Equal(board.GetTile(TestTile.T0).Id, gs.RobberTile.Id);

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetPreRollMove();

        // Assert
        Assert.Equal(DevelopmentCardType.Knight, move.PlayDevelopmentCard);
        Assert.NotNull(move.TileMove);
        // Robber was on T0. Opponent is on T4 (wool 2) and T5 (brick 5). T5 is better target.
        Assert.Equal(board.GetTile(TestTile.T5).Id, move.TileMove.Id);
        Assert.False(move.RollDice);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
    }

    [Fact]
    public void GetPreRollMove_RollDice_HasKnightButRobberNotOnBotTile()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, botPlayer, board.GetRedPlayer());

        // Give bot a Knight ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Knight);

        // Robber is on desert (T6) which doesn't border bot's settlement
        Assert.Equal(board.GetTile(TestTile.T6).Id, gs.RobberTile.Id);

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetPreRollMove();

        // Assert - Should roll dice, not play Knight (no benefit pre-roll)
        Assert.True(move.RollDice);
        Assert.Null(move.PlayDevelopmentCard);
        Assert.Null(move.TileMove);
    }

    [Fact]
    public void GetPreRollMove_RollDice_HasYoPButNotKnight()
    {
        // Arrange - Bot has YoP but not Knight. Should roll, not play YoP pre-roll.
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, botPlayer, board.GetRedPlayer());

        // Give bot YoP ready to play (but no Knight)
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.YearOfPlenty);

        // Even if robber is on bot's tile, YoP shouldn't be played pre-roll
        gs.SetRobberTile(board.GetTile(TestTile.T4));

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetPreRollMove();

        // Assert - Should roll dice, YoP is not played pre-roll
        Assert.True(move.RollDice);
        Assert.Null(move.PlayDevelopmentCard);
    }

    // ==================== GetDevCardRoadMove Tests ====================

    [Fact]
    public void GetDevCardRoadMove_WrongCurrentState()
    {
        var gs = CreateBoardForSetupTest(GameStates.BuildOrTrade);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, GetBotPlayer(gs), GetHumanPlayer(gs));

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            bai.GetDevCardRoadMove());

        Assert.StartsWith("GetDevCardRoadMove should only be called if phase is FirstDevCardRoad or SecondDevCardRoad.", exception.Message);
    }

    [Fact]
    public void GetDevCardRoadMove_FirstRoad_BuildsTowardHighValueVertex()
    {
        // Arrange - Bot is in FirstDevCardRoad phase
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.FirstDevCardRoad, botPlayer, board.GetRedPlayer());

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetDevCardRoadMove();

        // Assert - Should return an edge move toward a high value vertex
        Assert.NotNull(move.EdgeMove);
    }

    [Fact]
    public void GetDevCardRoadMove_SecondRoad_BuildsTowardNextVertex()
    {
        // Arrange - Bot is in SecondDevCardRoad phase (first road already built)
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();

        // Build the first dev card road
        board.GetEdge(TestEdge.E2).BuildRoad(botPlayer);

        gs.Phase = new GamePhase(GameStates.SecondDevCardRoad, botPlayer, board.GetRedPlayer());

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetDevCardRoadMove();

        // Assert - Should return an edge move (continuing toward vertex or next best option)
        Assert.NotNull(move.EdgeMove);
    }

    // ==================== GetBuildMove Tests ====================

    [Fact]
    public void GetBuildMove_WrongCurrentState()
    {
        var gs = CreateBoardForSetupTest(GameStates.RollOrUseDevCard);
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, GetBotPlayer(gs), GetHumanPlayer(gs));

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            bai.GetBuildMove());

        Assert.StartsWith("GetBuildMove should only be called if phase is BuildOrTrade. Current phase is", exception.Message);
    }

    [Fact]
    public void GetBuildMove_EndTurnIfNoResources()
    {
        var gs = CreateBoardForSetupTest(GameStates.BuildOrTrade);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, GetBotPlayer(gs), GetHumanPlayer(gs));

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        var move = bai.GetBuildMove();

        Assert.True(move.EndTurn);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
    }

    [Fact]
    public void GetBuildMove_BuildRoad()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var botPlayer = board.GetBluePlayer();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, GetHumanPlayer(gs));

        botPlayer.Resources[ResourceType.Brick] = 1;
        botPlayer.Resources[ResourceType.Wood] = 1;

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        var move = bai.GetBuildMove();

        Assert.NotNull(move.EdgeMove);
        Assert.Equal(botPlayer.Id, move.EdgeMove.PlayerId);
        var selectedEdge = gs.Edges.First(e => e.Id == move.EdgeMove.Id);
        Assert.Null(selectedEdge.Owner);
        Assert.Null(move.VertexMove);
        Assert.False(move.BuyDevelopmentCard);
        Assert.False(move.RollDice);
        Assert.Null(move.SelectedPlayer);
    }

    [Fact]
    public void GetBuildMove_BuildCity()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var botPlayer = board.GetBluePlayer();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, GetHumanPlayer(gs));

        botPlayer.Resources[ResourceType.Ore] = 4;
        botPlayer.Resources[ResourceType.Grain] = 2;
        botPlayer.Resources[ResourceType.Wool] = 1;

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        var move = bai.GetBuildMove();

        Assert.NotNull(move.VertexMove);
        Assert.Equal(BuildingType.City, move.VertexMove.Building);
        Assert.Equal(botPlayer.Id, move.VertexMove.PlayerId);
        var selectedVertex = gs.Vertices.First(v => v.Id == move.VertexMove.Id);
        Assert.NotNull(selectedVertex.Owner);
        Assert.Equal(botPlayer.Id, selectedVertex.Owner.Id);
        Assert.Equal(BuildingType.Settlement, selectedVertex.Building);
        Assert.Null(move.EdgeMove);
        Assert.False(move.BuyDevelopmentCard);
        Assert.False(move.RollDice);
    }

    [Fact]
    public void GetBuildMove_BuildSettlement()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var botPlayer = board.GetBluePlayer();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, GetHumanPlayer(gs));
        board.GetEdge(TestEdge.E10).BuildRoad(botPlayer);

        botPlayer.Resources[ResourceType.Brick] = 1;
        botPlayer.Resources[ResourceType.Grain] = 2;
        botPlayer.Resources[ResourceType.Wool] = 1;
        botPlayer.Resources[ResourceType.Wood] = 1;

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        var move = bai.GetBuildMove();

        Assert.NotNull(move.VertexMove);
        Assert.Equal(BuildingType.Settlement, move.VertexMove.Building);
        Assert.Equal(botPlayer.Id, move.VertexMove.PlayerId);
        var selectedVertex = gs.Vertices.First(v => v.Id == move.VertexMove.Id);
        Assert.Null(selectedVertex.Owner);
        Assert.Null(move.EdgeMove);
        Assert.False(move.BuyDevelopmentCard);
        Assert.False(move.RollDice);
    }

    [Fact]
    public void GetBuildMove_TradeWithBank()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var botPlayer = board.GetBluePlayer();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, GetHumanPlayer(gs));

        botPlayer.Resources[ResourceType.Brick] = 1;
        botPlayer.Resources[ResourceType.Wool] = 4;

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        var move = bai.GetBuildMove();

        Assert.NotNull(move.BankTrade);
        Assert.Equal(move.BankTrade.PlayerId, botPlayer.Id);
        Assert.NotNull(move.BankTrade.Offer);
        Assert.Contains(ResourceType.Wool, move.BankTrade.Offer.Keys);
        Assert.Equal(4, move.BankTrade.Offer[ResourceType.Wool]);
        Assert.NotNull(move.BankTrade.Request);
        Assert.Contains(ResourceType.Wood, move.BankTrade.Request.Keys);
        Assert.Equal(1, move.BankTrade.Request[ResourceType.Wood]);
        Assert.Null(move.VertexMove);
        Assert.Null(move.EdgeMove);
        Assert.False(move.BuyDevelopmentCard);
        Assert.False(move.RollDice);
        Assert.Null(move.SelectedPlayer);
    }

    // TODO: Need to add
    // 1. More complex scenarios where AI needs to make a decision on what to do
    // 2. Buy Dev Card
    // 3. Play Dev Card (especially when it has multiple options)

    [Fact]
    public void AnalyzePossibleBankTrades_NotEnoughResources_NoTradePossible()
    {
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();
        botPlayer.AssignResources(ResourceType.Wood, 3);

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

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

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

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

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

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

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

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

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

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

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

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

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

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

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

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

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        var move = bot.GetRobberMove();

        // Assert
        Assert.NotNull(move.TileMove);
        Assert.Equal(board.GetTile(TestTile.T5).Id, move.TileMove.Id);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
        Assert.False(move.RollDice);
        Assert.Null(move.SelectedPlayer);
    }

    private TestGameBoard CreateBoardForMultiPlayerTest()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        board.GetVertex(TestVertex.V4).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V19).BuildSettlement(board.GetBluePlayer());
        var thirdPlayer = Player.CreateTestPlayer("Harry", PlayerColor.Orange, true);
        board.GetGameState().Players.Add(thirdPlayer);
        board.GetVertex(TestVertex.V2).BuildSettlement(thirdPlayer);
        GamePlayHelpers.MarkBlockedVertices(board.GetGameState());
        board.GetGameState().UpdatePlayerVictoryPoints();

        return board;
    }

    [Fact]
    public void GetRobberMove_TwoOpponents()
    {
        // Arrange
        var board = CreateBoardForMultiPlayerTest();

        board.GetGameState().Phase.CurrentPlayer = board.GetBluePlayer();
        board.GetGameState().Phase.PhaseState = GameStates.PlaceRobber;

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        var move = bot.GetRobberMove();

        // Assert
        Assert.NotNull(move.TileMove);
        Assert.Equal(board.GetTile(TestTile.T0).Id, move.TileMove.Id);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
        Assert.False(move.RollDice);
        Assert.Null(move.SelectedPlayer);
    }

    [Fact]
    public void GetRobberMove_TwoOpponentsOneWinning()
    {
        // Arrange
        var board = CreateBoardForMultiPlayerTest();
        var human = board.GetRedPlayer();
        human.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        human.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        board.GetGameState().UpdatePlayerVictoryPoints();

        board.GetGameState().Phase.CurrentPlayer = board.GetBluePlayer();
        board.GetGameState().Phase.PhaseState = GameStates.PlaceRobber;

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        var move = bot.GetRobberMove();

        // Assert
        Assert.NotNull(move.TileMove);
        Assert.Equal(board.GetTile(TestTile.T3).Id, move.TileMove.Id);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
        Assert.False(move.RollDice);
        Assert.Null(move.SelectedPlayer);
   }

    [Fact]
    public void GetRobberMove_TwoOpponentsBothWinning()
    {
        // Arrange
        var board = CreateBoardForMultiPlayerTest();
        var human = board.GetRedPlayer();
        human.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        human.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        var orangePlayer = board.GetGameState().Players.First(p => p.Color == PlayerColor.Orange);
        orangePlayer.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        orangePlayer.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        board.GetGameState().UpdatePlayerVictoryPoints();

        board.GetGameState().Phase.CurrentPlayer = board.GetBluePlayer();
        board.GetGameState().Phase.PhaseState = GameStates.PlaceRobber;

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        var move = bot.GetRobberMove();

        // Assert
        Assert.NotNull(move.TileMove);
        Assert.Equal(board.GetTile(TestTile.T0).Id, move.TileMove.Id);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
        Assert.False(move.RollDice);
        Assert.Null(move.SelectedPlayer);
    }

    [Fact]
    public void GetDiscardMove_WrongState()
    {
        // Arrange
        var board = CreateBoardForBuildTest();

        board.GetGameState().Phase.CurrentPlayer = board.GetBluePlayer();
        board.GetGameState().Phase.PhaseState = GameStates.PlaceRobber;

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        Assert.Throws<InvalidOperationException>(() => bot.GetDiscardMove());
    }

    [Fact]
    public void GetDiscardMove_Valid()
    {
        // Arrange
        var board = CreateBoardForBuildTest();

        board.GetGameState().Phase.CurrentPlayer = board.GetBluePlayer();
        board.GetGameState().Phase.PhaseState = GameStates.DiscardCards;
        board.GetBluePlayer().AssignResources(ResourceType.Brick, 2);
        board.GetBluePlayer().AssignResources(ResourceType.Wood, 2);
        board.GetBluePlayer().AssignResources(ResourceType.Wool, 2);
        board.GetBluePlayer().AssignResources(ResourceType.Ore, 2);

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        var move = bot.GetDiscardMove();

        Assert.NotNull(move.DiscardResources);
        Assert.Equal(4, move.DiscardResources.Count);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
        Assert.False(move.RollDice);
        Assert.False(move.EndTurn);
    }

    [Fact]
    public void DetermineCardsToDiscard_WantToBuildCity_HaveCardsEvenAfterDiscard()
    {
        // Arrange
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();

        // Build all settlements so that settlements aren't an option. City should be the strong preference.
        board.GetVertex(TestVertex.V10).BuildSettlement(botPlayer);
        board.GetVertex(TestVertex.V16).BuildSettlement(botPlayer);

        botPlayer.AssignResources(ResourceType.Ore, 4);
        botPlayer.AssignResources(ResourceType.Grain, 3);
        botPlayer.AssignResources(ResourceType.Wood, 1);
        botPlayer.AssignResources(ResourceType.Brick, 1);

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        var discard = bot.DetermineCardsToDiscard();

        // Assert - Make sure bot keeps enough cards to build city
        Assert.Equal(4, discard.Count);
        Assert.Equal(1, discard.Count(r => r == ResourceType.Ore));
        Assert.Equal(1, discard.Count(r => r == ResourceType.Grain));
        Assert.Equal(1, discard.Count(r => r == ResourceType.Wood));
        Assert.Equal(1, discard.Count(r => r == ResourceType.Brick));
    }

    [Fact]
    public void DetermineCardsToDiscard_WantToBuildCity_HasCardsButNeedToDiscardSome()
    {
        // Arrange
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();

        // Build all settlements so that settlements aren't an option. City should be the strong preference.
        board.GetVertex(TestVertex.V10).BuildSettlement(botPlayer);
        board.GetVertex(TestVertex.V16).BuildSettlement(botPlayer);

        botPlayer.AssignResources(ResourceType.Ore, 3);
        botPlayer.AssignResources(ResourceType.Grain, 2);
        botPlayer.AssignResources(ResourceType.Wood, 2);
        botPlayer.AssignResources(ResourceType.Brick, 1);

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        var discard = bot.DetermineCardsToDiscard();

        // Assert - Make sure bot discards all other cards first
        Assert.Equal(4, discard.Count);
        Assert.Equal(2, discard.Count(r => r == ResourceType.Wood));
        Assert.Equal(1, discard.Count(r => r == ResourceType.Brick));
    }

    [Fact]
    public void DetermineCardsToDiscard_WantToBuildRoad_HaveCardsEvenAfterDiscard()
    {
        // Arrange
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();

        // Only build is already city and don't have roads to open settlement (yet)
        board.GetVertex(TestVertex.V3).UpgradeToCity();

        botPlayer.AssignResources(ResourceType.Grain, 1);
        botPlayer.AssignResources(ResourceType.Ore, 1);
        botPlayer.AssignResources(ResourceType.Wool, 2);
        botPlayer.AssignResources(ResourceType.Wood, 3);
        botPlayer.AssignResources(ResourceType.Brick, 2);

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        var discard = bot.DetermineCardsToDiscard();

        // Assert - Make sure bot keeps enough cards to road
        var playerResources = AIHelpers.ConvertResourceDictToList(botPlayer.Resources);
        var resourcesAfterDiscard = AIHelpers.MultiSetSubtraction(playerResources, discard);

        Assert.Equal(4, discard.Count);
        Assert.Contains(ResourceType.Wood, resourcesAfterDiscard);
        Assert.Contains(ResourceType.Brick, resourcesAfterDiscard);
    }

    [Fact]
    public void DetermineCardsToDiscard_WantToBuildSettlement_HaveCardsEvenAfterDiscard()
    {
        // Arrange
        var board = CreateBoardForBuildTest();
        var botPlayer = board.GetBluePlayer();

        // Build road to open up settlement option
        board.GetEdge(TestEdge.E10).BuildRoad(botPlayer);

        botPlayer.AssignResources(ResourceType.Grain, 1);
        botPlayer.AssignResources(ResourceType.Ore, 1);
        botPlayer.AssignResources(ResourceType.Wool, 2);
        botPlayer.AssignResources(ResourceType.Wood, 2);
        botPlayer.AssignResources(ResourceType.Brick, 2);

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        var discard = bot.DetermineCardsToDiscard();

        // Assert - Make sure bot keeps enough cards to road
        var playerResources = AIHelpers.ConvertResourceDictToList(botPlayer.Resources);
        var resourcesAfterDiscard = AIHelpers.MultiSetSubtraction(playerResources, discard);

        Assert.Equal(4, discard.Count);
        Assert.Contains(ResourceType.Wood, resourcesAfterDiscard);
        Assert.Contains(ResourceType.Brick, resourcesAfterDiscard);
        Assert.Contains(ResourceType.Grain, resourcesAfterDiscard);
        Assert.Contains(ResourceType.Wool, resourcesAfterDiscard);
    }

    // TODO: Need more tests for DetermineCardsToDiscard covering all scenarios
    // 1. When multiple potential goals, keep cards that help with both
    // 2. Keep most useful cards to likely future goals.
    // 3. Keep cards that are most valuable in trades.

    [Fact]
    public void ResourcePlayerDoesntNeed_PlayerNeedsAll_ReturnEmptyList()
    {
        var discard = BotAI.ResourcePlayerDoesntNeed(new List<ResourceType>()
        {
            ResourceType.Brick,
            ResourceType.Wood,
            ResourceType.Grain,
            ResourceType.Wool
        }, new GoalWeights(1, 0, 0, 0));

        Assert.Empty(discard);
    }

    [Fact]
    public void ResourcePlayerDoesntNeed_KeepEnoughForCity()
    {
        var discard = BotAI.ResourcePlayerDoesntNeed(new List<ResourceType>()
        {
            ResourceType.Ore,
            ResourceType.Ore,
            ResourceType.Brick,
            ResourceType.Wood,
            ResourceType.Grain,
            ResourceType.Wool,
            ResourceType.Ore,
            ResourceType.Grain,
            ResourceType.Ore,
            ResourceType.Grain
        }, new GoalWeights(0, 1, 0, 0));

        Assert.NotEmpty(discard);
        Assert.Equal(5, discard.Count);
        Assert.Equal(1, discard.Count(r => r == ResourceType.Ore));
        Assert.Equal(1, discard.Count(r => r == ResourceType.Grain));
        Assert.Equal(1, discard.Count(r => r == ResourceType.Brick));
        Assert.Equal(1, discard.Count(r => r == ResourceType.Wood));
        Assert.Equal(1, discard.Count(r => r == ResourceType.Wool));
    }

    [Fact]
    public void ResourcePlayerDoesntNeed_KeepEnoughForRoad()
    {
        var discard = BotAI.ResourcePlayerDoesntNeed(new List<ResourceType>()
        {
            ResourceType.Wood,
            ResourceType.Wood,
            ResourceType.Brick,
            ResourceType.Wood,
            ResourceType.Grain,
            ResourceType.Wool,
            ResourceType.Ore,
            ResourceType.Grain,
        }, new GoalWeights(0, 0, 1, 0));

        Assert.NotEmpty(discard);
        Assert.Equal(6, discard.Count);
        Assert.Equal(1, discard.Count(r => r == ResourceType.Ore));
        Assert.Equal(2, discard.Count(r => r == ResourceType.Grain));
        Assert.Equal(0, discard.Count(r => r == ResourceType.Brick));
        Assert.Equal(2, discard.Count(r => r == ResourceType.Wood));
        Assert.Equal(1, discard.Count(r => r == ResourceType.Wool));
    }

    [Fact]
    public void ResourcePlayerDoesntNeed_KeepEnoughForDevCard()
    {
        var discard = BotAI.ResourcePlayerDoesntNeed(new List<ResourceType>()
        {
            ResourceType.Wood,
            ResourceType.Wood,
            ResourceType.Brick,
            ResourceType.Wood,
            ResourceType.Grain,
            ResourceType.Wool,
            ResourceType.Ore,
            ResourceType.Grain,
        }, new GoalWeights(0, 0, 0, 1));

        Assert.NotEmpty(discard);
        Assert.Equal(5, discard.Count);
        Assert.Equal(0, discard.Count(r => r == ResourceType.Ore));
        Assert.Equal(1, discard.Count(r => r == ResourceType.Grain));
        Assert.Equal(1, discard.Count(r => r == ResourceType.Brick));
        Assert.Equal(3, discard.Count(r => r == ResourceType.Wood));
        Assert.Equal(0, discard.Count(r => r == ResourceType.Wool));
    }

    [Fact]
    public void SelectTargetPlayer_InvalidState()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceRobber, board.GetBluePlayer(), board.GetRedPlayer());

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        Assert.Throws<InvalidOperationException>( () => bot.SelectTargetMove());
    }

    private TestGameBoard CreateTestBoardForSelectTarget()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        board.GetGameState().Phase = new GamePhase(GameStates.SelectTarget, board.GetBluePlayer(), board.GetRedPlayer());
        var orangePlayer = Player.CreateTestPlayer("Tim", PlayerColor.Orange);
        board.GetVertex(TestVertex.V1).BuildSettlement(orangePlayer);
        board.GetGameState().SetRobberTile(board.GetTile(TestTile.T0));
        board.GetGameState().Phase.SetTargetPlayers(new List<Player>() { orangePlayer, board.GetRedPlayer()});

        return board;        
    }

    [Fact]
    public void SelectTargetPlayer_SelectWiningPlayer()
    {
        // Arrange
        var board = CreateTestBoardForSelectTarget();
        board.GetVertex(TestVertex.V16).BuildSettlement(board.GetRedPlayer());
        board.GetGameState().UpdatePlayerVictoryPoints();

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        var move = bot.SelectTargetMove();

        Assert.NotNull(move.SelectedPlayer);
        Assert.Equal(board.GetRedPlayer().Id, move.SelectedPlayer.Id);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
        Assert.Null(move.TileMove);
        Assert.False(move.EndTurn);
        Assert.Null(move.BankTrade);
        Assert.False(move.BuyDevelopmentCard);
        Assert.False(move.RollDice);
        Assert.Null(move.PlayDevelopmentCard);
    }

    [Fact]
    public void SelectTargetPlayer_RandomPlayer()
    {
        // Arrange
        var board = CreateTestBoardForSelectTarget();
        board.GetGameState().UpdatePlayerVictoryPoints();

        var bot = new BotAI(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer!);

        // Act
        var move = bot.SelectTargetMove();

        Assert.NotNull(move.SelectedPlayer);
        Assert.NotEqual(board.GetBluePlayer().Id, move.SelectedPlayer.Id);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.VertexMove);
        Assert.Null(move.TileMove);
        Assert.False(move.EndTurn);
        Assert.Null(move.BankTrade);
        Assert.False(move.BuyDevelopmentCard);
        Assert.False(move.RollDice);
        Assert.Null(move.PlayDevelopmentCard);
    }

    [Fact]
    public void GetBuildMove_PlayRoadBuilding_HasSettlementResourcesButNeedsRoads()
    {
        // Arrange - Bot has settlement resources and Road Building card, but needs roads to reach open vertex
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Give bot settlement resources
        botPlayer.Resources[ResourceType.Brick] = 1;
        botPlayer.Resources[ResourceType.Wood] = 1;
        botPlayer.Resources[ResourceType.Grain] = 1;
        botPlayer.Resources[ResourceType.Wool] = 1;

        // Give bot Road Building card ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.RoadBuilding);

        // Bot has settlement on V3, no roads built yet, so needs roads to reach open vertex
        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetBuildMove();

        // Assert - Should play Road Building to get free roads, saving held resources for settlement
        Assert.Equal(DevelopmentCardType.RoadBuilding, move.PlayDevelopmentCard);
        Assert.Null(move.VertexMove);
        Assert.Null(move.EdgeMove);
        Assert.False(move.BuyDevelopmentCard);
    }

    [Fact]
    public void GetBuildMove_PlayRoadBuilding_NoSettlementResources()
    {
        // Arrange - Bot has Road Building card but NO settlement resources
        // Should still play Road Building to extend road network
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Bot has NO resources (set all to 0)
        foreach (var key in botPlayer.Resources.Keys.ToList())
            botPlayer.Resources[key] = 0;

        // Give bot Road Building card ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.RoadBuilding);

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetBuildMove();

        // Assert - Should play Road Building to extend road network
        Assert.Equal(DevelopmentCardType.RoadBuilding, move.PlayDevelopmentCard);
    }

    [Fact]
    public void GetBuildMove_BuildSettlement_HasResourcesAndOpenVertex()
    {
        // Arrange - Bot has settlement resources AND open vertex available (no roads needed)
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Build road to open up a vertex
        board.GetEdge(TestEdge.E10).BuildRoad(botPlayer);

        // Give bot settlement resources
        botPlayer.Resources[ResourceType.Brick] = 1;
        botPlayer.Resources[ResourceType.Wood] = 1;
        botPlayer.Resources[ResourceType.Grain] = 1;
        botPlayer.Resources[ResourceType.Wool] = 1;

        // Give bot Road Building card (but should NOT use it since open vertex is available)
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.RoadBuilding);

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetBuildMove();

        // Assert - Should build settlement with held resources, not play Road Building
        Assert.NotNull(move.VertexMove);
        Assert.Equal(BuildingType.Settlement, move.VertexMove.Building);
        Assert.Null(move.PlayDevelopmentCard);
    }

    [Fact]
    public void GetBuildMove_BuyDevCard_HasResourcesButCantBuild()
    {
        // Arrange - Bot has dev card resources (ore, grain, wool) but can't build anything useful
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Give bot exactly dev card resources - can't build city, settlement, or road
        botPlayer.Resources[ResourceType.Ore] = 1;
        botPlayer.Resources[ResourceType.Grain] = 1;
        botPlayer.Resources[ResourceType.Wool] = 1;

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetBuildMove();

        // Assert - Should buy a dev card since can't build anything else
        Assert.True(move.BuyDevelopmentCard);
        Assert.Null(move.VertexMove);
        Assert.Null(move.EdgeMove);
        Assert.Null(move.PlayDevelopmentCard);
    }

    [Fact]
    public void GetBuildMove_BuildCity_NotBuyDevCard()
    {
        // Arrange - Bot has resources for both city and dev card, should prioritize city
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Give bot city resources (also covers dev card cost)
        botPlayer.Resources[ResourceType.Ore] = 3;
        botPlayer.Resources[ResourceType.Grain] = 2;
        botPlayer.Resources[ResourceType.Wool] = 1;

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetBuildMove();

        // Assert - Should build city, not buy dev card
        Assert.NotNull(move.VertexMove);
        Assert.Equal(BuildingType.City, move.VertexMove.Building);
        Assert.False(move.BuyDevelopmentCard);
    }

    [Fact]
    public void GameLoop_BotBuysDevCard_ResourcesDeductedAndCardReceived()
    {
        // Arrange - Bot has dev card resources but can't build anything useful
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();

        // Start in BuildOrTrade phase with bot as current player
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Give bot exactly dev card resources - can't build city, settlement, or road
        botPlayer.Resources[ResourceType.Ore] = 1;
        botPlayer.Resources[ResourceType.Grain] = 1;
        botPlayer.Resources[ResourceType.Wool] = 1;

        int initialDevCardCount = botPlayer.DevCardsPurchasedThisRound.Count + botPlayer.DevCardsReadyToPlay.Count;
        int initialDeckSize = gs.DevelopmentCards.Count;

        // Act - Run the game loop (bot should buy dev card then end turn)
        GamePlayHelpers.GameLoop(gs);

        // Assert - Bot should have bought a dev card
        int finalDevCardCount = botPlayer.DevCardsPurchasedThisRound.Count + botPlayer.DevCardsReadyToPlay.Count;
        Assert.Equal(initialDevCardCount + 1, finalDevCardCount);
        Assert.Equal(initialDeckSize - 1, gs.DevelopmentCards.Count);

        // Resources should be spent
        Assert.Equal(0, botPlayer.Resources[ResourceType.Ore]);
        Assert.Equal(0, botPlayer.Resources[ResourceType.Grain]);
        Assert.Equal(0, botPlayer.Resources[ResourceType.Wool]);
    }

    [Fact]
    public void GameLoop_BotPlaysKnight_RobberMovedAndKnightCounted()
    {
        // Arrange - Bot has Knight ready and robber is on bot's tile
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();

        // Start in RollOrUseDevCard phase
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, botPlayer, board.GetRedPlayer());

        // Give bot a Knight ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Knight);

        // Move robber to bot's tile (V3 borders T0, T2, T3)
        gs.SetRobberTile(board.GetTile(TestTile.T0));

        int initialKnightsPlayed = botPlayer.CountPlayedKnights();
        var initialRobberTile = gs.RobberTile;
        gs.Phase.SetWaitingForRoll();

        // Act - Run the game loop (bot should play Knight to move robber)
        GamePlayHelpers.GameLoop(gs);

        // Assert - Knight should have been played
        Assert.Equal(initialKnightsPlayed + 1, botPlayer.CountPlayedKnights());
        Assert.DoesNotContain(botPlayer.DevCardsReadyToPlay, c => c == DevelopmentCardType.Knight);
        // Robber should have moved (no longer on T0)
        Assert.NotEqual(initialRobberTile.Id, gs.RobberTile.Id);
    }

    [Fact]
    public void GetBuildMove_PlayYearOfPlenty_NeedsTwoResourcesForCity()
    {
        // Arrange - Bot has YearOfPlenty card and is 2 resources short of a city
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Bot has 1 Ore, 2 Grain - needs 2 more Ore for city
        botPlayer.Resources[ResourceType.Ore] = 1;
        botPlayer.Resources[ResourceType.Grain] = 2;

        // Give bot Year of Plenty card ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.YearOfPlenty);

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetBuildMove();

        // Assert - Should play Year of Plenty with 2 Ore
        Assert.Equal(DevelopmentCardType.YearOfPlenty, move.PlayDevelopmentCard);
        Assert.NotNull(move.YearOfPlentyResources);
        Assert.Equal(2, move.YearOfPlentyResources.Count);
        Assert.Equal(2, move.YearOfPlentyResources.Count(r => r == ResourceType.Ore));
        Assert.Null(move.VertexMove);
        Assert.False(move.BuyDevelopmentCard);
    }

    [Fact]
    public void GetBuildMove_PlayMonopoly_OpponentHasManyResources()
    {
        // Arrange - Bot has Monopoly card and opponent has 4+ of one resource
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        var opponent = board.GetRedPlayer();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, opponent);

        // Give opponent 5 Ore (triggers Monopoly with score 0.8)
        opponent.Resources[ResourceType.Ore] = 5;

        // Give bot Monopoly card ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Monopoly);

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetBuildMove();

        // Assert - Should play Monopoly targeting Ore
        Assert.Equal(DevelopmentCardType.Monopoly, move.PlayDevelopmentCard);
        Assert.Equal(ResourceType.Ore, move.MonopolyTarget);
        Assert.Null(move.VertexMove);
        Assert.False(move.BuyDevelopmentCard);
    }

    [Fact]
    public void GetBuildMove_PlayKnight_ForLargestArmy_TwoPlayedOneReady()
    {
        // Arrange - Bot has played 2 knights and has 1 more ready (total 3 = Largest Army)
        // Robber is NOT on bot's tile, but bot should still play Knight to get Largest Army
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Bot has already played 2 knights
        botPlayer.DevCardsPlayed.Add(DevelopmentCardType.Knight);
        botPlayer.DevCardsPlayed.Add(DevelopmentCardType.Knight);

        // Give bot another Knight ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Knight);

        // Verify robber is on T6 (desert) - not adjacent to bot's settlement (V3 borders T0, T2, T3)
        Assert.Equal(board.GetTile(TestTile.T6).Id, gs.RobberTile.Id);

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetBuildMove();

        // Assert - Should play Knight to get Largest Army
        Assert.Equal(DevelopmentCardType.Knight, move.PlayDevelopmentCard);
        Assert.NotNull(move.TileMove); // Should pick a target tile for robber
    }

    [Fact]
    public void GetBuildMove_PlayKnight_ForLargestArmy_NonePlayedThreeReady()
    {
        // Arrange - Bot has 0 knights played but 3 knights ready - should start playing them
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Give bot 3 Knights ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Knight);
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Knight);
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Knight);

        // Verify robber is on T6 (desert) - not adjacent to bot's settlement
        Assert.Equal(board.GetTile(TestTile.T6).Id, gs.RobberTile.Id);

        var bai = new BotAI(gs, gs.Phase.CurrentPlayer!);

        // Act
        var move = bai.GetBuildMove();

        // Assert - Should play Knight to work towards Largest Army
        Assert.Equal(DevelopmentCardType.Knight, move.PlayDevelopmentCard);
        Assert.NotNull(move.TileMove);
    }

    [Fact]
    public void GameLoop_BotPlaysYearOfPlenty_ResourcesReceivedAndCityBuilt()
    {
        // Arrange - Bot has YearOfPlenty card and is 2 resources short of a city
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();

        // Start in BuildOrTrade phase
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Bot has 1 Ore, 2 Grain - needs 2 more Ore for city
        botPlayer.Resources[ResourceType.Ore] = 1;
        botPlayer.Resources[ResourceType.Grain] = 2;

        // Give bot Year of Plenty card ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.YearOfPlenty);

        int initialCityCount = gs.CountCitiesForPlayer(botPlayer);

        // Act - Run the game loop (bot should play Year of Plenty then build city)
        GamePlayHelpers.GameLoop(gs);

        // Assert - YoP should have been played and city built
        Assert.DoesNotContain(botPlayer.DevCardsReadyToPlay, c => c == DevelopmentCardType.YearOfPlenty);
        Assert.Equal(initialCityCount + 1, gs.CountCitiesForPlayer(botPlayer));
    }

    [Fact]
    public void GameLoop_BotPlaysMonopoly_ResourcesStolenFromOpponent()
    {
        // Arrange - Bot has Monopoly card and opponent has 5 Ore
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();
        var opponent = board.GetRedPlayer();

        // Start in BuildOrTrade phase
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, opponent);

        // Give opponent 5 Ore
        opponent.Resources[ResourceType.Ore] = 5;

        // Give bot Monopoly card ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Monopoly);

        int initialBotOre = botPlayer.Resources.GetValueOrDefault(ResourceType.Ore, 0);

        // Act - Run the game loop (bot should play Monopoly)
        GamePlayHelpers.GameLoop(gs);

        // Assert - Monopoly should have been played, bot got opponent's Ore
        Assert.DoesNotContain(botPlayer.DevCardsReadyToPlay, c => c == DevelopmentCardType.Monopoly);
        Assert.Equal(initialBotOre + 5, botPlayer.Resources[ResourceType.Ore]);
        Assert.Equal(0, opponent.Resources[ResourceType.Ore]);
    }

    [Fact]
    public void GameLoop_BotPlaysKnight_DuringBuildPhaseForLargestArmy()
    {
        // Arrange - Bot has 3 Knights ready, should play during build phase for Largest Army
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();

        // Start in BuildOrTrade phase
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Give bot 3 Knights ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Knight);
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Knight);
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.Knight);

        // Verify robber starts on T6 (desert) - not on bot's tile
        Assert.Equal(board.GetTile(TestTile.T6).Id, gs.RobberTile.Id);

        int initialKnightsPlayed = botPlayer.CountPlayedKnights();

        // Act - Run the game loop (bot should play Knight for Largest Army)
        GamePlayHelpers.GameLoop(gs);

        // Assert - Knight should have been played and robber moved
        Assert.Equal(initialKnightsPlayed + 1, botPlayer.CountPlayedKnights());
        Assert.NotEqual(board.GetTile(TestTile.T6).Id, gs.RobberTile.Id);
    }

    [Fact]
    public void GameLoop_BotPlaysRoadBuilding_RoadsBuiltAndCardUsed()
    {
        // Arrange - Bot has settlement resources + Road Building, needs roads to reach open vertex
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var botPlayer = board.GetBluePlayer();

        // Start in BuildOrTrade phase
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, board.GetRedPlayer());

        // Give bot settlement resources
        botPlayer.Resources[ResourceType.Brick] = 1;
        botPlayer.Resources[ResourceType.Wood] = 1;
        botPlayer.Resources[ResourceType.Grain] = 1;
        botPlayer.Resources[ResourceType.Wool] = 1;

        // Give bot Road Building card ready to play
        botPlayer.DevCardsReadyToPlay.Add(DevelopmentCardType.RoadBuilding);

        int initialRoadCount = gs.CountRoadsForPlayer(botPlayer);
        int initialSettlementCount = gs.CountSettlementsForPlayer(botPlayer);

        // Act - Run the game loop (bot should play Road Building, build 2 roads, then build settlement)
        GamePlayHelpers.GameLoop(gs);

        // Assert - Road Building should have been played and 2 roads built
        Assert.DoesNotContain(botPlayer.DevCardsReadyToPlay, c => c == DevelopmentCardType.RoadBuilding);
        Assert.Equal(initialRoadCount + 2, gs.CountRoadsForPlayer(botPlayer));
        // Bot should have used settlement resources to build after roads opened up a vertex
        Assert.Equal(initialSettlementCount + 1, gs.CountSettlementsForPlayer(botPlayer));
    }

    // ==================== Bot Trade Response Tests ====================

    [Fact]
    public void OpenTrade_HumanInitiates_BotAcceptsGoodTrade()
    {
        // Arrange - Human offers brick, asks for wood
        // Bot has 2 wood but needs brick to build a road (roads cost 1 brick + 1 wood)
        // This is a good trade for the bot - it gets the brick it needs
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var humanPlayer = board.GetRedPlayer();
        var botPlayer = board.GetBluePlayer();

        // Human is the current player in BuildOrTrade phase
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, humanPlayer, humanPlayer);

        // Human has brick to offer
        humanPlayer.Resources[ResourceType.Brick] = 2;
        // Bot has wood (2) but no brick - needs brick to build a road
        botPlayer.Resources[ResourceType.Wood] = 2;
        botPlayer.Resources[ResourceType.Brick] = 0;

        var offer = new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };

        // Act - Human opens trade
        GamePlayHelpers.OpenTrade(gs, humanPlayer, offer, request);
        GamePlayHelpers.GameLoop(gs);

        // Assert - Bot should have responded to the trade
        Assert.NotNull(gs.Phase.PendingTradeResponses);
        var botResponse = gs.Phase.PendingTradeResponses.FirstOrDefault(r => r.Player.Id == botPlayer.Id);
        Assert.NotNull(botResponse);
        // Bot should accept - getting brick lets it build a road with its wood
        Assert.Equal(TradeResponseType.Accept, botResponse.ResponseType);
    }

    [Fact]
    public void OpenTrade_HumanInitiates_BotRejectsBadTrade()
    {
        // Arrange - Human offers a bad trade (1 wood for 3 ore)
        // This is a terrible deal for the bot
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var humanPlayer = board.GetRedPlayer();
        var botPlayer = board.GetBluePlayer();

        // Human is the current player in BuildOrTrade phase
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, humanPlayer, humanPlayer);

        // Human has wood to offer
        humanPlayer.Resources[ResourceType.Wood] = 1;
        // Bot has ore
        botPlayer.Resources[ResourceType.Ore] = 4;

        var offer = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Ore, 3 } };

        // Act - Human opens trade
        GamePlayHelpers.OpenTrade(gs, humanPlayer, offer, request);
        GamePlayHelpers.GameLoop(gs);

        // Assert - Bot should have responded to the trade
        Assert.NotNull(gs.Phase.PendingTradeResponses);
        var botResponse = gs.Phase.PendingTradeResponses.FirstOrDefault(r => r.Player.Id == botPlayer.Id);
        Assert.NotNull(botResponse);
        // Bot should reject this unfavorable trade
        Assert.Equal(TradeResponseType.Reject, botResponse.ResponseType);
    }

    [Fact]
    public void OpenTrade_HumanInitiates_BotRejectsWhenNoResources()
    {
        // Arrange - Human offers a trade but bot doesn't have the requested resource
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var humanPlayer = board.GetRedPlayer();
        var botPlayer = board.GetBluePlayer();

        // Human is the current player in BuildOrTrade phase
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, humanPlayer, humanPlayer);

        // Human has ore to offer
        humanPlayer.Resources[ResourceType.Ore] = 2;
        // Bot has NO wool (human is requesting wool)
        botPlayer.Resources[ResourceType.Wool] = 0;

        var offer = new Dictionary<ResourceType, int> { { ResourceType.Ore, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wool, 1 } };

        // Act - Human opens trade
        GamePlayHelpers.OpenTrade(gs, humanPlayer, offer, request);
        GamePlayHelpers.GameLoop(gs);

        // Assert - Bot should have responded with rejection (can't fulfill)
        Assert.NotNull(gs.Phase.PendingTradeResponses);
        var botResponse = gs.Phase.PendingTradeResponses.FirstOrDefault(r => r.Player.Id == botPlayer.Id);
        Assert.NotNull(botResponse);
        Assert.Equal(TradeResponseType.Reject, botResponse.ResponseType);
    }

    [Fact]
    public void AcceptTrade_HumanAcceptsBotAcceptance_TradeCompletes()
    {
        // Arrange - Human offers brick for wood, bot accepts, human accepts bot's acceptance
        // Same scenario as OpenTrade_HumanInitiates_BotAcceptsGoodTrade
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var humanPlayer = board.GetRedPlayer();
        var botPlayer = board.GetBluePlayer();

        // Human is the current player in BuildOrTrade phase
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, humanPlayer, humanPlayer);

        // Human has brick to offer
        humanPlayer.Resources[ResourceType.Brick] = 2;
        // Bot has wood but no brick - needs brick to build a road
        botPlayer.Resources[ResourceType.Wood] = 2;
        botPlayer.Resources[ResourceType.Brick] = 0;

        int humanInitialBrick = humanPlayer.Resources[ResourceType.Brick];
        int humanInitialWood = humanPlayer.Resources.GetValueOrDefault(ResourceType.Wood, 0);
        int botInitialBrick = botPlayer.Resources[ResourceType.Brick];
        int botInitialWood = botPlayer.Resources[ResourceType.Wood];

        var offer = new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };

        // Act - Human opens trade, bot should accept (via GameLoop)
        GamePlayHelpers.OpenTrade(gs, humanPlayer, offer, request);
        GamePlayHelpers.GameLoop(gs);

        // Verify bot accepted
        var botResponse = gs.Phase.PendingTradeResponses.FirstOrDefault(r => r.Player.Id == botPlayer.Id);
        Assert.NotNull(botResponse);
        Assert.Equal(TradeResponseType.Accept, botResponse.ResponseType);

        // Human accepts the bot's acceptance
        GamePlayHelpers.AcceptTrade(gs, humanPlayer, botPlayer);

        // Assert - Resources should have been exchanged
        Assert.Equal(humanInitialBrick - 1, humanPlayer.Resources[ResourceType.Brick]);
        Assert.Equal(humanInitialWood + 1, humanPlayer.Resources[ResourceType.Wood]);
        Assert.Equal(botInitialBrick + 1, botPlayer.Resources[ResourceType.Brick]);
        Assert.Equal(botInitialWood - 1, botPlayer.Resources[ResourceType.Wood]);
    }

    // ==================== GetTradeResponse Tests ====================

    [Fact]
    public void GetTradeResponse_GoodTrade_ReturnsAccept()
    {
        // Arrange - Bot has wood but needs brick to build a road
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var bot = board.GetBluePlayer();
        var human = board.GetRedPlayer();

        bot.Resources[ResourceType.Wood] = 2;
        bot.Resources[ResourceType.Brick] = 0;

        // Human is the trade initiator
        gs.Phase = new GamePhase(GameStates.RespondToTrade, human, human);

        var botAI = new BotAI(gs, bot);

        // Human offers brick, requests wood
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };

        // Act
        var response = botAI.GetTradeResponse(offer, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(bot.Id, response.Player.Id);
        Assert.Equal(TradeResponseType.Accept, response.ResponseType);
        Assert.Null(response.Offer);
        Assert.Null(response.Request);
    }

    [Fact]
    public void GetTradeResponse_BadTrade_ReturnsReject()
    {
        // Arrange - Human offers terrible trade (1 wood for 3 ore)
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var bot = board.GetBluePlayer();
        var human = board.GetRedPlayer();

        bot.Resources[ResourceType.Ore] = 4;

        // Human is the trade initiator
        gs.Phase = new GamePhase(GameStates.RespondToTrade, human, human);

        var botAI = new BotAI(gs, bot);

        // Human offers wood, requests 3 ore
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Ore, 3 } };

        // Act
        var response = botAI.GetTradeResponse(offer, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(bot.Id, response.Player.Id);
        Assert.Equal(TradeResponseType.Reject, response.ResponseType);
    }

    [Fact]
    public void GetTradeResponse_CantFulfill_ReturnsReject()
    {
        // Arrange - Bot doesn't have the requested resource
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var bot = board.GetBluePlayer();
        var human = board.GetRedPlayer();

        bot.Resources[ResourceType.Wool] = 0;

        // Human is the trade initiator
        gs.Phase = new GamePhase(GameStates.RespondToTrade, human, human);

        var botAI = new BotAI(gs, bot);

        // Human offers brick, requests wool (bot has none)
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wool, 1 } };

        // Act
        var response = botAI.GetTradeResponse(offer, request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(TradeResponseType.Reject, response.ResponseType);
    }

}
