using Xunit;
using GameTest.Models;
using GameTest.Services;

namespace GameTest.Tests;

public class PossiblePlayerActionTests
{
    [Fact]
    public void GetPossiblePlayerActions_SettingUpBoard()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        board.GetGameState().Phase = new GamePhase(GameStates.SettingUpBoard);

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetBluePlayer());

        Assert.NotNull(actions);
        Assert.Empty(actions);
    }

    [Fact]
    public void GetPossiblePlayerActions_GameOver()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        board.GetGameState().Phase = new GamePhase(GameStates.SettingUpBoard);

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetBluePlayer());

        Assert.NotNull(actions);
        Assert.Empty(actions);
    }

    [Fact]
    public void GetPossiblePlayerActions_PlaceFirstSettlement_EmptyBoard()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.PlaceSettlement, action.Action);
        Assert.Null(action.PlayerIds);
        Assert.Null(action.TileIds);
        Assert.Null(action.EdgeIds);
        Assert.NotNull(action.VertexIds);
        Assert.Equal(board.GetGameState().Vertices.Count, action.VertexIds.Count);
    }

    [Fact]
    public void GetPossiblePlayerActions_PlaceFirstSettlement_BotMoveEmptyBoard()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetBluePlayer(), board.GetRedPlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetBluePlayer());

        Assert.NotNull(actions);
        Assert.Empty(actions);
    }

    [Fact]
    public void GetPossiblePlayerActions_PlaceFirstSettlement_OneVertexOccupied()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        var v23 = board.GetVertex(TestVertex.V23);
        v23.BuildSettlement(board.GetBluePlayer());
        GamePlayHelpers.MarkBlockedVertices(gs, v23);
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(gs, board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.PlaceSettlement, action.Action);
        Assert.NotNull(action.VertexIds);
        Assert.Equal(board.GetGameState().Vertices.Count - 3, action.VertexIds.Count); // one settlement blocking 2 other vertices
        Assert.DoesNotContain(v23.Id, action.VertexIds);
        Assert.DoesNotContain(board.GetVertex(TestVertex.V24).Id, action.VertexIds);
        Assert.DoesNotContain(board.GetVertex(TestVertex.V22).Id, action.VertexIds);
    }

    [Fact]
    public void GetPossiblePlayerActions_PlaceFirstRoad()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        var v3 = board.GetVertex(TestVertex.V3);
        v3.BuildSettlement(board.GetRedPlayer());
        GamePlayHelpers.MarkBlockedVertices(gs, v3);
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(gs, board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.PlaceRoad, action.Action);
        Assert.Null(action.VertexIds);
        Assert.Null(action.TileIds);
        Assert.Null(action.PlayerIds);
        Assert.NotNull(action.EdgeIds);
        Assert.Equal(3, action.EdgeIds.Count);
        Assert.Contains(board.GetEdge(TestEdge.E2).Id, action.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E3).Id, action.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E9).Id, action.EdgeIds);
    }

    [Fact]
    public void GetPossiblePlayerActions_PlaceSecondSettlement()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.PlaceSettlement, action.Action);
        Assert.NotNull(action.VertexIds);
        Assert.Equal(board.GetGameState().Vertices.Count - 7, action.VertexIds.Count);
        Assert.DoesNotContain(board.GetVertex(TestVertex.V5).Id, action.VertexIds);
        Assert.DoesNotContain(board.GetVertex(TestVertex.V3).Id, action.VertexIds);
        Assert.DoesNotContain(board.GetVertex(TestVertex.V19).Id, action.VertexIds);
        Assert.DoesNotContain(board.GetVertex(TestVertex.V6).Id, action.VertexIds);
        Assert.DoesNotContain(board.GetVertex(TestVertex.V4).Id, action.VertexIds);
        Assert.DoesNotContain(board.GetVertex(TestVertex.V2).Id, action.VertexIds);
        Assert.DoesNotContain(board.GetVertex(TestVertex.V13).Id, action.VertexIds);
        Assert.Null(action.TileIds);
        Assert.Null(action.PlayerIds);
        Assert.Null(action.EdgeIds);
    }

    [Fact]
    public void GetPossiblePlayerActions_PlaceSecondRoad()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var v1 = board.GetVertex(TestVertex.V1);
        v1.BuildSettlement(board.GetRedPlayer());
        GamePlayHelpers.MarkBlockedVertices(gs, v1);
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondRoad, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(gs, board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.PlaceRoad, action.Action);
        Assert.Null(action.VertexIds);
        Assert.Null(action.TileIds);
        Assert.Null(action.PlayerIds);
        Assert.NotNull(action.EdgeIds);
        Assert.Equal(3, action.EdgeIds.Count);
        Assert.Contains(board.GetEdge(TestEdge.E7).Id, action.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E6).Id, action.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E1).Id, action.EdgeIds);
    }

    [Fact]
    public void GetPossiblePlayerActions_PlaceRobber()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var t0 = board.GetTile(TestTile.T0);
        gs.SetRobberTile(t0);
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(gs, board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.PlaceRobber, action.Action);
        Assert.NotNull(action.TileIds);
        Assert.Equal(6, action.TileIds.Count);
        Assert.DoesNotContain(t0.Id, action.TileIds);
        Assert.Null(action.VertexIds);
        Assert.Null(action.PlayerIds);
        Assert.Null(action.EdgeIds);
    }

    [Fact]
    public void GetPossiblePlayerActions_PlaceRobber_NoActionsIfNotPlayersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var t0 = board.GetTile(TestTile.T0);
        gs.SetRobberTile(t0);
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(gs, board.GetBluePlayer());

        Assert.NotNull(actions);
        Assert.Empty(actions);
    }

    [Fact]
    public void GetPossiblePlayerActions_StealRobber()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var orangePlayer = Player.CreateTestPlayer("Tim", PlayerColor.Orange);
        board.GetVertex(TestVertex.V1).BuildSettlement(orangePlayer);
        board.GetGameState().Phase = new GamePhase(GameStates.SelectTarget, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetStateToReturnTo(GameStates.BuildOrTrade, board.GetTile(TestTile.T6));
        board.GetGameState().Phase.SetTargetPlayers(new List<Player>() {orangePlayer, board.GetBluePlayer()});

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.SelectTarget, action.Action);
        Assert.Null(action.TileIds);
        Assert.Null(action.VertexIds);
        Assert.NotNull(action.PlayerIds);
        Assert.Equal(2, action.PlayerIds.Count);
        Assert.Contains(orangePlayer.Id, action.PlayerIds);
        Assert.Contains(board.GetBluePlayer().Id, action.PlayerIds);
        Assert.Null(action.EdgeIds);
    }

    [Fact]
    public void GetPossiblePlayerActions_DiscardCards()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        board.GetGameState().Phase = new GamePhase(GameStates.DiscardCards, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.DiscardCards, action.Action);
        Assert.Null(action.TileIds);
        Assert.Null(action.VertexIds);
        Assert.Null(action.PlayerIds);
        Assert.Null(action.EdgeIds);
    }

    private TestGameBoard CreatePlayerActionTestBoard1()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        board.GetVertex(TestVertex.V20).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E25).BuildRoad(board.GetRedPlayer());
        board.GetVertex(TestVertex.V1).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E7).BuildRoad(board.GetBluePlayer());
        GamePlayHelpers.MarkBlockedVertices(board.GetGameState());

        return board;
    }

    [Fact]
    public void GetPossiblePlayerActions_FirstDevCardRoad()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        board.GetGameState().Phase = new GamePhase(GameStates.FirstDevCardRoad, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(gs, board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.PlaceRoad, action.Action);
        Assert.Null(action.VertexIds);
        Assert.Null(action.TileIds);
        Assert.Null(action.PlayerIds);
        Assert.NotNull(action.EdgeIds);
        Assert.Equal(4, action.EdgeIds.Count);
        Assert.Contains(board.GetEdge(TestEdge.E5).Id, action.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E4).Id, action.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E24).Id, action.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E26).Id, action.EdgeIds);
    }

    [Fact]
    public void GetPossiblePlayerActions_SecondDevCardRoad()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        board.GetEdge(TestEdge.E5).BuildRoad(board.GetRedPlayer());
        board.GetGameState().Phase = new GamePhase(GameStates.SecondDevCardRoad, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(gs, board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.PlaceRoad, action.Action);
        Assert.Null(action.VertexIds);
        Assert.Null(action.TileIds);
        Assert.Null(action.PlayerIds);
        Assert.NotNull(action.EdgeIds);
        Assert.Equal(5, action.EdgeIds.Count);
        Assert.Contains(board.GetEdge(TestEdge.E6).Id, action.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E12).Id, action.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E4).Id, action.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E24).Id, action.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E26).Id, action.EdgeIds);
    }

    // NOTE: Not testing what happens if the player doesn't have roads left or the player
    // doesn't have any place to put roads in FirstDevCardRoad and SecondDevCardRoad. 
    // The game shouldn't put that player in these states in those cases.

    private TestGameBoard CreatePlayerActionTestBoard2(bool bluePlayerIsBot = false)
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(bluePlayerIsBot);
        var human = board.GetRedPlayer();
        human.AssignResources(ResourceType.Wood, 2);
        board.GetGameState().Phase = new GamePhase(GameStates.RespondToTrade, human, board.GetBluePlayer());
        board.GetGameState().Phase.AddPendingTradeResponse(new TradeResponse(human, TradeResponseType.Original, 
            new Dictionary<ResourceType, int>() {{ResourceType.Wood, 1}},
            new Dictionary<ResourceType, int>() {{ResourceType.Brick, 1}}));

        return board;
    }

    [Fact]
    public void GetPossiblePlayerActions_RespondToTrade_OriginalNoResponses()
    {
        var board = CreatePlayerActionTestBoard2();

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.RejectAllOffers, action.Action);
        Assert.Null(action.VertexIds);
        Assert.Null(action.TileIds);
        Assert.Null(action.PlayerIds);
        Assert.Null(action.EdgeIds);
    }

    [Fact]
    public void GetPossiblePlayerActions_RespondToTrade_OriginalNoAccepts()
    {
        var board = CreatePlayerActionTestBoard2();
        board.GetGameState().Phase.AddPendingTradeResponse(new TradeResponse(board.GetBluePlayer(),
                TradeResponseType.Reject, null!, null!));

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.RejectAllOffers, action.Action);
        Assert.Null(action.VertexIds);
        Assert.Null(action.TileIds);
        Assert.Null(action.PlayerIds);
        Assert.Null(action.EdgeIds);
    }

    [Fact]
    public void GetPossiblePlayerActions_RespondToTrade_OriginalAccepts()
    {
        var board = CreatePlayerActionTestBoard2();
        board.GetGameState().Phase.AddPendingTradeResponse(new TradeResponse(board.GetBluePlayer(),
                TradeResponseType.Accept, null!, null!));

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var rejectAction = actions.FirstOrDefault(a => a.Action == PlayerAction.RejectAllOffers);
        Assert.NotNull(rejectAction);
        var acceptAction = actions.FirstOrDefault(a => a.Action == PlayerAction.AcceptTrade);
        Assert.NotNull(acceptAction);
        Assert.NotNull(acceptAction.PlayerIds);
        Assert.Single(acceptAction.PlayerIds);
        Assert.Contains(board.GetBluePlayer().Id, acceptAction.PlayerIds);
    }

    [Fact]
    public void GetPossiblePlayerActions_RespondToTrade_ResponderIsHuman()
    {
        var board = CreatePlayerActionTestBoard2();

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetBluePlayer());

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.RespondToTrade, action.Action);
    }

    [Fact]
    public void GetPossiblePlayerActions_RespondToTrade_ResponderIsBot()
    {
        var board = CreatePlayerActionTestBoard2(true);

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetBluePlayer());

        Assert.NotNull(actions);
        Assert.Empty(actions);
    }

    [Fact]
    public void GetPossiblePlayerActions_RollOrUseDevCard_NoDevCards()
    {
        var board = CreatePlayerActionTestBoard2(true);
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.Single(actions);
        var action = actions.First();
        Assert.Equal(PlayerAction.RollDice, action.Action);
    }

    [Fact]
    public void GetPossiblePlayerActions_RollOrUseDevCard_DevCardAlreadyPlayed()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        human.AssignDevelopmentCard(DevelopmentCardType.Knight);
        human.AssignDevelopmentCard(DevelopmentCardType.Knight);
        human.MakeNewDevelopmentCardsPlayable();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, human, board.GetBluePlayer());
        GamePlayHelpers.PlayKnightDevCard(gs, human, board.GetTile(TestTile.T2));

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.Equal(2, actions.Count);
        Assert.Contains(actions, a => a.Action == PlayerAction.RollDice);
        Assert.Contains(actions, a => a.Action == PlayerAction.Undo);
        var action = actions.First(a => a.Action == PlayerAction.RollDice);
        Assert.Equal(PlayerAction.RollDice, action.Action);
        Assert.Null(action.EdgeIds);
        Assert.Null(action.VertexIds);
        Assert.Null(action.PlayerIds);
        Assert.Null(action.TileIds);
    }

    [Fact]
    public void GetPossiblePlayerActions_RollOrUseDevCard_AllDevCards()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        human.AssignDevelopmentCard(DevelopmentCardType.Knight);
        human.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        human.AssignDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        human.AssignDevelopmentCard(DevelopmentCardType.Monopoly);
        human.AssignDevelopmentCard(DevelopmentCardType.RoadBuilding);
        human.MakeNewDevelopmentCardsPlayable();
        var t1 = board.GetTile(TestTile.T1);
        gs.SetRobberTile(t1);
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.Equal(5, actions.Count);
        var rollAction = actions.FirstOrDefault(a => a.Action == PlayerAction.RollDice);
        Assert.NotNull(rollAction);
        var knightAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlayKnight);
        Assert.NotNull(knightAction);
        Assert.NotNull(knightAction.TileIds);
        Assert.Equal(6, knightAction.TileIds.Count);
        Assert.DoesNotContain(t1.Id, knightAction.TileIds);
        var yopAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlayYearOfPlenty);
        Assert.NotNull(yopAction);
        var monopolyAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlayMonopoly);
        Assert.NotNull(monopolyAction);
        var roadAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlayRoadBuilding);
        Assert.NotNull(roadAction);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_AllButMonopoly()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        human.AssignDevelopmentCard(DevelopmentCardType.Knight);
        human.AssignDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        human.AssignDevelopmentCard(DevelopmentCardType.RoadBuilding);
        human.MakeNewDevelopmentCardsPlayable();
        var t1 = board.GetTile(TestTile.T1);
        gs.SetRobberTile(t1);
        board.GetEdge(TestEdge.E24).BuildRoad(human);
        human.AssignResources(ResourceType.Wood, 1);
        human.AssignResources(ResourceType.Brick, 1);
        human.AssignResources(ResourceType.Ore, 4);
        human.AssignResources(ResourceType.Grain, 2);
        human.AssignResources(ResourceType.Wool, 1);
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.Equal(10, actions.Count);
        var knightAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlayKnight);
        Assert.NotNull(knightAction);
        Assert.NotNull(knightAction.TileIds);
        Assert.Equal(6, knightAction.TileIds.Count);
        Assert.DoesNotContain(t1.Id, knightAction.TileIds);
        Assert.Contains(actions, a => a.Action == PlayerAction.PlayYearOfPlenty);
        Assert.DoesNotContain(actions, a => a.Action == PlayerAction.PlayMonopoly);
        Assert.Contains(actions, a => a.Action == PlayerAction.PlayRoadBuilding);
        Assert.Contains(actions, a => a.Action == PlayerAction.EndTurn);
        var roadAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlaceRoad);
        Assert.NotNull(roadAction);
        Assert.NotNull(roadAction.EdgeIds);
        Assert.Equal(4, roadAction.EdgeIds.Count);
        Assert.Contains(board.GetEdge(TestEdge.E26).Id, roadAction.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E5).Id, roadAction.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E4).Id, roadAction.EdgeIds);
        Assert.Contains(board.GetEdge(TestEdge.E23).Id, roadAction.EdgeIds);
        var settlementAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlaceSettlement);
        Assert.NotNull(settlementAction);
        Assert.NotNull(settlementAction.VertexIds);
        Assert.Single(settlementAction.VertexIds);
        Assert.Contains(board.GetVertex(TestVertex.V18).Id, settlementAction.VertexIds);
        var cityAction = actions.FirstOrDefault(a => a.Action == PlayerAction.UpgradeSettlement);
        Assert.NotNull(cityAction);
        Assert.NotNull(cityAction.VertexIds);
        Assert.Equal(2, cityAction.VertexIds.Count);
        Assert.Contains(board.GetVertex(TestVertex.V5).Id, cityAction.VertexIds);
        Assert.Contains(board.GetVertex(TestVertex.V20).Id, cityAction.VertexIds);
        Assert.Contains(actions, a => a.Action == PlayerAction.BuyDevelopmentCard);
        Assert.Contains(actions, a => a.Action == PlayerAction.TradeWithBank);
        Assert.Contains(actions, a => a.Action == PlayerAction.TradeWithPlayers);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoRoadLackingResources()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        human.AssignResources(ResourceType.Brick, 1);
        human.AssignResources(ResourceType.Ore, 3);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var roadAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlaceRoad);
        Assert.Null(roadAction);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoRoadUsedAll()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        board.GetEdge(TestEdge.E24).BuildRoad(human);
        board.GetEdge(TestEdge.E26).BuildRoad(human);
        board.GetEdge(TestEdge.E27).BuildRoad(human);
        board.GetEdge(TestEdge.E4).BuildRoad(human);

        human.AssignResources(ResourceType.Brick, 1);
        human.AssignResources(ResourceType.Wood, 2);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var roadAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlaceRoad);
        Assert.Null(roadAction);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoRoadAsNoSpot()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        var bot = board.GetBluePlayer();

        board.GetVertex(TestVertex.V22).BuildSettlement(bot);
        board.GetEdge(TestEdge.E12).BuildRoad(bot);
        board.GetEdge(TestEdge.E5).BuildRoad(bot);
        board.GetVertex(TestVertex.V18).BuildSettlement(bot);
        board.GetEdge(TestEdge.E24).BuildRoad(bot);
        board.GetEdge(TestEdge.E4).BuildRoad(bot);

        board.GetVertex(TestVertex.V20).BuildSettlement(human);
        board.GetEdge(TestEdge.E25).BuildRoad(human);
        board.GetEdge(TestEdge.E26).BuildRoad(human);
        board.GetEdge(TestEdge.E27).BuildRoad(human);

        human.AssignResources(ResourceType.Brick, 1);
        human.AssignResources(ResourceType.Wood, 2);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var roadAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlaceRoad);
        Assert.Null(roadAction);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoSettlementLackingResources()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        board.GetEdge(TestEdge.E24).BuildRoad(human);

        human.AssignResources(ResourceType.Brick, 1);
        human.AssignResources(ResourceType.Wood, 2);
        human.AssignResources(ResourceType.Grain, 2);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var settlementAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlaceSettlement);
        Assert.Null(settlementAction);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoSettlementUsedAll()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        board.GetEdge(TestEdge.E24).BuildRoad(human);
        board.GetVertex(TestVertex.V18).BuildSettlement(human);
        board.GetEdge(TestEdge.E23).BuildRoad(human);
        board.GetEdge(TestEdge.E22).BuildRoad(human);

        human.AssignResources(ResourceType.Brick, 1);
        human.AssignResources(ResourceType.Wood, 2);
        human.AssignResources(ResourceType.Grain, 2);
        human.AssignResources(ResourceType.Wool, 1);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var settlementAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlaceSettlement);
        Assert.Null(settlementAction);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoSettlementAsNoSpot()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();

        human.AssignResources(ResourceType.Brick, 1);
        human.AssignResources(ResourceType.Wood, 2);
        human.AssignResources(ResourceType.Grain, 2);
        human.AssignResources(ResourceType.Wool, 1);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var settlementAction = actions.FirstOrDefault(a => a.Action == PlayerAction.PlaceSettlement);
        Assert.Null(settlementAction);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoCityLackingResources()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();

        human.AssignResources(ResourceType.Grain, 2);
        human.AssignResources(ResourceType.Ore, 2);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var cityAction = actions.FirstOrDefault(a => a.Action == PlayerAction.UpgradeSettlement);
        Assert.Null(cityAction);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoCityUsedAll()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        board.GetEdge(TestEdge.E24).BuildRoad(human);
        board.GetVertex(TestVertex.V18).BuildSettlement(human);
        board.GetVertex(TestVertex.V5).UpgradeToCity();
        board.GetVertex(TestVertex.V20).UpgradeToCity();

        human.AssignResources(ResourceType.Grain, 2);
        human.AssignResources(ResourceType.Ore, 3);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var cityAction = actions.FirstOrDefault(a => a.Action == PlayerAction.UpgradeSettlement);
        Assert.Null(cityAction);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoCityAsNoSpot()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        board.GetVertex(TestVertex.V5).UpgradeToCity();
        board.GetVertex(TestVertex.V20).UpgradeToCity();

        human.AssignResources(ResourceType.Grain, 2);
        human.AssignResources(ResourceType.Ore, 3);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var cityAction = actions.FirstOrDefault(a => a.Action == PlayerAction.UpgradeSettlement);
        Assert.Null(cityAction);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoTradeAsNoResources()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        Assert.DoesNotContain(actions, a => a.Action == PlayerAction.TradeWithBank);
        Assert.DoesNotContain(actions, a => a.Action == PlayerAction.TradeWithPlayers);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoBankTradeNoPorts()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        human.AssignResources(ResourceType.Wood, 3);
        human.AssignResources(ResourceType.Brick, 3);
        human.AssignResources(ResourceType.Grain, 3);
        human.AssignResources(ResourceType.Wool, 3);
        human.AssignResources(ResourceType.Ore, 3);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        Assert.DoesNotContain(actions, a => a.Action == PlayerAction.TradeWithBank);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoBankTradeOnly31Port()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        board.GetVertex(TestVertex.V14).BuildSettlement(human);
        GamePlayHelpers.PopulatePlayerPorts(gs);
        human.AssignResources(ResourceType.Wood, 2);
        human.AssignResources(ResourceType.Brick, 2);
        human.AssignResources(ResourceType.Grain, 2);
        human.AssignResources(ResourceType.Wool, 2);
        human.AssignResources(ResourceType.Ore, 2);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        Assert.DoesNotContain(actions, a => a.Action == PlayerAction.TradeWithBank);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoBankTradeOnlyBrick21Port()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        board.GetVertex(TestVertex.V10).BuildSettlement(human);
        GamePlayHelpers.PopulatePlayerPorts(gs);
        human.AssignResources(ResourceType.Wood, 3);
        human.AssignResources(ResourceType.Brick, 1);
        human.AssignResources(ResourceType.Grain, 3);
        human.AssignResources(ResourceType.Wool, 3);
        human.AssignResources(ResourceType.Ore, 3);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        Assert.DoesNotContain(actions, a => a.Action == PlayerAction.TradeWithBank);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_BankTradeBrick21Port()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();
        board.GetVertex(TestVertex.V10).BuildSettlement(human);
        GamePlayHelpers.PopulatePlayerPorts(gs);
        human.AssignResources(ResourceType.Wood, 3);
        human.AssignResources(ResourceType.Brick, 2);
        human.AssignResources(ResourceType.Grain, 3);
        human.AssignResources(ResourceType.Wool, 3);
        human.AssignResources(ResourceType.Ore, 3);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        Assert.Contains(actions, a => a.Action == PlayerAction.TradeWithBank);
    }

    [Fact]
    public void GetPossiblePlayerActions_BuildOrTrade_NoDevCardLackingResources()
    {
        var board = CreatePlayerActionTestBoard1();
        var gs = board.GetGameState();
        var human = board.GetRedPlayer();

        human.AssignResources(ResourceType.Grain, 1);
        human.AssignResources(ResourceType.Ore, 2);

        gs.Phase = new GamePhase(GameStates.BuildOrTrade, human, board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), human);

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        var buyDCAction = actions.FirstOrDefault(a => a.Action == PlayerAction.BuyDevelopmentCard);
        Assert.Null(buyDCAction);
    }

    [Fact]
    public void GetPossiblePlayerActions_Undo_InvalidDuringRespondToTrade()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.RespondToTrade, board.GetRedPlayer(), board.GetBluePlayer());

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        Assert.DoesNotContain(actions, a => a.Action == PlayerAction.Undo);
    }

    [Fact]
    public void GetPossiblePlayerActions_Undo_FirstSettlementLastPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        var gs = board.GetGameState();
        gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceFirstSettlement, board.GetVertex(TestVertex.V3)));
        gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E3)));
        gs.Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetRedPlayer());
        var response = GamePlayHelpers.BuildSettlementRequestFromUser(gs, board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Id);
        Assert.True(response.Success);
        Assert.Equal(GameStates.PlaceFirstRoad, gs.Phase.PhaseState);
        
        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        Assert.Contains(actions, a => a.Action == PlayerAction.Undo);
    }

    [Fact]
    public void GetPossiblePlayerActions_Undo_FirstRoadLastPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard(false);
        var gs = board.GetGameState();
        gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceFirstSettlement, board.GetVertex(TestVertex.V3)));
        gs.AddEventRecord(new EventRecordDTO(board.GetBluePlayer(), EventRecordAction.PlaceRoad, board.GetEdge(TestEdge.E3)));
        board.GetVertex(TestVertex.V5).BuildSettlement(board.GetRedPlayer());
        gs.AddEventRecord(new EventRecordDTO(board.GetRedPlayer(), EventRecordAction.PlaceFirstSettlement, board.GetVertex(TestVertex.V5)));
        gs.Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetRedPlayer());
        var response = GamePlayHelpers.BuildRoadRequestFromUser(gs, board.GetRedPlayer().Id, board.GetEdge(TestEdge.E11).Id);
        Assert.True(response.Success);
        Assert.Equal(GameStates.PlaceSecondSettlement, gs.Phase.PhaseState);

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        Assert.Contains(actions, a => a.Action == PlayerAction.Undo);
    }

    [Fact]
    public void GetPossiblePlayerActions_Undo_FirstRoadFirstPlayer()
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

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetRedPlayer());

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        Assert.Contains(actions, a => a.Action == PlayerAction.Undo);
    }

    [Fact]
    public void GetPossiblePlayerActions_Undo_SecondRoadSecondPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
         board.GetVertex(TestVertex.V5).BuildSettlement(board.GetRedPlayer());
         board.GetEdge(TestEdge.E11).BuildRoad(board.GetRedPlayer());
         board.GetVertex(TestVertex.V3).BuildSettlement(board.GetBluePlayer());
         board.GetEdge(TestEdge.E3).BuildRoad(board.GetBluePlayer());
         board.GetVertex(TestVertex.V10).BuildSettlement(board.GetBluePlayer());
        gs.Phase = new GamePhase(GameStates.PlaceSecondRoad, board.GetBluePlayer(), board.GetRedPlayer());
        var buildResponse = GamePlayHelpers.BuildRoadRequestFromUser(gs, board.GetBluePlayer().Id, board.GetEdge(TestEdge.E15).Id);
        Assert.True(buildResponse.Success);
        Assert.Equal(GameStates.PlaceSecondSettlement, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.EndPlayer.Id);

        var actions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetBluePlayer());

        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        Assert.Contains(actions, a => a.Action == PlayerAction.Undo);
    }

}
