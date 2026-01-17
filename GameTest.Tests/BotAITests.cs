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
            new BotAI(null!));

        Assert.Equal("Value cannot be null. (Parameter 'gs')", exception.Message);
    }

    [Fact]
    public void Constructor_CurrentPlayerMissingOrNotBot()
    {
        // Arrange
        var gs = new GameState(new Guid(), "UT");

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
    }

    [Fact]
    public void GetSetUpMove_FirstSettlement()
    {
        // Arrange
        GameState gs = CreateBoardForSetupTest(GameStates.PlaceFirstSettlement);
        var bai = new BotAI(gs);

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
        var bai = new BotAI(gs);

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

        var bai = new BotAI(gs);

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

        var bai = new BotAI(gs);

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
        Assert.Null(move.SelectedPlayer);
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

    [Fact]
    public void GetBuildMove_BuildRoad()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var botPlayer = board.GetBluePlayer();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, botPlayer, GetHumanPlayer(gs));

        botPlayer.Resources[ResourceType.Brick] = 1;
        botPlayer.Resources[ResourceType.Wood] = 1;

        var bai = new BotAI(gs);

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

        var bai = new BotAI(gs);

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

        var bai = new BotAI(gs);

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

        var bai = new BotAI(gs);

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

        var bot = new BotAI(board.GetGameState());

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

        var bot = new BotAI(board.GetGameState());

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

        var bot = new BotAI(board.GetGameState());

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

        var bot = new BotAI(board.GetGameState());

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

        var bot = new BotAI(board.GetGameState());

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

        var bot = new BotAI(board.GetGameState());

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

        var bot = new BotAI(board.GetGameState());

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

        var bot = new BotAI(board.GetGameState());

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

        var bot = new BotAI(board.GetGameState());

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

        var bot = new BotAI(board.GetGameState());

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

        var bot = new BotAI(board.GetGameState());

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

        var bot = new BotAI(board.GetGameState());

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
    public void BuyDevCard_TESTS_TBD()
    {
        Assert.True(false);
    }

}